using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySwitcher;

public class SettingsForm : Form
{
    private TextBox _serverTextBox;
    private TextBox _ssidTextBox;
    private CheckBox _autostartCheckBox;
    private CheckBox _wifiAutoCheckBox;
    private Button _saveButton;
    private Button _cancelButton;
    private AppConfig _config;

    public SettingsForm()
    {
        _config = AppConfig.Load();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ProxySwitcher 設定";
        this.Size = new Size(400, 350);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        Label label = new Label() { Text = "プロキシサーバー (host:port):", Left = 20, Top = 20, Width = 200 };
        _serverTextBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = _config.ProxyServer };

        Label ssidLabel = new Label() { Text = "対象のWiFi SSID (自動切替用):", Left = 20, Top = 85, Width = 250 };
        _ssidTextBox = new TextBox() { Left = 20, Top = 110, Width = 340, Text = _config.TargetSSID };

        _wifiAutoCheckBox = new CheckBox() { Text = "このWiFi接続時にプロキシを自動ONにする", Left = 20, Top = 145, Width = 350, Checked = _config.WifiAutomationEnabled };

        _autostartCheckBox = new CheckBox() { Text = "Windows起動時に自動実行する", Left = 20, Top = 185, Width = 300, Checked = AutoStartManager.IsAutoStartEnabled() };

        _saveButton = new Button() { Text = "保存", Left = 200, Top = 250, Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button() { Text = "キャンセル", Left = 290, Top = 250, Width = 80 };
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(label);
        this.Controls.Add(_serverTextBox);
        this.Controls.Add(ssidLabel);
        this.Controls.Add(_ssidTextBox);
        this.Controls.Add(_wifiAutoCheckBox);
        this.Controls.Add(_autostartCheckBox);
        this.Controls.Add(_saveButton);
        this.Controls.Add(_cancelButton);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _config.ProxyServer = _serverTextBox.Text.Trim();
        _config.TargetSSID = _ssidTextBox.Text.Trim();
        _config.WifiAutomationEnabled = _wifiAutoCheckBox.Checked;
        _config.Save();
        
        AutoStartManager.SetAutoStart(_autostartCheckBox.Checked);

        MessageBox.Show("設定を保存しました。可能であれば、現在のネットワーク接続状況を確認し、自動適用を開始します。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}
