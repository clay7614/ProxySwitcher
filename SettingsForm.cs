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
    private TextBox _manualSsidTextBox = null!;
    private Button _addButton = null!;
    private AppConfig _config;

    public SettingsForm()
    {
        _config = AppConfig.Load();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ProxySwitcher 設定";
        this.Size = new Size(420, 500); // サイズに余裕を持たせる
        this.Icon = Program.CreateStatusIcon(ProxyManager.IsProxyEnabled()); // アイコン設定
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        Label label = new Label() { Text = "プロキシサーバー (host:port):", Left = 20, Top = 20, Width = 300, Height = 25 };
        _serverTextBox = new TextBox() { Left = 20, Top = 50, Width = 360, Text = _config.ProxyServer };

        Label ssidLabel = new Label() { Text = "対象のWiFi (チェックしたWiFiでプロキシON):", Left = 20, Top = 90, Width = 350, Height = 25 };
        _ssidListBox = new CheckedListBox() { Left = 20, Top = 120, Width = 260, Height = 140 };
        
        // 保存されているSSIDを追加してチェックを入れる
        foreach (var ssid in _config.TargetSSIDs)
        {
            _ssidListBox.Items.Add(ssid, true);
        }

        _scanButton = new Button() { Text = "スキャン", Left = 290, Top = 120, Width = 90, Height = 35 };
        _scanButton.Click += ScanButton_Click;

        Label manualLabel = new Label() { Text = "手動追加:", Left = 20, Top = 285, Width = 100, Height = 25 };
        _manualSsidTextBox = new TextBox() { Left = 125, Top = 282, Width = 155 };
        _addButton = new Button() { Text = "追加", Left = 290, Top = 280, Width = 90, Height = 35 };
        _addButton.Click += AddButton_Click;

        _wifiAutoCheckBox = new CheckBox() { Text = "指定WiFi接続時に自動でON/OFFを切り替える", Left = 20, Top = 325, Width = 380, Height = 30, Checked = _config.WifiAutomationEnabled };

        _autostartCheckBox = new CheckBox() { Text = "Windows起動時に自動実行する", Left = 20, Top = 360, Width = 380, Height = 30, Checked = AutoStartManager.IsAutoStartEnabled() };

        _saveButton = new Button() { Text = "保存", Left = 210, Top = 410, Width = 90, Height = 35 };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button() { Text = "キャンセル", Left = 310, Top = 410, Width = 90, Height = 35 };
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(label);
        this.Controls.Add(_serverTextBox);
        this.Controls.Add(ssidLabel);
        this.Controls.Add(_ssidListBox);
        this.Controls.Add(_scanButton);
        this.Controls.Add(manualLabel);
        this.Controls.Add(_manualSsidTextBox);
        this.Controls.Add(_addButton);
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

    private void AddButton_Click(object? sender, EventArgs e)
    {
        string ssid = _manualSsidTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(ssid))
        {
            if (!_ssidListBox.Items.Contains(ssid))
            {
                _ssidListBox.Items.Add(ssid, true);
                _manualSsidTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("このSSIDは既に追加されています。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
