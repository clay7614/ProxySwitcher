import winreg
import ctypes
from config_loader import config

class ProxyManager:
    def __init__(self, notification_manager):
        self.notification_manager = notification_manager

    def get_proxy_status(self):
        """現在のプロキシ設定状態を確認します。"""
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, config.INTERNET_SETTINGS_PATH, 0, winreg.KEY_READ) as key:
                value, _ = winreg.QueryValueEx(key, config.PROXY_ENABLE_VALUE_NAME)
                return bool(value)
        except FileNotFoundError:
            print(f"情報: プロキシ設定値 '{config.PROXY_ENABLE_VALUE_NAME}' が見つかりません。無効として扱います。")
            return False
        except Exception as e:
            print(f"プロキシ状態の読み取り中にエラーが発生しました: {e}")
            return False

    def _set_proxy_registry(self, enable: bool):
        """レジストリのプロキシ設定を変更します。"""
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, config.INTERNET_SETTINGS_PATH, 0, winreg.KEY_WRITE) as key:
                winreg.SetValueEx(key, config.PROXY_ENABLE_VALUE_NAME, 0, winreg.REG_DWORD, 1 if enable else 0)
            return True
        except PermissionError:
            print(f"エラー: レジストリへの書き込み権限がありません。管理者として実行する必要があるかもしれません。")
            return False
        except Exception as e:
            print(f"プロキシ状態の設定中にエラーが発生しました: {e}")
            return False

    def _refresh_internet_settings(self):
        """システムにインターネット設定の変更を通知し、リフレッシュします。"""
        try:
            internet_set_option = ctypes.windll.Wininet.InternetSetOptionW
            settings_changed_result = internet_set_option(None, config.INTERNET_OPTION_SETTINGS_CHANGED, None, 0)
            if not settings_changed_result:
                print(f"警告: システムへの設定変更通知に失敗しました (INTERNET_OPTION_SETTINGS_CHANGED)。エラーコード: {ctypes.GetLastError()}")
            else:
                print("システムに設定変更を通知しました (INTERNET_OPTION_SETTINGS_CHANGED)。")

            refresh_result = internet_set_option(None, config.INTERNET_OPTION_REFRESH, None, 0)
            if not refresh_result:
                print(f"警告: インターネット設定のリフレッシュに失敗しました (INTERNET_OPTION_REFRESH)。エラーコード: {ctypes.GetLastError()}")
            else:
                print("インターネット設定をリフレッシュしました (INTERNET_OPTION_REFRESH)。")
        except AttributeError:
            print("エラー: WinINetライブラリのロードに失敗しました。Windows環境で実行しているか確認してください。")
        except Exception as e:
            print(f"インターネット設定の更新中に予期せぬエラーが発生しました: {e}")

    def toggle_proxy(self):
        """
        プロキシ設定をトグルし、結果を通知します。
        成功した場合はTrue、失敗した場合はFalseを返します。
        """
        current_status = self.get_proxy_status()
        new_status = not current_status
        action_verb = "有効化" if new_status else "無効化"
        new_status_str = "有効" if new_status else "無効"

        print(f"プロキシ設定を {new_status_str} に変更します...")
        if self._set_proxy_registry(new_status):
            print(f"レジストリのプロキシ設定を {new_status_str} に変更しました。")
            self._refresh_internet_settings()
            print(f"プロキシ設定は正常に {new_status_str} に変更されました。")

            notification_title = "プロキシ設定変更"
            notification_message = f"プロキシが {new_status_str} になりました。"
            self.notification_manager.show_notification(notification_title, notification_message)
            return True
        else:
            print(f"プロキシ設定の {action_verb} に失敗しました。")
            self.notification_manager.show_notification("プロキシ設定エラー", f"プロキシの {action_verb} に失敗しました。")
            return False