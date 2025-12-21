using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProxySwitcher;

public class HotKeyHandler : NativeWindow, IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int VK_P = 0x50;
    private const int HOTKEY_ID = 9000;

    public event Action? HotKeyPressed;

    public HotKeyHandler()
    {
        this.CreateHandle(new CreateParams());
        bool success = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_P);
        if (!success)
        {
            // ホットキー登録失敗時の処理（必要に応じて）
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            HotKeyPressed?.Invoke();
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        UnregisterHotKey(this.Handle, HOTKEY_ID);
        this.DestroyHandle();
    }
}
