using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace ProxySwitcher;

public class WifiWatcher
{
    public event Action<bool>? AutoProxyChanged;

    public WifiWatcher()
    {
        NetworkChange.NetworkAddressChanged += (s, e) => CheckWifiAndApplyProxy();
    }

    public void CheckWifiAndApplyProxy()
    {
        var config = AppConfig.Load();
        if (!config.WifiAutomationEnabled || string.IsNullOrEmpty(config.TargetSSID))
            return;

        string currentSSID = GetCurrentSSID();
        bool shouldBeEnabled = (currentSSID == config.TargetSSID);
        bool currentStatus = ProxyManager.IsProxyEnabled();

        if (shouldBeEnabled != currentStatus)
        {
            ProxyManager.SetProxy(shouldBeEnabled, config.ProxyServer);
            AutoProxyChanged?.Invoke(shouldBeEnabled);
        }
    }

    private string GetCurrentSSID()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "wlan show interfaces")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using Process? process = Process.Start(psi);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                Match match = Regex.Match(output, @"^\s+SSID\s+:\s+(.+)$", RegexOptions.Multiline);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
        }
        catch
        {
            // エラー時は空文字を返す
        }
        return string.Empty;
    }
}
