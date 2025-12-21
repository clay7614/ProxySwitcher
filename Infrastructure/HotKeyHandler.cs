using System;
using System.Windows.Forms;
using ProxySwitcher.Infrastructure;

namespace ProxySwitcher.Infrastructure;

public class HotKeyHandler : NativeWindow, IDisposable
{
    private const int HOTKEY_ID = 9000;

    public event Action? HotKeyPressed;

    public HotKeyHandler()
    {
        this.CreateHandle(new CreateParams());
        bool success = NativeMethods.RegisterHotKey(this.Handle, HOTKEY_ID, NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT, NativeMethods.VK_P);
        if (!success)
        {
            // ホットキー登録失敗時の処理（必要に応じて）
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            HotKeyPressed?.Invoke();
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        NativeMethods.UnregisterHotKey(this.Handle, HOTKEY_ID);
        this.DestroyHandle();
    }
}
