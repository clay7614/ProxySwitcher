# notification_manager.py
import os
import tempfile
import atexit
from winotify import Notification, Notifier, Registry
import icon_utils # icon_utils.py から関数をインポート
import config     # config.py から定数をインポート

class NotificationManager:
    def __init__(self, app_id, icon_base64_string):
        self.app_id = app_id
        self.icon_base64_string = icon_base64_string
        self._temp_notification_icon_path = None
        self.win_notifier = None
        try:
            # winotify.Registry を使用してNotifierを初期化
            # app_id は Notifier ではなく Registry に渡す
            registry = Registry(app_id=self.app_id) # app_name引数を削除
            self.win_notifier = Notifier(registry)
        except Exception as e:
            print(f"エラー: winotify.Notifierの初期化に失敗しました: {e}")

        atexit.register(self.cleanup_temp_icon)

    def _get_icon_path(self):
        if not self.icon_base64_string:
            return ""

        if self._temp_notification_icon_path and os.path.exists(self._temp_notification_icon_path):
            return self._temp_notification_icon_path

        pil_image = icon_utils.get_pil_image_from_base64(self.icon_base64_string)
        if pil_image:
            try:
                fd, temp_path = tempfile.mkstemp(suffix=".ico", prefix=f"{self.app_id}_notify_icon_")
                os.close(fd)
                pil_image.save(temp_path, format="ICO", sizes=[(64,64)]) # ICO形式で保存
                self._temp_notification_icon_path = temp_path
                print(f"通知用アイコンを一時ファイルに保存しました: {self._temp_notification_icon_path}")
                return self._temp_notification_icon_path
            except Exception as e:
                print(f"エラー: 通知用一時アイコンの作成/保存に失敗しました: {e}")
                self._temp_notification_icon_path = None
        return ""

    def show_notification(self, title, message):
        if not self.win_notifier:
            print("警告: Windows通知用のNotifierが初期化されていません。")
            return

        icon_path = self._get_icon_path()
        try:
            toast = Notification(app_id=self.app_id, # Notifier初期化時のapp_idが使われるはずだが、明示
                                title=title,
                                msg=message,
                                icon=icon_path,
                                duration='short')
            toast.show()
        except Exception as e:
            print(f"Windows通知の表示中にエラーが発生しました: {e}")

    def cleanup_temp_icon(self):
        if self._temp_notification_icon_path and os.path.exists(self._temp_notification_icon_path):
            try:
                os.remove(self._temp_notification_icon_path)
                print(f"一時通知アイコンファイルを削除しました: {self._temp_notification_icon_path}")
            except PermissionError:
                print(f"警告: 一時通知アイコンファイル '{self._temp_notification_icon_path}' は使用中のため削除できませんでした。")
            except Exception as e:
                print(f"エラー: 一時通知アイコンファイル '{self._temp_notification_icon_path}' のクリーンアップに失敗しました: {e}")
            finally:
                self._temp_notification_icon_path = None