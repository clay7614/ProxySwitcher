using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySwitcher;

internal static class Program
{
    private static NotifyIcon? _trayIcon;
    private static HotKeyHandler? _hotKeyHandler;
    private static WifiWatcher? _wifiWatcher;
    private static AppConfig _config = new();

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        _config = AppConfig.Load();
        
        // トレイアイコンの作成
        _trayIcon = new NotifyIcon()
        {
            Icon = CreateStatusIcon(ProxyManager.IsProxyEnabled()),
            ContextMenuStrip = CreateContextMenu(),
            Visible = true,
            Text = "ProxySwitcher"
        };

        // ホットキーの登録
        _hotKeyHandler = new HotKeyHandler();
        _hotKeyHandler.HotKeyPressed += ToggleProxy;

        // WiFi監視の開始
        _wifiWatcher = new WifiWatcher();
        _wifiWatcher.AutoProxyChanged += (newStatus) => {
            UpdateTray(newStatus);
            _trayIcon.ShowBalloonTip(3000, "ProxySwitcher", $"プロキシを{(newStatus ? "ON" : "OFF")}に自動切替しました。", ToolTipIcon.Info);
        };
        _wifiWatcher.CheckWifiAndApplyProxy();

        Application.Run();
    }

    private static void UpdateTray(bool enabled)
    {
        _trayIcon!.Icon = CreateStatusIcon(enabled);
        if (_trayIcon.ContextMenuStrip?.Items["ToggleItem"] is ToolStripMenuItem item)
        {
            item.Text = enabled ? "プロキシをOFFにする" : "プロキシをONにする";
        }
    }

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

    private static ContextMenuStrip CreateContextMenu()
    {
        ContextMenuStrip menu = new ContextMenuStrip();
        
        ToolStripMenuItem toggleItem = new ToolStripMenuItem(ProxyManager.IsProxyEnabled() ? "プロキシをOFFにする" : "プロキシをONにする");
        toggleItem.Click += (s, e) => ToggleProxy();
        toggleItem.Name = "ToggleItem";

        ToolStripMenuItem settingsItem = new ToolStripMenuItem("設定");
        settingsItem.Click += (s, e) => {
            using (var form = new SettingsForm())
            {
                form.ShowDialog();
            }
        };

        ToolStripMenuItem exitItem = new ToolStripMenuItem("終了");
        exitItem.Click += (s, e) => {
            _trayIcon!.Visible = false;
            _hotKeyHandler?.Dispose();
            Application.Exit();
        };

        menu.Items.Add(toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(settingsItem);
        menu.Items.Add(exitItem);

        return menu;
    }

    private static void ToggleProxy()
    {
        bool newStatus = !ProxyManager.IsProxyEnabled();
        _config = AppConfig.Load(); // 最新の設定を読み込む
        ProxyManager.SetProxy(newStatus, _config.ProxyServer);
        
        UpdateTray(newStatus);

        _trayIcon!.ShowBalloonTip(2000, "ProxySwitcher", $"プロキシを{(newStatus ? "ON" : "OFF")}にしました。", ToolTipIcon.Info);
    }
}