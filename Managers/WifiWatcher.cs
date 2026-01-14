using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ProxySwitcher.Models;

using System.Net.NetworkInformation;

namespace ProxySwitcher.Managers;

public class WifiWatcher
{
    private string _lastSsid = "";
    public event Action<bool>? AutoProxyChanged;

    public WifiWatcher()
    {
        // ネットワーク変更イベントを購読 (ポーリング廃止)
        NetworkChange.NetworkAddressChanged += OnNetworkChanged;
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        
        // 初回チェック
        CheckWifiAndApplyProxy();
    }

    private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
    {
        CheckWifiAndApplyProxy();
    }

    private void OnNetworkChanged(object sender, EventArgs e)
    {
        CheckWifiAndApplyProxy();
    }

    public void CheckWifiAndApplyProxy()
    {
        var config = AppConfig.Load();
        if (!config.WifiAutomationEnabled) return;

        // イベントは頻繁に発火するため、少し待機してからSSIDを取得しても良いが、
        // 現状はシンプルに実行する。必要に応じてデバウンスを検討。
        
        try
        {
            string currentSsid = GetCurrentSsid();
            
            // SSIDが変わっていない場合はスキップ (無駄なプロキシ設定を防ぐ)
            if (currentSsid == _lastSsid && !string.IsNullOrEmpty(currentSsid)) return;
            
            _lastSsid = currentSsid;
            bool isTarget = config.TargetSSIDs.Contains(currentSsid);

            if (ProxyManager.IsProxyEnabled() != isTarget)
            {
                ProxyManager.SetProxy(isTarget, config.ProxyServer);
                AutoProxyChanged?.Invoke(isTarget);
            }
        }
        catch (Exception ex)
        {
            // ログ出力など
            System.Diagnostics.Debug.WriteLine($"Wifi Check Error: {ex.Message}");
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
