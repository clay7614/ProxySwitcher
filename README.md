# Proxy Switcher

タスクトレイ上でプロキシ設定をON/OFFできるPython製アプリケーション。
Ctrl+Alt+Pでプロキシ切り替えが可能。

## 使い方
1. 必要パッケージをインストール:

   ```powershell
   pip install -r requirements.txt
   ```

2. 実行:

   ```powershell
   python main.py
   ```

# README.md を更新

## exe化
PyInstallerを使ってexe化できます:

```powershell
# 1. アイコンファイルを生成
python create_icon.py

# 2. PyInstallerをインストール
pip install pyinstaller

# 3. specファイルを使用してexe化
pyinstaller ProxySwitcher.spec

# または、コマンドラインで直接実行
# pyinstaller --onefile --windowed --name="ProxySwitcher" --version-file="version_info.txt" --icon="icon.ico" main.py
```

生成されたexeファイルは `dist` フォルダ内に作成されます。

## ファイル構成
- `main.py` - メインプログラム  
- `proxy_icon.ico` - アプリケーションアイコン
- `ProxySwitcher.spec` - PyInstaller設定ファイル
- `version_info.txt` - バージョン情報
- `proxy_config.json` - プロキシ設定（自動生成）

## 設定
- exe化時、設定ファイルは `%APPDATA%\ProxySwitcher\proxy_config.json` に保存されます
- 開発時は実行ディレクトリに `proxy_config.json` が作成されます
- `proxy_icon.ico` はexe内にリソースとして埋め込まれ、実行時に自動で利用されます
