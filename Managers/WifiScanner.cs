using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProxySwitcher.Managers;

public class WifiScanner
{
    public List<string> GetAvailableSSIDs()
    {
        List<string> ssids = new List<string>();
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "wlan show networks")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.GetEncoding(932)
            };

            using Process? process = Process.Start(psi);
            if (process == null) return ssids;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (string line in output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("SSID") && trimmedLine.Contains(":"))
                {
                    string ssid = trimmedLine.Split(':')[1].Trim();
                    if (!string.IsNullOrEmpty(ssid) && !ssids.Contains(ssid))
                    {
                        ssids.Add(ssid);
                    }
                }
            }
        }
        catch { }
        return ssids;
    }
}
