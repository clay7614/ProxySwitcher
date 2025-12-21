using System.Drawing;

namespace ProxySwitcher.Utilities;

public static class IconUtility
{
    public static Icon CreateStatusIcon(bool enabled)
    {
        // 高品質なアイコン画像を動的に生成
        Bitmap bitmap = new Bitmap(64, 64);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // 背景の円（グラデーション）
            Color color1 = enabled ? Color.LimeGreen : Color.Crimson;
            Color color2 = enabled ? Color.ForestGreen : Color.DarkRed;
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(8, 8, 48, 48), color1, color2, 45f))
            {
                g.FillEllipse(brush, 8, 8, 48, 48);
            }

            // 外枠
            using (Pen pen = new Pen(enabled ? Color.DarkGreen : Color.Maroon, 2))
            {
                g.DrawEllipse(pen, 8, 8, 48, 48);
            }

            // 反射光（グロス感）
            using (var glossBrush = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                g.FillEllipse(glossBrush, 18, 14, 28, 15);
            }
            
            // 中央のシンボル（簡易的なプロキシ/ネットワークイメージ）
            using (Pen symbolPen = new Pen(Color.White, 3))
            {
                // 横棒（接続をイメージ）
                g.DrawLine(symbolPen, 24, 40, 40, 40);
                // 点（ノードをイメージ）
                g.FillEllipse(Brushes.White, 20, 37, 6, 6);
                g.FillEllipse(Brushes.White, 38, 37, 6, 6);
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
