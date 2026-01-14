using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ProxySwitcher.Infrastructure;

namespace ProxySwitcher.Managers;

/// <summary>
/// Native Wifi API を使用して周辺の WiFi SSID をスキャンするクラス。
/// netsh コマンドの文字化け問題を根本的に解決。
/// </summary>
public class WifiScanner
{
    public List<string> GetAvailableSSIDs()
    {
        List<string> ssids = new List<string>();
        IntPtr clientHandle = IntPtr.Zero;
        IntPtr pInterfaceList = IntPtr.Zero;

        try
        {
            uint negotiatedVersion;
            if (NativeMethods.WlanOpenHandle(2, IntPtr.Zero, out negotiatedVersion, out clientHandle) != 0)
                return ssids;

            if (NativeMethods.WlanEnumInterfaces(clientHandle, IntPtr.Zero, out pInterfaceList) != 0)
                return ssids;

            var list = (NativeMethods.WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(pInterfaceList, typeof(NativeMethods.WLAN_INTERFACE_INFO_LIST));
            IntPtr currentPtr = new IntPtr(pInterfaceList.ToInt64() + 8);

            for (int i = 0; i < list.dwNumberOfItems; i++)
            {
                var info = (NativeMethods.WLAN_INTERFACE_INFO)Marshal.PtrToStructure(currentPtr, typeof(NativeMethods.WLAN_INTERFACE_INFO));
                currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(NativeMethods.WLAN_INTERFACE_INFO)));

                // このインターフェースで見えるネットワークを取得
                IntPtr pNetworkList = IntPtr.Zero;
                if (NativeMethods.WlanGetAvailableNetworkList(clientHandle, ref info.InterfaceGuid, 0, IntPtr.Zero, out pNetworkList) == 0)
                {
                    try
                    {
                        var networkList = (NativeMethods.WLAN_AVAILABLE_NETWORK_LIST)Marshal.PtrToStructure(pNetworkList, typeof(NativeMethods.WLAN_AVAILABLE_NETWORK_LIST));
                        IntPtr networkPtr = new IntPtr(pNetworkList.ToInt64() + 8);

                        for (int j = 0; j < networkList.dwNumberOfItems; j++)
                        {
                            var network = (NativeMethods.WLAN_AVAILABLE_NETWORK)Marshal.PtrToStructure(networkPtr, typeof(NativeMethods.WLAN_AVAILABLE_NETWORK));
                            networkPtr = new IntPtr(networkPtr.ToInt64() + Marshal.SizeOf(typeof(NativeMethods.WLAN_AVAILABLE_NETWORK)));

                            if (network.dot11Ssid.uSSIDLength > 0)
                            {
                                string ssid = DecodeSsid(network.dot11Ssid.ucSSID, (int)network.dot11Ssid.uSSIDLength);
                                if (!string.IsNullOrEmpty(ssid) && !ssids.Contains(ssid))
                                {
                                    ssids.Add(ssid);
                                }
                            }
                        }
                    }
                    finally
                    {
                        NativeMethods.WlanFreeMemory(pNetworkList);
                    }
                }
            }
        }
        catch { }
        finally
        {
            if (pInterfaceList != IntPtr.Zero)
                NativeMethods.WlanFreeMemory(pInterfaceList);
            if (clientHandle != IntPtr.Zero)
                NativeMethods.WlanCloseHandle(clientHandle, IntPtr.Zero);
        }

        return ssids;
    }

    private string DecodeSsid(byte[] rawBytes, int length)
    {
        if (length <= 0) return "";
        var usefulBytes = new byte[length];
        Array.Copy(rawBytes, usefulBytes, length);
        return Encoding.UTF8.GetString(usefulBytes);
    }
}

