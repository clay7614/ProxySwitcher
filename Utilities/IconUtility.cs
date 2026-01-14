using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProxySwitcher.Utilities;

public static class IconUtility
{
    public static Icon CreateStatusIcon(bool enabled)
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // リソース名は「プロジェクト名.フォルダ名.ファイル名」の形式
            string resourceName = enabled 
                ? "ProxySwitcher.Utilities.icon_on.ico" 
                : "ProxySwitcher.Utilities.icon_off.ico";

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }
        }
        catch { /* ロード失敗時はデフォルトアイコンへ */ }

        // リソースがない場合はアプリケーション自体のアイコンを返す
        return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
    }
}
