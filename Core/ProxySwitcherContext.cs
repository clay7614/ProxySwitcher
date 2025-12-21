using System;
using System.Drawing;
using System.Windows.Forms;
using ProxySwitcher.Managers;
using ProxySwitcher.Models;
using ProxySwitcher.UI;
using ProxySwitcher.Utilities;
using ProxySwitcher.Infrastructure;

namespace ProxySwitcher.Core;

public class ProxySwitcherContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly HotKeyHandler _hotKeyHandler;
    private readonly WifiWatcher _wifiWatcher;
    private AppConfig _config;

    public ProxySwitcherContext()
    {
        _config = AppConfig.Load();

        // トレイアイコンの作成
        _trayIcon = new NotifyIcon()
        {
            Icon = IconUtility.CreateStatusIcon(ProxyManager.IsProxyEnabled()),
            ContextMenuStrip = CreateContextMenu(),
            Visible = true,
            Text = UIConstants.AppName
        };

        // ホットキーの登録
        _hotKeyHandler = new HotKeyHandler();
        _hotKeyHandler.HotKeyPressed += ToggleProxy;

        // WiFi監視の開始
        _wifiWatcher = new WifiWatcher();
        _wifiWatcher.AutoProxyChanged += (newStatus) => {
            UpdateTray(newStatus);
            _trayIcon.ShowBalloonTip(3000, UIConstants.AppName, $"プロキシを{(newStatus ? "ON" : "OFF")}に自動切替しました。", ToolTipIcon.Info);
        };
        _wifiWatcher.CheckWifiAndApplyProxy();
    }

    private void UpdateTray(bool enabled)
    {
        _trayIcon.Icon = IconUtility.CreateStatusIcon(enabled);
        if (_trayIcon.ContextMenuStrip?.Items["ToggleItem"] is ToolStripMenuItem item)
        {
            item.Text = enabled ? "プロキシをOFFにする" : "プロキシをONにする";
        }
    }

    private ContextMenuStrip CreateContextMenu()
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
            ExitThread();
        };

        menu.Items.Add(toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(settingsItem);
        menu.Items.Add(exitItem);

        return menu;
    }

    private void ToggleProxy()
    {
        bool newStatus = !ProxyManager.IsProxyEnabled();
        _config = AppConfig.Load(); // 最新の設定を読み込む
        ProxyManager.SetProxy(newStatus, _config.ProxyServer);
        
        UpdateTray(newStatus);

        _trayIcon.ShowBalloonTip(2000, UIConstants.AppName, $"プロキシを{(newStatus ? "ON" : "OFF")}にしました。", ToolTipIcon.Info);
    }

    protected override void ExitThreadCore()
    {
        _trayIcon.Visible = false;
        _hotKeyHandler?.Dispose();
        base.ExitThreadCore();
    }
}
