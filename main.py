import threading
import winreg
import ctypes
import pystray
from PIL import Image, ImageDraw
import keyboard
import tkinter as tk
from tkinter import messagebox
import json
import os
import sys
import subprocess
import time

# exe化時のリソースファイルパス取得関数
def get_resource_path(relative_path):
    """リソースファイルの正しいパスを取得（exe化対応）"""
    if hasattr(sys, '_MEIPASS'):
        # PyInstallerでexe化された場合
        appdata = os.environ.get('APPDATA', os.path.expanduser('~'))
        config_dir = os.path.join(appdata, 'ProxySwitcher')
        os.makedirs(config_dir, exist_ok=True)
        return os.path.join(config_dir, relative_path)
    else:
        # 通常のPythonスクリプト実行時
        return os.path.join(os.path.dirname(os.path.abspath(__file__)), relative_path)

CONFIG_FILE = get_resource_path('proxy_config.json')
ICON_FILE = get_resource_path('proxy_icon.ico')

# デフォルト設定
DEFAULT_CONFIG = {
    'proxy_server': 'proxy2.maizuru-ct.ac.jp:8090'
}

# 設定を読み込む関数
def load_config():
    if os.path.exists(CONFIG_FILE):
        try:
            with open(CONFIG_FILE, 'r', encoding='utf-8') as f:
                return json.load(f)
        except:
            pass
    return DEFAULT_CONFIG.copy()

# 設定を保存する関数
def save_config(config):
    with open(CONFIG_FILE, 'w', encoding='utf-8') as f:
        json.dump(config, f, ensure_ascii=False, indent=2)

# トレイアイコン用の画像を生成する関数
def create_image(enabled=True):
    # フォールバック: 動的にアイコンを生成
    width = 64
    height = 64
    image = Image.new('RGBA', (width, height), color=(0, 0, 0, 0))
    dc = ImageDraw.Draw(image)
    
    # 円形でステータスを表示（緑=ON, 赤=OFF）
    color = (0, 255, 0, 255) if enabled else (255, 0, 0, 255)
    dc.ellipse((width//2 - 20, height//2 - 20, width//2 + 20, height//2 + 20), fill=color)
    
    border_color = (0, 200, 0, 255) if enabled else (200, 0, 0, 255)
    dc.ellipse((width//2 - 20, height//2 - 20, width//2 + 20, height//2 + 20), outline=border_color, width=2)
    
    return image

# 現在のプロキシ設定を取得する関数
def is_proxy_enabled():
    REG_PATH = r'Software\Microsoft\Windows\CurrentVersion\Internet Settings'
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REG_PATH) as key:
            value, _ = winreg.QueryValueEx(key, 'ProxyEnable')
            return bool(value)
    except Exception:
        return False

# プロキシ設定をON/OFFする関数
def set_proxy(enable):
    REG_PATH = r'Software\Microsoft\Windows\CurrentVersion\Internet Settings'
    config = load_config()
    server = config['proxy_server']
    
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REG_PATH, 0, winreg.KEY_SET_VALUE) as key:
        winreg.SetValueEx(key, 'ProxyEnable', 0, winreg.REG_DWORD, 1 if enable else 0)
        if enable:
            winreg.SetValueEx(key, 'ProxyServer', 0, winreg.REG_SZ, server)
    # 設定を即時反映させる
    ctypes.windll.Wininet.InternetSetOptionW(0, 39, 0, 0)  # SETTINGS_CHANGED
    ctypes.windll.Wininet.InternetSetOptionW(0, 37, 0, 0)  # REFRESH

# トグル処理
def toggle_proxy(icon=None, item=None):
    enabled = not is_proxy_enabled()
    set_proxy(enabled)
    # アイコンを更新して通知
    icon.icon = create_image(enabled)
    status = 'ON' if enabled else 'OFF'
    icon.notify(f'プロキシの状態： {status}')
    # メニューを更新
    icon.update_menu()

# ホットキーリスナー
def keyboard_listener(icon):
    def register_hotkey():
        try:
            # 既存のホットキーをクリア（エラーを無視）
            try:
                keyboard.remove_hotkey('ctrl+alt+p')
            except:
                pass
            
            # ホットキーを登録
            keyboard.add_hotkey('ctrl+alt+p', lambda: toggle_proxy(icon))
            print("ショートカットキー (Ctrl+Alt+P) を登録しました")
        except Exception as e:
            print(f"ショートカットキーの登録に失敗しました: {e}")
    
    # 初回登録
    register_hotkey()
    
    # 1分ごとに再登録
    while True:
        try:
            time.sleep(30)  # 30秒待機
            register_hotkey()
        except KeyboardInterrupt:
            break
        except Exception as e:
            print(f"ショートカットキーの再登録中にエラーが発生しました: {e}")
            time.sleep(30)  # エラーが発生しても30秒後に再試行

# 設定ダイアログクラス
class ProxySettingsDialog:
    def __init__(self, icon):
        self.icon = icon
        self.config = load_config()
        self.root = None
    def show_dialog(self):
        try:
            # tkinterのルートウィンドウを作成
            self.root = tk.Tk()
            self.root.title("ProxySwitcher 設定")
            self.root.geometry("450x300")
            self.root.resizable(False, False)
            
            # メインフレーム
            main_frame = tk.Frame(self.root, padx=20, pady=20)
            main_frame.pack(fill=tk.BOTH, expand=True)
            
            # プロキシサーバー設定
            proxy_frame = tk.LabelFrame(main_frame, text="プロキシ設定", padx=10, pady=10)
            proxy_frame.pack(fill=tk.X, pady=(0, 15))
            
            tk.Label(proxy_frame, text="プロキシサーバー:").pack(anchor=tk.W)
            self.server_var = tk.StringVar(value=self.config['proxy_server'])
            server_entry = tk.Entry(proxy_frame, textvariable=self.server_var, width=50)
            server_entry.pack(fill=tk.X, pady=(5, 10))
            
            # ローカルアドレス設定
            self.local_var = tk.BooleanVar(value=self.config.get('use_proxy_for_local', False))
            
            # システム設定
            system_frame = tk.LabelFrame(main_frame, text="システム設定", padx=10, pady=10)
            system_frame.pack(fill=tk.X, pady=(0, 15))
            
            # 自動起動設定
            self.autostart_var = tk.BooleanVar(value=is_autostart_enabled())
            autostart_check = tk.Checkbutton(system_frame, text="Windowsスタートアップ時に自動起動", 
                                           variable=self.autostart_var)
            autostart_check.pack(anchor=tk.W)
            
            # ボタンフレーム
            button_frame = tk.Frame(main_frame)
            button_frame.pack(fill=tk.X, pady=(10, 0))
            
            # 保存ボタン
            save_btn = tk.Button(button_frame, text="保存", command=self.save_settings)
            save_btn.pack(side=tk.RIGHT, padx=(5, 0))
            
            # キャンセルボタン
            cancel_btn = tk.Button(button_frame, text="キャンセル", command=self.root.destroy)
            cancel_btn.pack(side=tk.RIGHT)
            
            # ウィンドウを中央に配置
            self.root.update_idletasks()
            x = (self.root.winfo_screenwidth() // 2) - (self.root.winfo_width() // 2)
            y = (self.root.winfo_screenheight() // 2) - (self.root.winfo_height() // 2)
            self.root.geometry(f"+{x}+{y}")
            
            # ウィンドウを最前面に表示
            self.root.lift()
            self.root.attributes('-topmost', True)
            self.root.after_idle(lambda: self.root.attributes('-topmost', False))
            self.root.focus_force()
            
            print("設定ダイアログを表示します")  # デバッグ用
            self.root.mainloop()
        except Exception as e:
            print(f"ダイアログ表示エラー: {e}")
            if hasattr(self, 'root') and self.root:
                self.root.destroy()
    
    def save_settings(self):
        server = self.server_var.get().strip()
        if not server:
            messagebox.showerror("エラー", "プロキシサーバーを入力してください")
            return
            
        # プロキシ設定を保存
        self.config['proxy_server'] = server
        save_config(self.config)
        
        # 自動起動設定を保存
        autostart_enabled = self.autostart_var.get()
        if set_autostart(autostart_enabled):
            autostart_msg = "有効" if autostart_enabled else "無効"
            print(f"自動起動設定: {autostart_msg}")
        else:
            messagebox.showwarning("警告", "自動起動設定の変更に失敗しました")
        
        messagebox.showinfo("設定保存", "設定が保存されました")
        self.root.destroy()

# 自動起動機能
def get_autostart_registry_key():
    """自動起動用のレジストリキーを取得"""
    return r'Software\Microsoft\Windows\CurrentVersion\Run'

def is_autostart_enabled():
    """自動起動が有効かどうかをチェック"""
    try:
        key_path = get_autostart_registry_key()
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path) as key:
            try:
                winreg.QueryValueEx(key, 'ProxySwitcher')
                return True
            except FileNotFoundError:
                return False
    except Exception:
        return False

def set_autostart(enable):
    """自動起動を有効/無効にする"""
    try:
        key_path = get_autostart_registry_key()
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path, 0, winreg.KEY_SET_VALUE) as key:
            if enable:
                # 実行ファイルのパスを取得
                if hasattr(sys, '_MEIPASS'):
                    # exe化されている場合
                    exe_path = sys.executable
                else:
                    # Pythonスクリプトの場合
                    exe_path = f'"{sys.executable}" "{os.path.abspath(__file__)}"'
                winreg.SetValueEx(key, 'ProxySwitcher', 0, winreg.REG_SZ, exe_path)
            else:
                try:
                    winreg.DeleteValue(key, 'ProxySwitcher')
                except FileNotFoundError:
                    pass  # 既に削除されている
        return True
    except Exception as e:
        print(f"自動起動設定エラー: {e}")
        return False

# メイン処理
def main():
    # 起動時のプロキシ状態を取得・アイコン設定
    initial = is_proxy_enabled()
    settings_dialog = ProxySettingsDialog(None)
    
    # トグルメニューのテキスト関数
    def get_toggle_text(item):
        return 'プロキシをOFF' if is_proxy_enabled() else 'プロキシをON'    # 設定ダイアログを開く関数
    def open_settings(icon, item):
        try:
            # 設定ダイアログを直接開く
            dialog = ProxySettingsDialog(icon)
            # 別スレッドでダイアログを表示
            def show_dialog_thread():
                dialog.show_dialog()
            
            threading.Thread(target=show_dialog_thread, daemon=True).start()
        except Exception as e:
            print(f"設定ダイアログエラー: {e}")
            # フォールバック: 別プロセスで開く
            subprocess.Popen([sys.executable, __file__, '--settings'])
    
    icon = pystray.Icon(
        'ProxySwitcher',
        create_image(initial),
        '',
        menu=pystray.Menu(
            pystray.MenuItem(get_toggle_text, toggle_proxy),
            pystray.Menu.SEPARATOR,
            pystray.MenuItem('設定', open_settings),
            pystray.MenuItem('終了', lambda icon, item: icon.stop())
        )
    )
    
    # 起動時の通知を遅延実行する関数
    def startup_notification():
        import time
        time.sleep(1)  # アイコンが完全に初期化されるまで待機
        status = 'ON' if initial else 'OFF'
        icon.notify(f'プロキシの状態： {status}')
    
    # ホットキーリスナーをバックグラウンドで起動
    listener = threading.Thread(target=keyboard_listener, args=(icon,), daemon=True)
    listener.start()
    
    # 起動時通知をバックグラウンドで実行
    notification_thread = threading.Thread(target=startup_notification, daemon=True)
    notification_thread.start()
    
    icon.run()

if __name__ == '__main__':
    # コマンドライン引数をチェック
    if len(sys.argv) > 1 and sys.argv[1] == '--settings':
        # 設定ダイアログのみを開く
        dialog = ProxySettingsDialog(None)
        dialog.show_dialog()
    else:
        # 通常のトレイアプリケーションを開始
        main()
