using System;
using System.IO;
using System.Text.Json;

namespace ProxySwitcher;

public class AppConfig
{
    public string ProxyServer { get; set; } = "proxy2.maizuru-ct.ac.jp:8090";
    public bool UseProxyForLocal { get; set; } = false;
    public string TargetSSID { get; set; } = "";
    public bool WifiAutomationEnabled { get; set; } = false;

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProxySwitcher",
        "config.json"
    );

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch
        {
            // 読み込み失敗時はデフォルトを返す
        }
        return new AppConfig();
    }

    public void Save()
    {
        try
        {
            string? directory = Path.GetDirectoryName(ConfigPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.ReadAllText(ConfigPath); // ダミー
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            // 保存失敗時の処理 (必要に応じてログ出力等)
        }
    }
}
