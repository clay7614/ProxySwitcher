# main.py
import sys
import os # update_path_environment_variable で使用

import config
import windows_utils
from proxy_manager import ProxyManager
from notification_manager import NotificationManager
from shortcut_handler import ShortcutHandler
from tray_manager import TrayManager

def main():
    if sys.platform != "win32":
        print("このスクリプトはWindows環境でのみ動作します。")
        return

    # EXEとして実行されている場合、PATH環境変数を更新
    windows_utils.update_path_environment_variable()

    # AppUserModelIDを設定 (通知に影響)
    windows_utils.set_app_user_model_id(config.APP_ID)

    # マネージャークラスの初期化
    # NotificationManager は atexit で自身のクリーンアップを登録
    notification_mgr = NotificationManager(config.APP_ID, config.ICON_BASE64_STRING)
    proxy_mgr = ProxyManager(notification_mgr)
    
    # ShortcutHandler と TrayManager は相互に参照する場合があるため、
    # TrayManager が ShortcutHandler を保持し、リスナーの開始/停止を管理する
    shortcut_hdlr = ShortcutHandler(config.SHORTCUT_KEY_COMBINATION, proxy_mgr, None) # TrayManagerは後で設定
    tray_mgr = TrayManager(proxy_mgr, shortcut_hdlr)
    shortcut_hdlr.tray_manager = tray_mgr # TrayManagerのインスタンスをShortcutHandlerに設定

    # 起動時のプロキシ状態を通知 (NotificationManager経由で)
    initial_proxy_status = proxy_mgr.get_proxy_status()
    initial_status_str = "有効" if initial_proxy_status else "無効"
    notification_title_startup = "Proxy Switcherを起動"
    notification_message_startup = f"現在のプロキシ設定: {initial_status_str}"
    notification_mgr.show_notification(notification_title_startup, notification_message_startup)

    # タスクトレイを開始 (これがメインループになる)
    tray_mgr.run()

if __name__ == "__main__":
    main()