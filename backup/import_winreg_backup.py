import winreg
import ctypes
import sys
import os # アイコンファイルのパス解決用
from PIL import Image # アイコン画像読み込み用
import pystray # タスクトレイ制御用


# レジストリ関連の定数
INTERNET_SETTINGS_PATH = r"Software\Microsoft\Windows\CurrentVersion\Internet Settings"
PROXY_ENABLE_VALUE_NAME = "ProxyEnable"

# WinINet API関連の定数 (設定変更の即時反映のため)
INTERNET_OPTION_SETTINGS_CHANGED = 39
INTERNET_OPTION_REFRESH = 37

# アイコンファイル名 (スクリプトと同じディレクトリに配置)
ICON_FILENAME = "proxy_icon.ico" # または "icon.ico" など

def get_proxy_status():
    """現在のプロキシ設定状態を確認します。"""
    try:
        # HKEY_CURRENT_USER の下にあるインターネット設定キーを開く
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, INTERNET_SETTINGS_PATH, 0, winreg.KEY_READ) as key:
            # ProxyEnable 値を読み取る (0:無効, 1:有効)
            value, reg_type = winreg.QueryValueEx(key, PROXY_ENABLE_VALUE_NAME)
            return bool(value)
    except FileNotFoundError:
        # ProxyEnable 値が存在しない場合は、プロキシ無効とみなす
        print(f"情報: プロキシ設定値 '{PROXY_ENABLE_VALUE_NAME}' が見つかりません。無効として扱います。")
        return False
    except Exception as e:
        print(f"プロキシ状態の読み取り中にエラーが発生しました: {e}")
        return False # エラー時は無効として扱うか、例外を再送出するかは要件による

def set_proxy_status(enable: bool):
    """プロキシ設定を有効または無効にします。"""
    try:
        # HKEY_CURRENT_USER の下にあるインターネット設定キーを書き込みモードで開く
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, INTERNET_SETTINGS_PATH, 0, winreg.KEY_WRITE) as key:
            # ProxyEnable 値を設定 (DWORD型)
            winreg.SetValueEx(key, PROXY_ENABLE_VALUE_NAME, 0, winreg.REG_DWORD, 1 if enable else 0)
        return True
    except PermissionError:
        print(f"エラー: レジストリへの書き込み権限がありません。")
        print(f"このスクリプトを管理者として実行する必要があるかもしれません。")
        return False
    except Exception as e:
        print(f"プロキシ状態の設定中にエラーが発生しました: {e}")
        return False

def refresh_internet_settings():
    """システムにインターネット設定の変更を通知し、リフレッシュします。"""
    try:
        internet_set_option = ctypes.windll.Wininet.InternetSetOptionW
        
        # 設定変更を通知
        # InternetSetOptionW(hInternet, dwOption, lpBuffer, dwBufferLength)
        # グローバルオプションの場合、hInternet は NULL (None)
        # INTERNET_OPTION_SETTINGS_CHANGED の場合、lpBuffer は NULL (None), dwBufferLength は 0
        settings_changed_result = internet_set_option(None, INTERNET_OPTION_SETTINGS_CHANGED, None, 0)
        if not settings_changed_result:
            # GetLastError は Windows API のエラーコードを返す
            print(f"警告: システムへの設定変更通知に失敗しました (INTERNET_OPTION_SETTINGS_CHANGED)。エラーコード: {ctypes.GetLastError()}")
        else:
            print("システムに設定変更を通知しました (INTERNET_OPTION_SETTINGS_CHANGED)。")

        # 設定をリフレッシュ
        # INTERNET_OPTION_REFRESH の場合も同様
        refresh_result = internet_set_option(None, INTERNET_OPTION_REFRESH, None, 0)
        if not refresh_result:
            print(f"警告: インターネット設定のリフレッシュに失敗しました (INTERNET_OPTION_REFRESH)。エラーコード: {ctypes.GetLastError()}")
        else:
            print("インターネット設定をリフレッシュしました (INTERNET_OPTION_REFRESH)。")
            
    except AttributeError:
        # windll.Wininet や InternetSetOptionW が見つからない場合 (通常は発生しない)
        print("エラー: WinINetライブラリのロードに失敗しました。Windows環境で実行しているか確認してください。")
    except Exception as e:
        print(f"インターネット設定の更新中に予期せぬエラーが発生しました: {e}")


def get_proxy_menu_text(item):
    """メニューのプロキシ切り替えテキストを動的に生成します。"""
    if get_proxy_status():
        return "プロキシを無効にする"
    else:
        return "プロキシを有効にする"

def toggle_proxy_action(icon, item):
    """プロキシ設定をトグルし、メニューを更新します。"""
    current_status = get_proxy_status()
    new_status = not current_status
    action_verb = "有効化" if new_status else "無効化"
    new_status_str = "有効" if new_status else "無効"

    print(f"プロキシ設定を {new_status_str} に変更します...")
    if set_proxy_status(new_status):
        print(f"レジストリのプロキシ設定を {new_status_str} に変更しました。")
        refresh_internet_settings()
        print(f"プロキシ設定は正常に {new_status_str} に変更されました。")
    else:
        print(f"プロキシ設定の {action_verb} に失敗しました。")
    
    # メニューの更新をpystrayに通知 (テキストが動的に変わるため)
    icon.update_menu()

def exit_action(icon, item):
    """プログラムを終了します。"""
    print("プログラムを終了します...")
    icon.stop()

def setup_tray_icon():
    """タスクトレイアイコンとメニューをセットアップし、表示します。"""
    # アイコンファイルのパスを取得
    # スクリプトが実行されているディレクトリを基準にする
    script_dir = os.path.dirname(os.path.abspath(sys.argv[0] if getattr(sys, 'frozen', False) else __file__))
    icon_path = os.path.join(script_dir, ICON_FILENAME)

    if not os.path.exists(icon_path):
        print(f"エラー: アイコンファイルが見つかりません: {icon_path}")
        print(f"スクリプトと同じディレクトリに '{ICON_FILENAME}' を配置してください。")
        return

    try:
        image = Image.open(icon_path)
    except Exception as e:
        print(f"エラー: アイコンファイルの読み込みに失敗しました: {e}")
        return

    # メニューアイテムの定義
    # MenuItemの第一引数にテキストを返す関数を渡すことで動的なテキストを実現
    menu = pystray.Menu(
        pystray.MenuItem(
            get_proxy_menu_text, # 現在の状態に応じてテキストが変わる
            toggle_proxy_action
        ),
        pystray.Menu.SEPARATOR,
        pystray.MenuItem(
            "終了",
            exit_action
        )
    )

    icon = pystray.Icon("proxy_toggler", image, "プロキシ設定トグラー", menu)
    print("タスクトレイアイコンを起動します。右クリックでメニューを表示できます。")
    icon.run()

def main():
    if sys.platform != "win32":
        print("このスクリプトはWindows環境でのみ動作します。")
        return
    setup_tray_icon()

if __name__ == "__main__":
    main()