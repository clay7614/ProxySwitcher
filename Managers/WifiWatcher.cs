using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ProxySwitcher.Models;

namespace ProxySwitcher.Managers;

public class WifiWatcher
{
    private string _lastSsid = "";
    private bool _isAutomationRunning = false;
    public event Action<bool>? AutoProxyChanged;

    public WifiWatcher()
    {
        // ネットワーク変更を監視するスレッドを開始
        Task.Run(MonitorWifi);
    }

    public void CheckWifiAndApplyProxy()
    {
        var config = AppConfig.Load();
        if (!config.WifiAutomationEnabled) return;

        string currentSsid = GetCurrentSsid();
        bool isTarget = config.TargetSSIDs.Contains(currentSsid);

        if (ProxyManager.IsProxyEnabled() != isTarget)
        {
            ProxyManager.SetProxy(isTarget, config.ProxyServer);
            AutoProxyChanged?.Invoke(isTarget);
        }
    }

    private async Task MonitorWifi()
    {
        while (true)
        {
            try
            {
                var config = AppConfig.Load();
                if (config.WifiAutomationEnabled)
                {
                    string currentSsid = GetCurrentSsid();
                    if (currentSsid != _lastSsid)
                    {
                        _lastSsid = currentSsid;
                        CheckWifiAndApplyProxy();
                    }
                }
            }
            catch { /* Ignore */ }
            await Task.Delay(5000);
        }
    }

    private string GetCurrentSsid()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "wlan show interfaces")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.GetEncoding(932) // 日本語環境の文字化け対策
            };

            using Process? process = Process.Start(psi);
            if (process == null) return "";

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (string line in output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmedLine = line.Trim();
                // BSSID ではなく SSID を確実に取得するため、行の開始を確認
                if (trimmedLine.StartsWith("SSID") && trimmedLine.Contains(":"))
                {
                    return trimmedLine.Split(':')[1].Trim();
                }
            }
        }
        catch { }
        return "";
    }
}
