import winreg
import ctypes
import sys
import os # アイコンファイルのパス解決用
from PIL import Image, ImageFile # アイコン画像読み込み用
import base64 # Base64エンコード/デコード用
import io     # メモリ上のバイナリデータ操作用
import tempfile # 一時ファイル作成用
import atexit   # 終了時処理登録用
import pystray # タスクトレイ制御用
from pynput import keyboard # キーボード入力監視用
from winotify import Notification, Notifier # Windowsトースト通知用 (win10toastから変更)
from winotify import Registry  # 追加: Registryをインポート

ImageFile.LOAD_TRUNCATED_IMAGES = True

# レジストリ関連の定数
INTERNET_SETTINGS_PATH = r"Software\Microsoft\Windows\CurrentVersion\Internet Settings"
PROXY_ENABLE_VALUE_NAME = "ProxyEnable"

# WinINet API関連の定数 (設定変更の即時反映のため)
INTERNET_OPTION_SETTINGS_CHANGED = 39
INTERNET_OPTION_REFRESH = 37

# --- ショートカットキー設定 ---
# 例: Ctrl + Alt + P (P for Proxy)
SHORTCUT_KEY_COMBINATION = frozenset([
    keyboard.Key.ctrl_l,  # 左Ctrlキー (keyboard.Key.ctrl でも可)
    keyboard.Key.alt_l,   # 左Altキー (keyboard.Key.alt でも可)
    keyboard.KeyCode.from_char('p') # 'p'キー
])
current_pressed_keys = set() # 現在押されているキーを保持するセット

# --- 環境変数関連 ---
ENVIRONMENT_KEY_PATH = r"Environment" # HKEY_CURRENT_USER の下のパス
PATH_VALUE_NAME = "Path"
HWND_BROADCAST = 0xFFFF
WM_SETTINGCHANGE = 0x001A
SMTO_ABORTIFHUNG = 0x0002

keyboard_listener = None # キーボードリスナーオブジェクト
tray_icon = None # pystray.Icon オブジェクトを保持
win_notifier = None # winotify.Notifier オブジェクトを保持 (toasterから変更)

APP_ID = "Proxy Switcher" # アプリケーションID
_temp_notification_icon_path = None # 通知用の一時アイコンファイルのパス
ICON_BASE64_STRING = """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"""

def get_icon_image_from_base64():
    """
    Base64エンコードされた文字列からPIL.Imageオブジェクトを生成して返します。
    これにより、ファイルシステム上のアイコンファイルへのアクセスを回避します。
    """
    try:
        image_data = base64.b64decode(ICON_BASE64_STRING)
        pil_image = Image.open(io.BytesIO(image_data))
        return pil_image
    except Exception as e:
        print(f"Base64からのアイコン画像生成中にエラー: {e}")
        return None

def _cleanup_temp_notification_icon():
    """
    通知用に作成された一時アイコンファイルをクリーンアップします。
    """
    global _temp_notification_icon_path
    if _temp_notification_icon_path and os.path.exists(_temp_notification_icon_path):
        try:
            os.remove(_temp_notification_icon_path)
            print(f"一時通知アイコンファイルを削除しました: {_temp_notification_icon_path}")
            _temp_notification_icon_path = None
        except PermissionError:
             # ファイルがまだ使用中の場合 (通常は発生しにくいが念のため)
            print(f"警告: 一時通知アイコンファイル '{_temp_notification_icon_path}' は使用中のため削除できませんでした。")
        except Exception as e:
            print(f"エラー: 一時通知アイコンファイル '{_temp_notification_icon_path}' のクリーンアップに失敗しました: {e}")
    else:
        _temp_notification_icon_path = None # パスが無効ならクリア

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

def set_app_user_model_id(app_id):
    """現在のプロセスにAppUserModelIDを設定します。Windows Vista以降で有効です。"""
    if sys.platform == "win32":
        try:
            ctypes.windll.shell32.SetCurrentProcessExplicitAppUserModelID(app_id)
            print(f"AppUserModelIDを設定しました: {app_id}")
        except AttributeError:
            print("警告: AppUserModelIDの設定に失敗しました (shell32.SetCurrentProcessExplicitAppUserModelIDが見つかりません)。古いWindowsバージョンの可能性があります。")
        except Exception as e:
            print(f"警告: AppUserModelIDの設定中にエラーが発生しました: {e}")

def broadcast_env_change():
    """環境変数の変更をシステムに通知します。"""
    try:
        # SendMessageTimeoutW の lpdwResult 用の変数 (ctypes.c_ulong)
        # この変数のアドレスを渡す必要があるため、byrefで渡す
        lpdwResult = ctypes.c_ulong()
        result = ctypes.windll.user32.SendMessageTimeoutW(
            HWND_BROADCAST,
            WM_SETTINGCHANGE,
            0, # wParam は使用しない
            "Environment", # lParam は変更された設定の種類を示す文字列
            SMTO_ABORTIFHUNG,
            5000, # タイムアウト5秒 (ミリ秒単位)
            ctypes.byref(lpdwResult)
        )
        if result == 0: # タイムアウトまたはエラー
            # error_code = ctypes.GetLastError() # 必要であればエラーコードを取得
            print(f"警告: 環境変数の変更通知に失敗しました (SendMessageTimeoutW)。")
        else:
            print("環境変数の変更をシステムに通知しました。")
    except Exception as e:
        print(f"環境変数の変更通知中にエラーが発生しました: {e}")

def update_path_environment_variable():
    """
    現在のスクリプトのディレクトリをユーザーのPATH環境変数に追加/更新します。
    EXEとして実行されている場合のみ動作します。
    """
    if not getattr(sys, 'frozen', False):
        print("情報: スクリプトがEXEとして実行されていないため、PATH環境変数の更新をスキップします。")
        return

    try:
        executable_path = sys.executable
        current_script_dir = os.path.normpath(os.path.dirname(executable_path))
        script_filename = os.path.basename(executable_path)

        print(f"実行ファイルパス: {executable_path}")
        print(f"現在のスクリプトディレクトリ: {current_script_dir}")
        print(f"スクリプトファイル名: {script_filename}")

        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, ENVIRONMENT_KEY_PATH, 0, winreg.KEY_READ | winreg.KEY_WRITE) as env_key:
            try:
                current_path_value, reg_type = winreg.QueryValueEx(env_key, PATH_VALUE_NAME)
            except FileNotFoundError:
                current_path_value = ""
                reg_type = winreg.REG_EXPAND_SZ # 新規作成時はREG_EXPAND_SZが一般的
                print(f"情報: 環境変数 '{PATH_VALUE_NAME}' が見つかりません。新規に作成します。")

            original_paths_list = [os.path.normpath(p.strip()) for p in current_path_value.split(';') if p.strip()]
            
            paths_after_removal = []
            removed_old_paths = False
            # 注意: 以下のロジックは、同じ実行ファイル名を持つ他のアプリケーションのパスを
            # 誤って削除する可能性がないか、慎重に検討する必要があります。
            for path_entry in original_paths_list:
                if os.path.normcase(path_entry) != os.path.normcase(current_script_dir) and \
                   os.path.isfile(os.path.join(path_entry, script_filename)):
                    print(f"削除対象の古いPATHエントリ: {path_entry} (理由: '{script_filename}' が存在し、現在のディレクトリと異なる)")
                    removed_old_paths = True
                else:
                    paths_after_removal.append(path_entry)

            final_path_list = [current_script_dir] + [p for p in paths_after_removal if os.path.normcase(p) != os.path.normcase(current_script_dir)]
            final_path_list = list(dict.fromkeys(final_path_list)) # 順序を保持しつつ重複を削除

            path_actually_changed = (original_paths_list != final_path_list) or removed_old_paths

            if path_actually_changed:
                new_path_value = ";".join(final_path_list)
                target_reg_type = reg_type if current_path_value else winreg.REG_EXPAND_SZ
                winreg.SetValueEx(env_key, PATH_VALUE_NAME, 0, target_reg_type, new_path_value)
                print(f"環境変数 '{PATH_VALUE_NAME}' を更新しました。新しい値: {new_path_value}")
                broadcast_env_change()
            else:
                print(f"環境変数 '{PATH_VALUE_NAME}' は既に最新の状態です。変更はありません。")

    except PermissionError:
        print(f"エラー: 環境変数 '{PATH_VALUE_NAME}' への書き込み権限がありません。管理者として実行する必要があるかもしれません。")
    except FileNotFoundError:
        # HKEY_CURRENT_USER\Environment が存在しない場合など
        print(f"エラー: 環境変数レジストリキー '{ENVIRONMENT_KEY_PATH}' が見つかりません。")
    except Exception as e:
        print(f"環境変数 '{PATH_VALUE_NAME}' の更新中に予期せぬエラーが発生しました: {e}", file=sys.stderr)

def show_windows_notification(title, message):
    """Windowsのトースト通知を表示します。"""
    global win_notifier, _temp_notification_icon_path

    icon_to_show_path = "" # デフォルトはアイコンなし

    # Base64からアイコンイメージを取得
    pil_image = get_icon_image_from_base64()

    if pil_image:
        # 一時アイコンファイルがまだ作成されていないか、存在しない場合のみ作成
        if not _temp_notification_icon_path or not os.path.exists(_temp_notification_icon_path):
            try:
                # 一時ファイルとして保存 (atexitでクリーンアップ)
                # 接頭辞にアプリ名を含めると識別しやすい
                fd, temp_path = tempfile.mkstemp(suffix=".png", prefix=f"{APP_ID}_notify_icon_")
                os.close(fd) # mkstemp で開かれたファイルディスクリプタを閉じる (PIL.Image.saveで再度開くため)
                
                pil_image.save(temp_path, format="ICO", sizes=[(64, 64)]) # ICO形式で保存
                _temp_notification_icon_path = temp_path # グローバル変数に保存
                print(f"通知用アイコンを一時ファイルに保存しました: {_temp_notification_icon_path}")
            except Exception as e:
                print(f"エラー: 通知用一時アイコンの作成/保存に失敗しました: {e}")
                _temp_notification_icon_path = None # 作成失敗
        
        if _temp_notification_icon_path and os.path.exists(_temp_notification_icon_path):
            icon_to_show_path = _temp_notification_icon_path

    try:
        if win_notifier: # win_notifierがNoneでないことを確認
            toast = Notification(app_id=APP_ID, # グローバルなAPP_IDを使用
                                 title=title,
                                 msg=message,
                                 icon=icon_to_show_path, # 処理されたアイコンパス
                                 duration='short')
            toast.show()
        else:
            print("警告: Windows通知用のNotifierが初期化されていません。")
    except Exception as e:
        print(f"Windows通知の表示中にエラーが発生しました: {e}")

def _perform_proxy_toggle():
    """
    プロキシ設定を実際にトグルし、結果を返す内部関数。
    成功した場合はTrue、失敗した場合はFalseを返します。
    """
    current_status = get_proxy_status()
    new_status = not current_status
    action_verb = "有効化" if new_status else "無効化"
    new_status_str = "有効" if new_status else "無効"

    print(f"プロキシ設定を {new_status_str} に変更します...")
    if set_proxy_status(new_status):
        print(f"レジストリのプロキシ設定を {new_status_str} に変更しました。")
        refresh_internet_settings()
        print(f"プロキシ設定は正常に {new_status_str} に変更されました。")

        # Windows通知を表示
        notification_title = "プロキシ設定変更"
        notification_message = f"プロキシが {new_status_str} になりました。"
        show_windows_notification(notification_title, notification_message)
        return True
    else:
        print(f"プロキシ設定の {action_verb} に失敗しました。")
        show_windows_notification("プロキシ設定エラー", f"プロキシの {action_verb} に失敗しました。")
        return False

def get_proxy_menu_text(item):
    """メニューのプロキシ切り替えテキストを動的に生成します。"""
    if get_proxy_status():
        return "プロキシを無効にする"
    else:
        return "プロキシを有効にする"

def toggle_proxy_action_menu(icon_obj, item): # pystrayメニューコールバック用
    """プロキシ設定をトグルし、メニューを更新します。(メニューからの呼び出し用)"""
    _perform_proxy_toggle()
    if icon_obj: # icon_obj は pystray.Icon インスタンス
        icon_obj.update_menu()

def format_shortcut_keys(keys_iterable):
    """pynputのキーのイテラブルを人間が読める形式の文字列に変換します。"""
    display_parts = []
    ctrl_keys = {keyboard.Key.ctrl, keyboard.Key.ctrl_l, keyboard.Key.ctrl_r}
    alt_keys = {keyboard.Key.alt, keyboard.Key.alt_l, keyboard.Key.alt_r}
    shift_keys = {keyboard.Key.shift, keyboard.Key.shift_l, keyboard.Key.shift_r}

    has_ctrl = any(k in keys_iterable for k in ctrl_keys)
    has_alt = any(k in keys_iterable for k in alt_keys)
    has_shift = any(k in keys_iterable for k in shift_keys)

    if has_ctrl: display_parts.append("Ctrl")
    if has_alt: display_parts.append("Alt")
    if has_shift: display_parts.append("Shift")

    for key_obj in keys_iterable:
        is_modifier = False
        if isinstance(key_obj, keyboard.Key):
            if key_obj in ctrl_keys or key_obj in alt_keys or key_obj in shift_keys:
                is_modifier = True
        
        if not is_modifier:
            if isinstance(key_obj, keyboard.KeyCode) and key_obj.char:
                display_parts.append(key_obj.char.upper())
            elif isinstance(key_obj, keyboard.Key): # Fキーなど
                display_parts.append(str(key_obj).replace("Key.", "").capitalize())
                
    return " + ".join(display_parts)

def on_key_press(key):
    """キーが押されたときに呼び出される関数。"""
    global current_pressed_keys, tray_icon
    if key in SHORTCUT_KEY_COMBINATION:
        current_pressed_keys.add(key)
        if SHORTCUT_KEY_COMBINATION.issubset(current_pressed_keys):
            shortcut_str = format_shortcut_keys(SHORTCUT_KEY_COMBINATION)
            print(f"ショートカットキー ({shortcut_str}) が押されました。プロキシを切り替えます。")
            if _perform_proxy_toggle(): # プロキシ切り替え実行
                if tray_icon:
                    tray_icon.update_menu() # タスクトレイメニューのテキストを更新
            # 一度のショートカットキー入力で一度のトグル実行を保証するため、
            # 処理後に押下キーのセットをクリアします。
            current_pressed_keys.clear()

def on_key_release(key):
    """キーが離されたときに呼び出される関数。"""
    global current_pressed_keys
    # on_key_pressで押下キーセットをクリアする方式を採用した場合、
    # on_key_releaseでの明示的なセットからの削除は必須ではなくなります。
    # ただし、特定のキーが離されたことを検知したい場合はこの関数自体は有用です。
    # 今回のショートカットキー処理では、on_key_pressで完結するため、
    # ここでの current_pressed_keys の操作は不要になります。
    if key in current_pressed_keys: # 念のため、まだセットに残っていたら削除
         current_pressed_keys.remove(key)

def exit_action(icon, item):
    """プログラムを終了します。"""
    global keyboard_listener
    print("プログラムを終了します...")
    if keyboard_listener:
        print("キーボードリスナーを停止しています...")
        keyboard_listener.stop()
    _cleanup_temp_notification_icon() # 通知用の一時アイコンをクリーンアップ
    if icon: # icon は pystray.Icon インスタンス
        icon.stop()

def setup_tray_icon():
    """タスクトレイアイコンとメニューをセットアップし、表示します。"""
    global tray_icon, keyboard_listener # グローバル変数の使用を宣言
    
    image = get_icon_image_from_base64() # Base64から画像を取得

    if image is None:
        print(f"エラー: タスクトレイアイコン画像の取得に失敗しました。アイコンは表示できません。")
        return

    # メニューアイテムの定義
    # MenuItemの第一引数にテキストを返す関数を渡すことで動的なテキストを実現
    menu = pystray.Menu(
        pystray.MenuItem(
            get_proxy_menu_text, # 現在の状態に応じてテキストが変わる
            toggle_proxy_action_menu # メニューアイテムからのアクション
        ),
        pystray.Menu.SEPARATOR,
        pystray.MenuItem(
            "終了",
            exit_action
        )
    )
    
    icon_object = pystray.Icon("proxy_toggler", image, "プロキシ設定トグラー", menu)
    tray_icon = icon_object # グローバル変数にアイコンオブジェクトを保持

    print("タスクトレイアイコンを起動します。右クリックでメニューを表示できます。")
    shortcut_str = format_shortcut_keys(SHORTCUT_KEY_COMBINATION)
    print(f"ショートカットキー ({shortcut_str}) でプロキシ設定を切り替えられます。")

    # キーボードリスナーを別スレッドで開始
    keyboard_listener = keyboard.Listener(on_press=on_key_press, on_release=on_key_release)
    keyboard_listener.start()

    try:
        icon_object.run() # タスクトレイのメインループを開始 (ブロッキング)
    finally:
        # icon.run() が終了した (通常は exit_action で icon.stop() が呼ばれた) 後に実行
        if keyboard_listener and keyboard_listener.is_alive():
            print("タスクトレイ終了後、キーボードリスナーを確実に停止します。")
            keyboard_listener.stop()
            # keyboard_listener.join() # joinするとメインスレッドが待機してしまう場合がある
        print("タスクトレイアプリケーションが終了しました。")

def main():
    if sys.platform != "win32":
        print("このスクリプトはWindows環境でのみ動作します。")
        return

    # EXEとして実行されている場合、PATH環境変数を更新
    update_path_environment_variable()

    # アプリケーション終了時に一時ファイルをクリーンアップする処理を登録
    atexit.register(_cleanup_temp_notification_icon)

    set_app_user_model_id(APP_ID) # AppUserModelIDを設定

    global win_notifier
    try:
        # Registryオブジェクトを作成してNotifierに渡す
        win_notifier = Notifier(registry=Registry(app_id=APP_ID))
    except Exception as e:
        print(f"エラー: winotify.Notifierの初期化に失敗しました: {e}")

    # 起動時のプロキシ状態を通知
    initial_proxy_status = get_proxy_status()
    initial_status_str = "有効" if initial_proxy_status else "無効"
    notification_title_startup = "Proxy Switcherを起動"
    notification_message_startup = f"現在のプロキシ設定: {initial_status_str}"
    show_windows_notification(notification_title_startup, notification_message_startup)

    setup_tray_icon()

if __name__ == "__main__":
    main()