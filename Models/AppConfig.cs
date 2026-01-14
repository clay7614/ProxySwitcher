using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization; // Requires System.Web.Extensions reference

namespace ProxySwitcher.Models;

public class AppConfig
{
    public string ProxyServer { get; set; } = "proxy.maizuru-ct.ac.jp:8080";
    public List<string> TargetSSIDs { get; set; } = new List<string> { "MCSTUDENT" };
    public bool WifiAutomationEnabled { get; set; } = true;

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProxySwitcher",
        "config.json"
    );

    public void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            var serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(this);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch { }
        return new AppConfig();
    }
}
