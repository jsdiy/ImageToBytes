# ImageToBytes
画像ファイルを電子工作でよく使われるLCDモジュール向けのフォーマットに変換するツールです。

![画面参考](https://github.com/jsdiy/ImageToBytes/blob/main/sample/usage.png)

# 概要
- .jpg/.png/.bmpをRGB:565やRGB:444形式に変換します。BGR順へも変換できます。
- カンマ区切りのbyte配列としてテキスト形式で書き出します。　※ファイルへリダイレクト
- Windows用コンソールアプリケーションです。

# 開発環境
- Windows11
- VisualStudio2026 / .NET 10
	- Windowsコンソールアプリ / Top-level statements
	- System.Drwing.Common　※これを利用しているのでOSはWindowsのみの対応

# 動作環境
WindowsPC上で 実行ファイルと .NET Runtimeがあれば動作します。  
※実行には .NET Runtimeが必要 → [Microsoft .NETのダウンロードページ](https://dotnet.microsoft.com/ja-jp/download/dotnet/10.0)

Releaseフォルダ内の実行ファイルは下記プロファイルで作成しています。
- net10.0-windows, win-x64, フレームワーク依存, 単一ファイルの作成

# 利用イメージ
コンソールで実行
```
ImageToBytes.exe D:\pic\img20250310.png -f565 >Z:\img20250310rgb565.txt
```

出力結果
```
//img20250310 - 120x160 - RGB:565 (38,400 bytes)
0x43,0x10,0x4B,0x74,0x62,0x4B,0x52,0xEE,0x3A,0xED,0x11,0x27,0x1B,0x54,0x43,0xB5,
0x3A,0xD0,0x18,0x41,0x83,0x2A,0xB4,0x50,0x9B,0xAE,0x6B,0xB3,0x13,0xD7,0x0B,0xB6,
0x33,0x31,0x2A,0xD0,0x23,0x74,0x23,0x74,0x3B,0x93,0x41,0xC3,0x6A,0xED,0x62,0x88,
……
```
