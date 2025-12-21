using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ProxySwitcher;

public class WifiScanner
{
    public List<string> GetAvailableSSIDs()
    {
        var ssids = new List<string>();
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "wlan show networks")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.GetEncoding(932)
            };

            using Process? process = Process.Start(psi);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                // SSID : [NAME] の形式を抽出
                var lines = output.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("SSID") && line.Contains(":"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length >= 2)
                        {
                            string ssid = parts[1].Trim();
                            if (!string.IsNullOrEmpty(ssid) && !ssids.Contains(ssid))
                            {
                                ssids.Add(ssid);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // エラー時は空リストを返す
        }
        return ssids;
    }
}
