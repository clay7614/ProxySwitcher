using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySwitcher;

internal static class Program
{
    private static NotifyIcon? _trayIcon;
    private static HotKeyHandler? _hotKeyHandler;
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

        Application.Run();
    }

    private static Icon CreateStatusIcon(bool enabled)
    {
        // 簡易的なアイコン生成（緑または赤の円）
        Bitmap bitmap = new Bitmap(64, 64);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            Brush brush = enabled ? Brushes.LimeGreen : Brushes.Red;
            g.FillEllipse(brush, 8, 8, 48, 48);
            using (Pen pen = new Pen(enabled ? Color.DarkGreen : Color.DarkRed, 4))
            {
                g.DrawEllipse(pen, 8, 8, 48, 48);
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
        
        // アイコンとメニューの更新
        _trayIcon!.Icon = CreateStatusIcon(newStatus);
        
        if (_trayIcon.ContextMenuStrip?.Items["ToggleItem"] is ToolStripMenuItem item)
        {
            item.Text = newStatus ? "プロキシをOFFにする" : "プロキシをONにする";
        }

        _trayIcon.ShowBalloonTip(2000, "ProxySwitcher", $"プロキシを{(newStatus ? "ON" : "OFF")}にしました。", ToolTipIcon.Info);
    }
}