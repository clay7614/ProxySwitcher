from pynput import keyboard
# config は main.py から渡されるため、直接の import は不要

class ShortcutHandler:
    def __init__(self, shortcut_keys, proxy_manager, tray_manager):
        self.shortcut_keys_combination = shortcut_keys
        self.proxy_manager = proxy_manager
        self.tray_manager = tray_manager
        self.current_pressed_keys = set()
        self.listener = None

    @staticmethod
    def format_shortcut_keys_display(keys_iterable):
        """pynputのキーのイテラブルを人間が読める形式の文字列に変換します。"""
        display_parts = []
        # keyboard.Key.ctrl など、左右を区別しないキー定数も考慮
        ctrl_keys = {keyboard.Key.ctrl, keyboard.Key.ctrl_l, keyboard.Key.ctrl_r}
        alt_keys = {keyboard.Key.alt, keyboard.Key.alt_l, keyboard.Key.alt_r}
        shift_keys = {keyboard.Key.shift, keyboard.Key.shift_l, keyboard.Key.shift_r}

        # Modifierキーの存在を確認
        if any(k in keys_iterable for k in ctrl_keys): display_parts.append("Ctrl")
        if any(k in keys_iterable for k in alt_keys): display_parts.append("Alt")
        if any(k in keys_iterable for k in shift_keys): display_parts.append("Shift")

        # 通常キーを追加
        for key_obj in keys_iterable:
            is_modifier = False
            if isinstance(key_obj, keyboard.Key): # pynput.keyboard.Key オブジェクトの場合
                if key_obj in ctrl_keys or key_obj in alt_keys or key_obj in shift_keys:
                    is_modifier = True

            if not is_modifier:
                if isinstance(key_obj, keyboard.KeyCode) and key_obj.char:
                    display_parts.append(key_obj.char.upper())
                elif isinstance(key_obj, keyboard.Key): # Fキーなど、Enumメンバーの場合
                    display_parts.append(str(key_obj).replace("Key.", "").capitalize())

        return " + ".join(sorted(list(set(display_parts)))) # 重複を除きソートして結合

    def _on_press(self, key):
        if key in self.shortcut_keys_combination:
            self.current_pressed_keys.add(key)
            if self.shortcut_keys_combination.issubset(self.current_pressed_keys):
                shortcut_str = self.format_shortcut_keys_display(self.shortcut_keys_combination)
                print(f"ショートカットキー ({shortcut_str}) が押されました。プロキシを切り替えます。")
                if self.proxy_manager.toggle_proxy():
                    if self.tray_manager: # tray_manager が設定されていればメニューを更新
                        self.tray_manager.update_menu_text()
                self.current_pressed_keys.clear()

    def _on_release(self, key):
        if key in self.current_pressed_keys:
            self.current_pressed_keys.remove(key)

    def start_listener(self):
        self.listener = keyboard.Listener(on_press=self._on_press, on_release=self._on_release)
        self.listener.start()

    def stop_listener(self):
        if self.listener:
            self.listener.stop()