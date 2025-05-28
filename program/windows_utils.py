# windows_utils.py
import ctypes
import sys
import os
import winreg
import config

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
        lpdwResult = ctypes.c_ulong()
        result = ctypes.windll.user32.SendMessageTimeoutW(
            config.HWND_BROADCAST,
            config.WM_SETTINGCHANGE,
            0,
            "Environment",
            config.SMTO_ABORTIFHUNG,
            5000,
            ctypes.byref(lpdwResult)
        )
        if result == 0:
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

        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, config.ENVIRONMENT_KEY_PATH, 0, winreg.KEY_READ | winreg.KEY_WRITE) as env_key:
            try:
                current_path_value, reg_type = winreg.QueryValueEx(env_key, config.PATH_VALUE_NAME)
            except FileNotFoundError:
                current_path_value = ""
                reg_type = winreg.REG_EXPAND_SZ
                print(f"情報: 環境変数 '{config.PATH_VALUE_NAME}' が見つかりません。新規に作成します。")

            original_paths_list = [os.path.normpath(p.strip()) for p in current_path_value.split(';') if p.strip()]
            
            paths_after_removal = [
                p for p in original_paths_list
                if not (os.path.normcase(p) != os.path.normcase(current_script_dir) and
                        os.path.isfile(os.path.join(p, script_filename)))
            ]
            removed_old_paths = len(original_paths_list) != len(paths_after_removal)

            final_path_list = [current_script_dir] + [p for p in paths_after_removal if os.path.normcase(p) != os.path.normcase(current_script_dir)]
            final_path_list = list(dict.fromkeys(final_path_list))

            path_actually_changed = (original_paths_list != final_path_list) or removed_old_paths

            if path_actually_changed:
                new_path_value = ";".join(final_path_list)
                target_reg_type = reg_type if current_path_value else winreg.REG_EXPAND_SZ
                winreg.SetValueEx(env_key, config.PATH_VALUE_NAME, 0, target_reg_type, new_path_value)
                print(f"環境変数 '{config.PATH_VALUE_NAME}' を更新しました。新しい値: {new_path_value}")
                broadcast_env_change()
            else:
                print(f"環境変数 '{config.PATH_VALUE_NAME}' は既に最新の状態です。変更はありません。")
    except PermissionError:
        print(f"エラー: 環境変数 '{config.PATH_VALUE_NAME}' への書き込み権限がありません。管理者として実行する必要があるかもしれません。")
    except FileNotFoundError:
        print(f"エラー: 環境変数レジストリキー '{config.ENVIRONMENT_KEY_PATH}' が見つかりません。")
    except Exception as e:
        print(f"環境変数 '{config.PATH_VALUE_NAME}' の更新中に予期せぬエラーが発生しました: {e}", file=sys.stderr)