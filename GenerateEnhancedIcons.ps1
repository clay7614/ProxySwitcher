$code = @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

public class EnhancedIconGeneratorV7
{
    public static void Generate(bool isProxyEnabled, string path)
    {
        int size = 128; 
        using (var bmp = new Bitmap(size, size))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Colors - Intermediate Tones (Exact preservation via PNG-ICO)
            Color baseColor = isProxyEnabled 
                ? Color.FromArgb(17, 170, 17)   // Intermediate Green
                : Color.FromArgb(189, 17, 17);  // Intermediate Red
            
            // Design Parameters
            float scale = size / 256f;
            RectangleF rect = new RectangleF(20 * scale, 20 * scale, 216 * scale, 216 * scale);
            float radius = 48 * scale;
            
            // Draw Background
            using (var pathObj = GetRoundedRect(rect, radius))
            using (var brush = new SolidBrush(baseColor))
            {
                g.FillPath(brush, pathObj);
            }

            // Draw Connection Line
            PointF p1 = new PointF(85 * scale, 165 * scale);
            PointF p2 = new PointF(171 * scale, 91 * scale);
            PointF c1 = new PointF(128 * scale, 165 * scale);
            PointF c2 = new PointF(128 * scale, 91 * scale);

            using (var pen = new Pen(Color.FromArgb(230, 255, 255, 255), 12 * scale))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawBezier(pen, p1, c1, c2, p2);
            }

            // Draw Nodes
            float nodeRadius = 24 * scale;
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, p1.X - nodeRadius, p1.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                g.FillEllipse(brush, p2.X - nodeRadius, p2.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
            }
            
            // Center Dot
            float centerRadius = 8 * scale;
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                 g.FillEllipse(brush, (128 * scale) - centerRadius, (128 * scale) - centerRadius, centerRadius * 2, centerRadius * 2);
            }

            // Save as PNG-based ICO to preserve true colors
            SaveAsIcon(bmp, path);
        }
        Console.WriteLine("Generated: " + path);
    }

    private static GraphicsPath GetRoundedRect(RectangleF rect, float radius)
    {
        float diameter = radius * 2;
        Size size = new Size((int)diameter, (int)diameter);
        RectangleF arc = new RectangleF(rect.Location, size);
        GraphicsPath path = new GraphicsPath();
        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static void SaveAsIcon(Bitmap bmp, string path)
    {
        using (var fs = new FileStream(path, FileMode.Create))
        using (var ms = new MemoryStream())
        {
            // Save Bitmap as PNG to memory
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] pngData = ms.ToArray();

            using (var writer = new BinaryWriter(fs))
            {
                // ICO Header
                writer.Write((ushort)0);      // Reserved
                writer.Write((ushort)1);      // Type 1 = Icon
                writer.Write((ushort)1);      // Count = 1 image

                // Directory Entry
                writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));   // Width
                writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height)); // Height
                writer.Write((byte)0);        // Colors (0 = no palette)
                writer.Write((byte)0);        // Reserved
                writer.Write((ushort)1);      // Planes
                writer.Write((ushort)32);     // BPP
                writer.Write((uint)pngData.Length); // Image Size
                writer.Write((uint)22);       // Image Offset (6 + 16)

                // Image Data (PNG)
                writer.Write(pngData);
            }
        }
    }
}
'@

Add-Type -TypeDefinition $code -ReferencedAssemblies System.Drawing

# Ensure directory exists
$dir = "Utilities"
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }

# Generate ON Icon (Green Theme)
[EnhancedIconGeneratorV7]::Generate($true, "$dir\icon_on.ico")

# Generate OFF Icon (Red Theme)
[EnhancedIconGeneratorV7]::Generate($false, "$dir\icon_off.ico")
