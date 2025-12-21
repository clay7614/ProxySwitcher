using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySwitcher;

public class SettingsForm : Form
{
    private TextBox _serverTextBox = null!;
    private CheckedListBox _ssidListBox = null!;
    private CheckBox _autostartCheckBox = null!;
    private CheckBox _wifiAutoCheckBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private Button _scanButton = null!;
    private AppConfig _config;

    public SettingsForm()
    {
        _config = AppConfig.Load();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ProxySwitcher 設定";
        this.Size = new Size(400, 450); // 高さを 350 -> 450 に拡大
        this.Icon = Program.CreateStatusIcon(ProxyManager.IsProxyEnabled()); // アイコン設定
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        Label label = new Label() { Text = "プロキシサーバー (host:port):", Left = 20, Top = 20, Width = 200 };
        _serverTextBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = _config.ProxyServer };

        Label ssidLabel = new Label() { Text = "対象のWiFi (チェックしたWiFiでプロキシON):", Left = 20, Top = 85, Width = 300 };
        _ssidListBox = new CheckedListBox() { Left = 20, Top = 110, Width = 260, Height = 150 }; // 高さ調整
        
        // 保存されているSSIDを追加してチェックを入れる
        foreach (var ssid in _config.TargetSSIDs)
        {
            _ssidListBox.Items.Add(ssid, true);
        }

        _scanButton = new Button() { Text = "スキャン", Left = 290, Top = 110, Width = 80 };
        _scanButton.Click += ScanButton_Click;

        _wifiAutoCheckBox = new CheckBox() { Text = "指定WiFi接続時に自動でON/OFFを切り替える", Left = 20, Top = 280, Width = 350, Checked = _config.WifiAutomationEnabled };

        _autostartCheckBox = new CheckBox() { Text = "Windows起動時に自動実行する", Left = 20, Top = 310, Width = 300, Checked = AutoStartManager.IsAutoStartEnabled() };

        _saveButton = new Button() { Text = "保存", Left = 200, Top = 360, Width = 80, Height = 32 };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button() { Text = "キャンセル", Left = 290, Top = 360, Width = 80, Height = 32 };
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(label);
        this.Controls.Add(_serverTextBox);
        this.Controls.Add(ssidLabel);
        this.Controls.Add(_ssidListBox);
        this.Controls.Add(_scanButton);
        this.Controls.Add(_wifiAutoCheckBox);
        this.Controls.Add(_autostartCheckBox);
        this.Controls.Add(_saveButton);
        this.Controls.Add(_cancelButton);
    }

    private async void ScanButton_Click(object? sender, EventArgs e)
    {
        _scanButton.Enabled = false;
        _scanButton.Text = "待機中...";
        
        try
        {
            // 周囲のWiFiをスキャン (非同期で実行)
            var ssids = await Task.Run(() => new WifiScanner().GetAvailableSSIDs());

            foreach (var ssid in ssids)
            {
                if (!_ssidListBox.Items.Contains(ssid))
                {
                    _ssidListBox.Items.Add(ssid, false);
                }
            }
            
            if (ssids.Count == 0)
            {
                MessageBox.Show("WiFi SSIDが見つかりませんでした。インターフェースが有効か確認してください。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"スキャン中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _scanButton.Enabled = true;
            _scanButton.Text = "スキャン";
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _config.ProxyServer = _serverTextBox.Text.Trim();
        
        _config.TargetSSIDs.Clear();
        foreach (string item in _ssidListBox.CheckedItems)
        {
            _config.TargetSSIDs.Add(item);
        }
        _config.WifiAutomationEnabled = _wifiAutoCheckBox.Checked;
        _config.Save();
        
        AutoStartManager.SetAutoStart(_autostartCheckBox.Checked);

        MessageBox.Show("設定を保存しました。可能であれば、現在のネットワーク接続状況を確認し、自動適用を開始します。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}
