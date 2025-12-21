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
        NetworkChange.NetworkAddressChanged += (s, e) => {
            // 接続完了まで少し待機して判定を行う（タイミングの余裕を持たせる）
            Task.Delay(3000).ContinueWith(_ => CheckWifiAndApplyProxy());
        };
    }

    public void CheckWifiAndApplyProxy()
    {
        try
        {
            var config = AppConfig.Load();
            if (!config.WifiAutomationEnabled || config.TargetSSIDs.Count == 0)
                return;

            string currentSSID = GetCurrentSSID();
            bool shouldBeEnabled = (!string.IsNullOrEmpty(currentSSID) && 
                                    config.TargetSSIDs.Any(ssid => ssid.Equals(currentSSID, StringComparison.OrdinalIgnoreCase)));
            bool currentStatus = ProxyManager.IsProxyEnabled();

            if (shouldBeEnabled != currentStatus)
            {
                ProxyManager.SetProxy(shouldBeEnabled, config.ProxyServer);
                AutoProxyChanged?.Invoke(shouldBeEnabled);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WiFi判定エラー: {ex.Message}");
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
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.GetEncoding(932) // 日本語環境（Shift-JIS）への対応検討
            };

            using Process? process = Process.Start(psi);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                // より柔軟な正規表現でSSIDを抽出
                var lines = output.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains(" SSID") && line.Contains(":"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length >= 2)
                        {
                            string ssid = parts[1].Trim();
                            if (!string.IsNullOrEmpty(ssid) && !line.Contains("BSSID"))
                            {
                                return ssid;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // netsh実行エラー時など
        }
        return string.Empty;
    }
}
