# Proxy Switcher 

Windowsのプロキシ設定を簡単に切り替えられるタスクトレイ常駐型アプリケーションです。  
ショートカットキーまたはタスクトレイメニューから、素早くプロキシの有効/無効を切り替えることができます。

---

## 目次

- [使用方法](#使用方法)
  - [EXE版 (推奨)](#exe版-推奨)
  - [スクリプト版](#スクリプト版)
- [特徴](#特徴)
- [動作環境](#動作環境)
- [必要なライブラリ](#必要なライブラリ)
- [設定](#設定)
  - [ショートカットキーの変更](#ショートカットキーの変更)
  - [アプリケーションID (通知用)](#アプリケーションid-通知用)
- [注意事項](#注意事項)

---

## 使用方法

### EXE版 (推奨)

1. Releasesから `proxy.exe` をダウンロードします。  
2. `proxy.exe` を実行します。  
3. 通知が送られ，タスクトレイにアイコンが表示されます。右クリックするとメニューが表示され、「プロキシを有効にする」または「プロキシを無効にする」を選択して設定を切り替えられます。  
4. 「終了」を選択するとアプリケーションを終了します。  
5. デフォルトのショートカットキー `Ctrl + Alt + P` でもプロキシ設定を切り替えられます。  
6. 「Windows + R」から「Proxy」と入力することでも実行可能です。

### スクリプト版

1. このリポジトリをクローンするか、`proxy.py` (またはメインのPythonスクリプトファイル) をダウンロードします。  
2. 必要なライブラリをインストールします（下記「必要なライブラリ」参照）。  
3. コマンドプロンプトやターミナルから以下を実行します。

    ```sh
    python proxy.py
    ```

4. 以降の操作はEXE版と同様です。

---

## 動作環境

- Windows OS  
- Python 3.x（スクリプトとして実行する場合）

---

## 必要なライブラリ

スクリプトとして実行する場合、以下のPythonライブラリが必要です。  
`pip install -r requirements.txt` でインストールできます。

- [Pillow (PIL Fork)](https://pypi.org/project/Pillow/): アイコン画像の処理用  
- [pystray](https://pypi.org/project/pystray/): タスクトレイアイコンの制御用  
- [pynput](https://pypi.org/project/pynput/): グローバルショートカットキーの監視用  
- [winotify](https://pypi.org/project/winotify/): Windowsトースト通知の表示用

**requirements.txt の例:**

    Pillow>=9.0.0
    pystray>=0.19.0
    pynput>=1.7.0
    winotify>=1.1.0

---

## 設定

### ショートカットキーの変更

ショートカットキーを変更したい場合は、`proxy.py` スクリプト内の以下の部分を編集してください。

```python
# --- ショートカットキー設定 ---
# 例: Ctrl + Alt + P (P for Proxy)
SHORTCUT_KEY_COMBINATION = frozenset([
    keyboard.Key.ctrl_l,    # 左Ctrlキー (keyboard.Key.ctrl でも可)
    keyboard.Key.alt_l,     # 左Altキー (keyboard.Key.alt でも可)
    keyboard.KeyCode.from_char('p')  # 'p'キー
])
```

利用可能なキーについては [pynput のドキュメント](https://pynput.readthedocs.io/) を参照してください。

### アプリケーションID (通知用)

Windowsの通知設定でこのアプリケーションを識別するために使用されるIDです。通常は変更不要です。

```python
APP_ID = "Proxy Switcher"  # アプリケーションID
```

---

## 注意事項

- プロキシ設定の変更には、レジストリへの書き込み権限が必要です。環境によっては管理者権限が必要になる場合があります。  
- 環境変数のPATHを更新する機能（EXE版）も、管理者権限が必要になる場合があります。  
- セキュリティソフトによっては、グローバルなキーボード入力を監視する動作やレジストリ変更を警告またはブロックすることがあります。その場合は、本アプリケーションを信頼済みとして設定してください。

---
