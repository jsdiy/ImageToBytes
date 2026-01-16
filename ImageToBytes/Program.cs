/*
【コマンドラインオプションを指定してデバッグ実行する方法】
Visual Studio 2026では、プロジェクトのプロパティ（[デバッグ] ＞ [デバッグ起動プロファイル UI を開く]）から、
実行時に渡す引数を設定してテストすることができる。 

【System.Drawingの利用時の注意】
Bitmapなど画像系クラスを利用するにはNuGetパッケージ「System.Drwing.Common」をインストールする。
ただしWindows以外のOSでは動作しないので、ターゲットOSをWindowsに限定する必要がある。
→メニュー[プロジェクト]-[<プロジェクト名>のプロパティ]で、「ターゲットOS」に'Windows'を指定する。
*/
using System.Drawing;

//更新履歴
const string AppVer = "v1.0";    //2026/01 初版

//引数なしの起動で使用方法を表示
if (args.Length == 0)
{
	Console.WriteLine($"ImageToBytes {AppVer} (c)2026 @jsdiy");
	Usage();
	return;
}

//オプション解析
const Int32 FormatDefault = 888;
const Int32 LinePixelDefault = 8;

GetOptionParam(out string imgFile, out Int32 format, out bool toBGR, out bool lowByteFirst, out Int32 linePixel);
if (string.IsNullOrEmpty(imgFile)) { Console.WriteLine("画像ファイルを指定してください."); Usage(); return; }
if (linePixel < 1) { linePixel = LinePixelDefault; }
_ = toBGR;  //未使用警告回避
_ = lowByteFirst;	//未使用警告回避

//画像読み込み
using Bitmap? bmp = LoadImage(imgFile);
if (bmp == null) { Console.WriteLine(@"画像ファイルが読み込めませんでした."); return; }
Int32 TotalPixelCount = bmp.Width * bmp.Height;

//変換処理の選択
Func<Color, bool> FnOutputColorByte;
Int32 fmt444FuncCallCount = 0;
Color fmt444FirstColor = Color.Empty;
Int32 fmtByteLength = 0;

switch (format)
{
case 888: FnOutputColorByte = OutputColorByte888; fmtByteLength = TotalPixelCount * 3; break;
case 666: FnOutputColorByte = OutputColorByte666; fmtByteLength = TotalPixelCount * 3; break;
case 565: FnOutputColorByte = OutputColorByte565; fmtByteLength = TotalPixelCount * 2; break;
case 555: FnOutputColorByte = OutputColorByte555; fmtByteLength = TotalPixelCount * 2; break;
case 444: FnOutputColorByte = OutputColorByte444; fmtByteLength = TotalPixelCount / 2 * 3; break;
default: Console.WriteLine(@"出力フォーマットの指定が違います."); Usage(); return;
}

//タイトルを出力
Console.WriteLine($"//{Path.GetFileNameWithoutExtension(imgFile)} - {bmp.Width}x{bmp.Height} - " +
	$"{(toBGR ? "BGR" : "RGB")}:{format} ({fmtByteLength:N0} bytes)");

//変換データを出力
Int32 pixelCount = 0;		//読み込んだ画素数
Int32 linePixelCount = 0;	//1行に出力した画素数（出力バイト数ではない）

for (Int32 y = 0; y < bmp.Height; y++)
{
	for (Int32 x = 0; x < bmp.Width; x++)
	{
		Color color = bmp.GetPixel(x, y);
		pixelCount++;
		linePixelCount++;
		bool isOutput = FnOutputColorByte(color);   //444形式の場合、true/falseを交互に返してくる
		if (!isOutput) { continue; }	//出力がなかった場合、カンマ処理や改行処理は考えなくてよい

		if (pixelCount < TotalPixelCount) { Console.Write(","); }
		if (linePixelCount == linePixel) { Console.WriteLine(); linePixelCount = 0; }
	}
}

//End of Main logic

//使用方法
void Usage()
{
	string mes = @"書式 : ImageToBytes.exe <画像ファイル> [-f<n>] [-b<n>] [-bgr] [-l]
		オプション
		<画像ファイル> : 変換元の画像ファイル（.jpg/.png/.bmpなど）のフルパス.
		-f<n> : 出力フォーマット.　省略時は'-f{0}'.
		        888 = RGB:888, 666 = RGB:666, 565 = RGB:565, 555 = RGB:555, 444 = RGB:444
		-b<n> : 出力を何画素分ごとに改行するか(1 <= n).　省略時は'-b{1}'.
		-bgr  : BGR形式で出力する.
		-l    : '-f565'または'-f555'指定時に下位バイト先行で出力する.（-エル）
		例
		ImageToBytes.exe c:\dir\myimage.jpg -f565 -b20 >z:\myimage565.txt
		※オプションの指定順は不問.".Replace("\t", "");
	mes = string.Format(mes, FormatDefault, LinePixelDefault);
	Console.WriteLine(mes);
}

//オプション解析
void GetOptionParam(out string imgFile, out Int32 format, out bool toBGR, out bool lowByteFirst, out Int32 linePixel)
{
	imgFile = string.Empty;
	format = FormatDefault;
	toBGR = false;
	lowByteFirst = false;
	linePixel = LinePixelDefault;

	foreach (string option in args)
	{
		if (string.IsNullOrEmpty(option)) { continue; }
		
		if (!option.StartsWith('-'))
		{
			imgFile = option;
		}
		else if (option.ToLower().Equals("-bgr"))
		{
			toBGR = true;
		}
		else if (option.ToLower().Equals("-l"))
		{
			lowByteFirst = true;
		}
		else if (option.StartsWith("-f"))
		{
			try { format = Int32.Parse(option.Substring(2)); }
			catch { }
		}
		else if (option.StartsWith("-b"))
		{
			try { linePixel = Int32.Parse(option.Substring(2)); }
			catch { }
		}
	}
}

//ファイル読み込み
Bitmap? LoadImage(string path)
{
	try { return new Bitmap(path); }
	catch { return null; }
}

//スケーリング
Byte BitScaling(Byte byteValue, Int32 toBit)
{
	UInt32 maxValue = (1U << toBit) - 1;
	UInt32 scaledValue = (byteValue * maxValue + 127) / 255;  //+127は四捨五入のため
	return (Byte)scaledValue;
}

//RGBのRとBを入れ替え
void SwapColor(ref Byte r, ref Byte b)
{
    (b, r) = (r, b);	//タプルを利用した値の交換
}

//上位バイト取得
Byte HiByte(UInt16 val)
{
	return (Byte)((val >> 8) & 0xFF);
}

//下位バイト取得
Byte LoByte(UInt16 val)
{
	return (Byte)(val & 0xFF);
}

//画素データを出力
bool OutputColorByte888(Color color)
{
	var r = color.R;
	var g = color.G;
	var b = color.B;

	if (toBGR) { SwapColor(ref r, ref b); }
	Console.Write("0x{0:X2},0x{1:X2},0x{2:X2}", r, g, b);
	return true;
}

//画素データを出力
bool OutputColorByte666(Color color)
{
	var r = BitScaling(color.R, 6);
	var g = BitScaling(color.G, 6);
	var b = BitScaling(color.B, 6);

	if (toBGR) { SwapColor(ref r, ref b); }
	Console.Write("0x{0:X2},0x{1:X2},0x{2:X2}", r, g, b);
	return true;
}

//画素データを出力
bool OutputColorByte565(Color color)
{
	var r = BitScaling(color.R, 5);
	var g = BitScaling(color.G, 6);
	var b = BitScaling(color.B, 5);

	if (toBGR) { SwapColor(ref r, ref b); }
	UInt16 val = (UInt16)((r << 11) | (g << 5) | b);
	if (lowByteFirst)
		{ Console.Write("0x{0:X2},0x{1:X2}", LoByte(val), HiByte(val)); }
	else
		{ Console.Write("0x{0:X2},0x{1:X2}", HiByte(val), LoByte(val)); }
	return true;
}

//画素データを出力
bool OutputColorByte555(Color color)
{
	var r = BitScaling(color.R, 5);
	var g = BitScaling(color.G, 5);
	var b = BitScaling(color.B, 5);

	if (toBGR) { SwapColor(ref r, ref b); }
	UInt16 val = (UInt16)((r << 10) | (g << 5) | b);
	if (lowByteFirst)
		{ Console.Write("0x{0:X2},0x{1:X2}", LoByte(val), HiByte(val)); }
	else
		{ Console.Write("0x{0:X2},0x{1:X2}", HiByte(val), LoByte(val)); }
	return true;
}

//画素データを出力
bool OutputColorByte444(Color color)
{
	if (fmt444FuncCallCount == 0) { fmt444FirstColor = color; fmt444FuncCallCount++; return false; }
	if (fmt444FuncCallCount == 1) { fmt444FuncCallCount = 0; }

	var r1 = BitScaling(fmt444FirstColor.R, 4);
	var g1 = BitScaling(fmt444FirstColor.G, 4);
	var b1 = BitScaling(fmt444FirstColor.B, 4);
	var r2 = BitScaling(color.R, 4);
	var g2 = BitScaling(color.G, 4);
	var b2 = BitScaling(color.B, 4);

	if (toBGR) { SwapColor(ref r1, ref b1); SwapColor(ref r2, ref b2); }
	UInt32 val = (UInt32)((r1 << 20) | (g1 << 16) | (b1 << 12) | (r2 << 8) | (g2 << 4) | b2);
	Byte byte3 = (Byte)(val & 0xFF);
	Byte byte2 = (Byte)((val >> 8) & 0xFF);
	Byte byte1 = (Byte)((val >> 16) & 0xFF);
	Console.Write("0x{0:X2},0x{1:X2},0x{2:X2}", byte1, byte2, byte3);
	return true;
}
