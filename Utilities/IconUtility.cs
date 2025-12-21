using System;
using System.Drawing;
using System.IO;

namespace ProxySwitcher.Utilities;

public static class IconUtility
{
    private static readonly string IconOnPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "icon_on.ico");
    private static readonly string IconOffPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "icon_off.ico");

    // フォールバック用のパス（ルートディレクトリ）
    private static readonly string IconOnPathFallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon_on.ico");
    private static readonly string IconOffPathFallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon_off.ico");

    public static Icon CreateStatusIcon(bool enabled)
    {
        string primaryPath = enabled ? IconOnPath : IconOffPath;
        string fallbackPath = enabled ? IconOnPathFallback : IconOffPathFallback;

        try
        {
            if (File.Exists(primaryPath))
            {
                return new Icon(primaryPath);
            }
            if (File.Exists(fallbackPath))
            {
                return new Icon(fallbackPath);
            }
        }
        catch { /* ロード失敗時はデフォルトアイコンへ */ }

        // ファイルがない場合はアプリケーション自体のアイコンを返す
        return Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath) ?? SystemIcons.Application;
    }
}
