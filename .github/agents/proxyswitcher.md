---
trigger: always_on
glob: "**/*.cs"
description: ProxySwitcherのプロジェクトコンテキストと開発ルール
---

# ProxySwitcher カスタムエージェント規則

このファイルは、ProxySwitcher リポジトリを扱うAIエージェントのためのガイドラインとプロジェクトコンテキストを定義します。

## 1. プロジェクト概要
ProxySwitcher は、Windows のプロキシ設定をタスクトレイから素早く切り替えるための軽量アプリケーションです。C# (.NET 9 / Windows Forms) で実装されています。

## 2. コア機能
- **手動切り替え**: タスクトレイメニューまたはホットキー (Ctrl + Alt + P) による即時切り替え。
- **WiFi連動自動化**: 接続中の SSID が指定リストに含まれる場合、自動的にプロキシを有効化。
- **WiFiスキャン & 手動追加**: 周囲のネットワークをスキャンして選択、または SSID を手動入力してリスト登録。
- **動的アイコン**: プロキシの ON/OFF 状態を反映したグラフィカルなアイコンを動的に生成。

## 3. ディレクトリ・ファイル構成
- **Program.cs**: エントリポイント。トレイアイコン、コンテキストメニュー、ホットキーおよび監視クラスのライフサイクルを管理。
- **ProxyManager.cs**: レジストリ (HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings) を操作。ProxyEnable と ProxyServer を制御し、Wininet API で反映。
- **WifiWatcher.cs**: ネットワーク変更を監視。SSID パースには netsh wlan show interfaces を使用。
- **WifiScanner.cs**: netsh wlan show networks を使用して周辺の SSID をリストアップ。
- **SettingsForm.cs**: 各種設定 UI。文字見切れを防ぐため、フォントは Yu Gothic UI 9pt 固定。コントロールの配置はマージンに余裕を持つこと。
- **AppConfig.cs**: %AppData%\ProxySwitcher\config.json に設定を永続化。
- **HotKeyHandler.cs**: Win32 API (RegisterHotKey) を使用したシステム全体でのショートカット。
- **AutoStartManager.cs**: レジストリの Run キーによる Windows 起動時実行の管理。

## 4. 開発時の重要なルール
- **UI調整**: 文字の見切れや重なりに非常に敏感なため、新しいコントロールを追加する際は、余裕を持ったサイズ設計と明示的なフォント指定を行うこと。
- **文字コード**: netsh コマンドの結果を読み取る際は、システムの標準文字コードを使用し、日本語環境でのパース崩れに注意すること。
- **非同期処理**: WiFiスキャンなどの時間のかかる処理は、UIフリーズを防ぐために非同期化すること。

## 5. 命名とスタイル
- すべての出力、説明、ドキュメントは日本語。文字・コメントも日本語。
- 絵文字の使用は厳禁。
- コミットプレフィックスルール（fix, add, update 等）を遵守。
