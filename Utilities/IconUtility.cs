using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProxySwitcher.Utilities;

public static class IconUtility
{
    private static readonly string IconOnPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "icon_on.ico");
    private static readonly string IconOffPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "icon_off.ico");

    // フォールバック用のパス（ビルド形式によってディレクトリ構造が変わる場合）
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
        catch
        {
            // ロード失敗時は動的生成へ
        }

        // ファイルがない場合のフォールバック（動的生成）
        return GenerateStatusIcon(enabled);
    }

    private static Icon GenerateStatusIcon(bool enabled)
    {
        Bitmap bitmap = new Bitmap(64, 64);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            Color color1 = enabled ? Color.LimeGreen : Color.Crimson;
            Color color2 = enabled ? Color.ForestGreen : Color.DarkRed;
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(8, 8, 48, 48), color1, color2, 45f))
            {
                g.FillEllipse(brush, 8, 8, 48, 48);
            }

            using (Pen pen = new Pen(enabled ? Color.DarkGreen : Color.Maroon, 2))
            {
                g.DrawEllipse(pen, 8, 8, 48, 48);
            }

            using (var glossBrush = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                g.FillEllipse(glossBrush, 18, 14, 28, 15);
            }
            
            using (Pen symbolPen = new Pen(Color.White, 3))
            {
                g.DrawLine(symbolPen, 24, 40, 40, 40);
                g.FillEllipse(Brushes.White, 20, 37, 6, 6);
                g.FillEllipse(Brushes.White, 38, 37, 6, 6);
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
