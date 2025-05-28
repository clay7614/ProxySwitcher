import configparser
from pynput import keyboard
import os

# このモジュールと同じディレクトリにある config.ini を指す
CONFIG_FILE_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'config.ini')

class AppConfig:
    def __init__(self, file_path):
        self.parser = configparser.ConfigParser()
        if not os.path.exists(file_path):
            # 設定ファイルが見つからない場合は、アプリケーションの起動を妨げる重大なエラーとする
            raise FileNotFoundError(f"設定ファイルが見つかりません: {file_path}")
        self.parser.read(file_path, encoding='utf-8') # UTF-8エンコーディングを指定
        self._load_values()

    def _parse_shortcut_string(self, shortcut_str):
        keys = []
        if not shortcut_str:
            return frozenset()
        
        # キー文字列をコンマで分割し、各パーツを小文字化して空白を除去
        key_parts = [k.strip().lower() for k in shortcut_str.split(',')]
        
        for part in key_parts:
            try:
                # pynput.keyboard.Key の特殊キー (例: ctrl_l, alt_l, shift, f1) かどうかを確認
                if hasattr(keyboard.Key, part):
                    keys.append(getattr(keyboard.Key, part))
                # 1文字のキー (例: p, q, 1) かどうかを確認
                elif len(part) == 1:
                    keys.append(keyboard.KeyCode.from_char(part))
                else:
                    # 不明なキーパーツの場合はエラー
                    raise ValueError(f"ショートカットキーの文字列 '{part}' は認識できません。")
            except Exception as e:
                # パース中にエラーが発生した場合は、より詳細な情報と共にエラーを再送出
                raise ValueError(f"ショートカットキー '{part}' の解析中にエラーが発生しました: {e}") from e
        return frozenset(keys)

    def _load_values(self):
        # Applicationセクション
        self.APP_ID = self.parser.get('Application', 'ID', fallback="ProxySwitcher")

        # Registryセクション
        self.INTERNET_SETTINGS_PATH = self.parser.get('Registry', 'InternetSettingsPath', fallback=r"Software\Microsoft\Windows\CurrentVersion\Internet Settings")
        self.PROXY_ENABLE_VALUE_NAME = self.parser.get('Registry', 'ProxyEnableValueName', fallback="ProxyEnable")

        # WinINetセクション (整数値として読み込み)
        self.INTERNET_OPTION_SETTINGS_CHANGED = self.parser.getint('WinINet', 'OptionSettingsChanged', fallback=39)
        self.INTERNET_OPTION_REFRESH = self.parser.getint('WinINet', 'OptionRefresh', fallback=37)

        # Shortcutセクション
        shortcut_str = self.parser.get('Shortcut', 'KeyCombination', fallback="")
        self.SHORTCUT_KEY_COMBINATION = self._parse_shortcut_string(shortcut_str)

        # Environmentセクション
        self.ENVIRONMENT_KEY_PATH = self.parser.get('Environment', 'KeyPath', fallback="Environment")
        self.PATH_VALUE_NAME = self.parser.get('Environment', 'PathValueName', fallback="Path")

        # WindowsAPIセクション (16進数/10進数文字列を整数に変換)
        self.HWND_BROADCAST = int(self.parser.get('WindowsAPI', 'HwndBroadcast', fallback="0xFFFF"), 0)
        self.WM_SETTINGCHANGE = int(self.parser.get('WindowsAPI', 'WmSettingChange', fallback="0x001A"), 0)
        self.SMTO_ABORTIFHUNG = int(self.parser.get('WindowsAPI', 'SmtoAbortIfHung', fallback="0x0002"), 0)

        # Iconセクション
        self.ICON_BASE64_STRING = self.parser.get('Icon', 'Base64String', fallback="")

# アプリケーション全体で共有される設定インスタンス
# モジュールインポート時に一度だけロードされる
try:
    config = AppConfig(CONFIG_FILE_PATH)
except FileNotFoundError as e:
    print(f"エラー: {e}。設定ファイルなしではアプリケーションを起動できません。")
    raise
except Exception as e:
    print(f"エラー: 設定ファイルの読み込みまたは解析に失敗しました: {e}")
    raise