using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ProxySwitcher;

public static class ProxyManager
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    private const int INTERNET_OPTION_REFRESH = 37;

    public static bool IsProxyEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key != null)
            {
                object? value = key.GetValue("ProxyEnable");
                return value != null && (int)value == 1;
            }
        }
        catch
        {
            // エラー時は無効とみなす
        }
        return false;
    }

    public static void SetProxy(bool enable, string? server = null)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key != null)
            {
                key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
                if (enable && !string.IsNullOrEmpty(server))
                {
                    key.SetValue("ProxyServer", server, RegistryValueKind.String);
                }
                
                // 設定の即時反映
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"プロキシ設定の変更に失敗しました: {ex.Message}", "エラー", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
