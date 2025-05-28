import pystray
from PIL import Image
import icon_utils
from app_config_loader import config

class TrayManager:
    def __init__(self, proxy_manager, shortcut_handler):
        self.proxy_manager = proxy_manager
        self.shortcut_handler = shortcut_handler # ShortcutHandlerのインスタンスを保持
        self.icon = None
        self._icon_image = icon_utils.get_pil_image_from_base64(config.ICON_BASE64_STRING)

    def _get_proxy_menu_text(self, item):
        if self.proxy_manager.get_proxy_status():
            return "プロキシを無効にする"
        else:
            return "プロキシを有効にする"

    def _toggle_proxy_action_menu(self, icon_obj, item):
        self.proxy_manager.toggle_proxy()
        self.update_menu_text() # メニューテキストを即時更新

    def _exit_action(self, icon_obj, item):
        print("プログラムを終了します...")
        if self.shortcut_handler:
            print("キーボードリスナーを停止しています...")
            self.shortcut_handler.stop_listener()
        # NotificationManagerのクリーンアップはatexitで処理される
        if self.icon:
            self.icon.stop()

    def update_menu_text(self):
        if self.icon:
            self.icon.update_menu()

    def run(self):
        if not self._icon_image:
            print("エラー: タスクトレイアイコン画像の取得に失敗しました。タスクトレイは起動できません。")
            return

        menu = pystray.Menu(
            pystray.MenuItem(
                self._get_proxy_menu_text,
                self._toggle_proxy_action_menu
            ),
            pystray.Menu.SEPARATOR,
            pystray.MenuItem(
                "終了",
                self._exit_action
            )
        )

        self.icon = pystray.Icon(config.APP_ID.lower().replace(" ", "_"), self._icon_image, config.APP_ID, menu)

        print("タスクトレイアイコンを起動します。右クリックでメニューを表示できます。")
        if self.shortcut_handler:
            shortcut_str = self.shortcut_handler.format_shortcut_keys_display(config.SHORTCUT_KEY_COMBINATION)
            print(f"ショートカットキー ({shortcut_str}) でプロキシ設定を切り替えられます。")
            self.shortcut_handler.start_listener() # ここでリスナーを開始

        try:
            self.icon.run() # ブロッキング呼び出し
        finally:
            # icon.run() が終了した後 (通常は exit_action で icon.stop() が呼ばれた後)
            if self.shortcut_handler and self.shortcut_handler.listener and self.shortcut_handler.listener.is_alive():
                print("タスクトレイ終了後、キーボードリスナーを確実に停止します。")
                self.shortcut_handler.stop_listener()
            print("タスクトレイアプリケーションが終了しました。")