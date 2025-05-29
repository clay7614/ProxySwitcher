# encode_icon_with_specific_resolution.py
import base64
import io
import os
from PIL import Image, ImageFile

ImageFile.LOAD_TRUNCATED_IMAGES = True # 破損した画像を読み込もうとする場合に役立つことがある

# --- 設定項目 ---
INPUT_ICON_FILENAME = "proxy_icon.png"  # 入力するICOファイル名
TARGET_RESOLUTION = (64,64)  # 抽出・エンコードしたい解像度 (幅, 高さ)
OUTPUT_VARIABLE_NAME = "ICON_BASE64_STRING"
# --- 設定項目ここまで ---

def get_specific_resolution_image_from_ico(icon_path, target_size):
    """
    ICOファイルから指定された解像度の画像をPIL.Imageオブジェクトとして抽出します。
    該当する解像度が見つからない場合はNoneを返します。
    """
    try:
        img = Image.open(icon_path)
        # .icoファイルの場合、img.size は最も大きい画像のサイズを返すことが多い
        # img.info['sizes'] や img.ico.sizes() で利用可能なサイズ一覧を取得できる場合がある
        # しかし、最も確実なのは、フレームをイテレートしてサイズを確認する方法
        
        # Pillow 9.1.0 以降では、img.seek() でフレームを移動し、img.size でそのフレームのサイズを取得可能
        # それ以前のバージョンでは、img.ico.getimage(size) のようなAPIがあったり、
        # img.n_frames でフレーム数を確認してループする方法がある
        
        print(f"ICOファイル '{icon_path}' を開きました。")
        print(f"利用可能なフレーム数: {getattr(img, 'n_frames', 1)}") # n_frames がない場合は1とする

        # まず、直接指定した解像度で取得できるか試す (PillowのバージョンやICOの構造による)
        try:
            # img.size がタプルであることを期待
            if hasattr(img, 'ico') and hasattr(img.ico, 'getimage'):
                 # 古いPillowのAPI (img.ico.getimage(target_size)) は現在推奨されないか、存在しない可能性
                 pass # この方法は信頼性が低いので、フレームイテレーションを優先

            # フレームをイテレートして探す
            # n_frames が存在しない場合 (単一画像ICOなど) も考慮
            num_frames = getattr(img, 'n_frames', 1)
            for i in range(num_frames):
                img.seek(i)
                current_frame_size = img.size
                print(f"  フレーム {i}: サイズ {current_frame_size}")
                if current_frame_size == target_size:
                    print(f"ターゲット解像度 {target_size} の画像が見つかりました (フレーム {i})。")
                    # 新しいImageオブジェクトとしてコピーを返すのが安全
                    return img.copy()
        except EOFError:
            # フレームの終端に達した場合
            pass
        except Exception as e_seek:
            print(f"フレームのイテレーション中にエラー: {e_seek}")


        # もし上記で見つからなければ、img.info['sizes'] を試す (存在すれば)
        if 'sizes' in img.info:
            print(f"img.info['sizes'] に含まれるサイズ: {img.info['sizes']}")
            if target_size in img.info['sizes']:
                 # この方法で直接画像を取得するのは難しい場合がある
                 # 通常はフレームイテレーションがより確実
                 print(f"警告: img.info['sizes'] に {target_size} が含まれていますが、フレームイテレーションでの抽出を推奨します。")


        print(f"エラー: ICOファイル '{icon_path}' 内に解像度 {target_size} の画像が見つかりませんでした。")
        # 利用可能なサイズを表示してユーザーにヒントを与える
        available_sizes = set()
        if num_frames > 1:
            for i in range(num_frames):
                img.seek(i)
                available_sizes.add(img.size)
        else: # 単一画像の場合
            available_sizes.add(img.size)
        if available_sizes:
            print(f"利用可能な解像度 (検出されたもの): {available_sizes}")
        return None

    except FileNotFoundError:
        print(f"エラー: アイコンファイル '{icon_path}' が見つかりません。")
        return None
    except Exception as e:
        print(f"アイコンファイルの読み込みまたは解析中にエラーが発生しました: {e}")
        return None

def encode_image_to_base64(pil_image, image_format="PNG"):
    """
    PIL.ImageオブジェクトをBase64エンコードされた文字列に変換します。
    ICO形式のままエンコードするより、PNGなどの標準形式に一度変換してからエンコードする方が安定することがあります。
    """
    if not pil_image:
        return None
    try:
        buffered = io.BytesIO()
        # ICO形式のままエンコードするか、PNGに変換してエンコードするか選択可能
        # PNGの方が一般的にBase64エンコード用途では安定
        pil_image.save(buffered, format=image_format)
        img_byte = buffered.getvalue()
        encoded_string = base64.b64encode(img_byte).decode('utf-8')
        print(f"画像を {image_format} 形式としてBase64エンコードしました。")
        return encoded_string
    except Exception as e:
        print(f"画像のBase64エンコード中にエラーが発生しました: {e}")
        return None

if __name__ == "__main__":
    # スクリプトのディレクトリを基準にアイコンファイルのパスを解決
    script_dir = os.path.dirname(os.path.abspath(__file__))
    icon_file_path = os.path.join(script_dir, INPUT_ICON_FILENAME)

    print(f"アイコンファイル '{icon_file_path}' から解像度 {TARGET_RESOLUTION} を抽出します...")
    
    # 指定解像度の画像を抽出
    extracted_image = get_specific_resolution_image_from_ico(icon_file_path, TARGET_RESOLUTION)
    
    if extracted_image:
        print(f"抽出された画像のサイズ: {extracted_image.size}, モード: {extracted_image.mode}")
        
        # 抽出した画像をBase64エンコード (PNGとしてエンコードすることを推奨)
        # ICO形式のままエンコードすると、デコード側で再度ICOパーサーが必要になり、
        # 複雑さが増す可能性があります。PNGならよりシンプル。
        base64_data = encode_image_to_base64(extracted_image, image_format="PNG")
        
        if base64_data:
            print(f"\n以下の行を proxy.py の {OUTPUT_VARIABLE_NAME} にコピー＆ペーストしてください:\n")
            print(f'{OUTPUT_VARIABLE_NAME} = """{base64_data}"""')
            print(f"\nコピーが完了したら、このスクリプトは不要です。")
        else:
            print("Base64エンコードに失敗しました。")
    else:
        print(f"指定された解像度 {TARGET_RESOLUTION} の画像の抽出に失敗しました。")

