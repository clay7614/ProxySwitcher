# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['main.py'],
    pathex=[],
    binaries=[],
    datas=[
        ('proxy_icon.ico', '.'),  # アイコンファイルをexeに含める
        ('version_info.txt', '.'),  # バージョン情報ファイルを含める
    ],    hiddenimports=[
        'pyparsing',  # pkg_resourcesで必要
        'packaging',  # pyparsing関連
        'pkg_resources',  # PyInstallerで必要
        'PIL.ImageColor',  # ImageDrawで使用される
        'PIL.ImageDraw',  # 明示的に含める
        'PIL.Image',  # 基本モジュール
        'PIL.ImageFile',  # 画像保存で必要
        'PIL.PngImagePlugin',  # PNG形式で必要
        'PIL.BmpImagePlugin',  # BMP形式で必要
        'PIL.IcoImagePlugin',  # ICO形式で必要
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],    excludes=[
        # 本当に不要なモジュールのみを除外
        'matplotlib',
        'numpy',
        'scipy',
        'pandas',
        'IPython',
        'jupyter',
        'notebook',
        'tornado',
        'zmq',
        'sqlite3',
        'unittest',
        'test',
        'tests',
        'pydoc',
        'doctest',
        'lib2to3',
        # tkinterの不要な部分のみ除外
        'tkinter.test',
        'tkinter.scrolledtext',
        'tkinter.colorchooser',
        'tkinter.filedialog',
        'tkinter.font',
        'tkinter.simpledialog',
        'tkinter.dnd',        # PILの不要な機能のみ除外（pystrayが依存するものは除外しない）
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='ProxySwitcher',
    debug=False,
    bootloader_ignore_signals=False,
    strip=True,  # デバッグシンボルを削除
    upx=True,   # UPXで圧縮
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    version='version_info.txt',
    icon='proxy_icon.ico',
    optimize=2,  # Python最適化レベル2
    noupx=False,  # UPX圧縮を有効にする
)
