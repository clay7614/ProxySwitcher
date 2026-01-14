using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ProxySwitcher.Models;

using System.Net.NetworkInformation;

using System.Runtime.InteropServices;
using ProxySwitcher.Infrastructure;

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
            System.Diagnostics.Debug.WriteLine($"Wifi Check Error: {ex.Message}");
        }
    }

    private string GetCurrentSsid()
    {
        uint negotiatedVersion;
        IntPtr clientHandle = IntPtr.Zero;
        IntPtr pInterfaceList = IntPtr.Zero;
        string ssid = "";

        try
        {
            if (NativeMethods.WlanOpenHandle(2, IntPtr.Zero, out negotiatedVersion, out clientHandle) != 0)
                return "";

            if (NativeMethods.WlanEnumInterfaces(clientHandle, IntPtr.Zero, out pInterfaceList) != 0)
                return "";

            var list = (NativeMethods.WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(pInterfaceList, typeof(NativeMethods.WLAN_INTERFACE_INFO_LIST));
            
            IntPtr currentPtr = new IntPtr(pInterfaceList.ToInt64() + 8); // Skip dwNumberOfItems (4) + dwIndex (4)

            for (int i = 0; i < list.dwNumberOfItems; i++)
            {
                var info = (NativeMethods.WLAN_INTERFACE_INFO)Marshal.PtrToStructure(currentPtr, typeof(NativeMethods.WLAN_INTERFACE_INFO));
                currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(NativeMethods.WLAN_INTERFACE_INFO)));

                if (info.isState == NativeMethods.WLAN_INTERFACE_STATE_CONNECTED)
                {
                    IntPtr pData = IntPtr.Zero;
                    uint dataSize;
                    int opcodeValueType;
                    
                    if (NativeMethods.WlanQueryInterface(clientHandle, ref info.InterfaceGuid, NativeMethods.WLAN_INTF_OPCODE_CURRENT_CONNECTION, IntPtr.Zero, out dataSize, out pData, out opcodeValueType) == 0)
                    {
                         try 
                         {
                             var connection = (NativeMethods.WLAN_CONNECTION_ATTRIBUTES)Marshal.PtrToStructure(pData, typeof(NativeMethods.WLAN_CONNECTION_ATTRIBUTES));
                             if (connection.wlanAssociationAttributes.dot11Ssid.uSSIDLength > 0)
                             {
                                 // SSID解読ロジック: UTF-8 -> Shift-JIS (CP932) の順で試行
                                 ssid = DecodeSsid(connection.wlanAssociationAttributes.dot11Ssid.ucSSID, (int)connection.wlanAssociationAttributes.dot11Ssid.uSSIDLength);
                             }
                         }
                         finally 
                         {
                             NativeMethods.WlanFreeMemory(pData);
                         }
                         
                         if (!string.IsNullOrEmpty(ssid)) break;
                    }
                }
            }
        }
        catch { /* 無視 */ }
        finally
        {
            if (pInterfaceList != IntPtr.Zero)
                NativeMethods.WlanFreeMemory(pInterfaceList);
            if (clientHandle != IntPtr.Zero)
                NativeMethods.WlanCloseHandle(clientHandle, IntPtr.Zero);
        }

        return ssid;
    }

    /// <summary>
    /// SSID バイト列を文字列にデコードする。
    /// IEEE 802.11 規格では SSID は UTF-8 でエンコードされる。
    /// </summary>
    private string DecodeSsid(byte[] rawBytes, int length)
    {
        if (length <= 0) return "";
        
        // 必要な長さ分だけコピー
        var usefulBytes = new byte[length];
        Array.Copy(rawBytes, usefulBytes, length);

        // UTF-8 でデコード (IEEE 802.11 標準)
        return Encoding.UTF8.GetString(usefulBytes);
    }
}
