using System;
using System.Windows.Forms;

namespace ProxySwitcher.Core;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 構造をリファクタリング: Contextに主要ロジックを移譲
        var context = new ProxySwitcherContext();
        
        Application.Run(context);
    }
}