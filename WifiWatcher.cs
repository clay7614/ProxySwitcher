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
                CreateNoWindow = true
                // エンコーディング指定を削除してOSデフォルトに任せる
            };

            using Process? process = Process.Start(psi);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                var lines = output.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    // "SSID" で始まり、かつ "BSSID" ではない行を探す
                    string trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("SSID") && !trimmedLine.StartsWith("BSSID") && trimmedLine.Contains(":"))
                    {
                        var parts = trimmedLine.Split(new[] { ':' }, 2); // 最初のコロンで分割
                        if (parts.Length >= 2)
                        {
                            return parts[1].Trim();
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
