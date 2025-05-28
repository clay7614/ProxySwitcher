# icon_utils.py
import base64
import io
from PIL import Image, ImageFile

ImageFile.LOAD_TRUNCATED_IMAGES = True

def get_pil_image_from_base64(base64_string):
    """
    Base64エンコードされた文字列からPIL.Imageオブジェクトを生成して返します。
    """
    try:
        image_data = base64.b64decode(base64_string)
        pil_image = Image.open(io.BytesIO(image_data))
        return pil_image
    except Exception as e:
        print(f"Base64からのアイコン画像生成中にエラー: {e}")
        return None