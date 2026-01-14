# ProxySwitcher

![Platform-Windows](https://img.shields.io/badge/Platform-Windows-blue.svg)
![Framework-.NET4.8](https://img.shields.io/badge/Framework-.NET%20Framework%204.8-purple.svg)

**ProxySwitcher** は、Windows のプロキシ設定をタスクトレイから切り替えられるアプリケーションです。

## 機能

- タスクトレイのアイコンから、プロキシの ON/OFF を切り替えできます。
- 予め指定したSSIDに接続した際、自動でプロキシを ON にし、それ以外では OFF にする機能を搭載。
- `Ctrl + Alt + P` のショートカットキーで素早く切り替えができます。
- プロキシの状態に合わせてタスクトレイアイコンが変化（緑：ON、赤：OFF）。
- Windows スタートアップへの登録が可能。

## 使い方

### 1. 準備
- [`ProxySwitcher.exe`](https://github.com/clay7614/ProxySwitcher/releases/download/v2.1.0/ProxySwitcher_v2.1.0.exe) をダウンロードして、任意のフォルダに配置してください。
  
  ※ダウンロードが失敗する場合、[`ProxySwitcher.zip`](https://github.com/clay7614/ProxySwitcher/releases/download/v2.1.0/ProxySwitcher_v2.1.0.zip)をダウンロードし、展開してからファイルを実行してください。
- 実行するだけで動作します。

### 2. 基本的な操作
- **プロキシの切り替え**: 
  - タスクトレイアイコンを右クリック → 「プロキシをON/OFFにする」を選択。
  - または、ショートカットキー `Ctrl + Alt + P` を押す。
- **設定の変更**:
  - タスクトレイアイコンを右クリック → 「設定」を選択。
  - プロキシの設定（例: `proxy.example.com:8080`）を入力して保存。

### 3. WiFi 自動連動の設定
- 設定画面で「対象のWiFi SSID」を入力し、「このWiFi接続時にプロキシを自動ONにする」にチェックを入れて保存してください。
- 指定した SSID に接続されると自動的にプロキシが有効になり、切断または別のネットワークに接続されると自動的に無効になります。

## システム要件

- **OS**: Windows 10 / 11

## FAQ
- 「次により位置情報が使用中：ProxySwitcher」と表示されることがありますが、  WiFIのスキャンやプロキシの自動切替の際に、SSIDを取得するために表示される現象です。実際に位置情報の取得は行っておりません。
