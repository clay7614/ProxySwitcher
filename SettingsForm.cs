using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySwitcher;

public class SettingsForm : Form
{
    private TextBox _serverTextBox;
    private CheckBox _autostartCheckBox;
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
        this.Size = new Size(400, 250);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        Label label = new Label() { Text = "プロキシサーバー (host:port):", Left = 20, Top = 20, Width = 200 };
        _serverTextBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = _config.ProxyServer };

        _autostartCheckBox = new CheckBox() { Text = "Windows起動時に自動実行する", Left = 20, Top = 85, Width = 300, Checked = AutoStartManager.IsAutoStartEnabled() };

        _saveButton = new Button() { Text = "保存", Left = 200, Top = 150, Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button() { Text = "キャンセル", Left = 290, Top = 150, Width = 80 };
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(label);
        this.Controls.Add(_serverTextBox);
        this.Controls.Add(_autostartCheckBox);
        this.Controls.Add(_saveButton);
        this.Controls.Add(_cancelButton);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _config.ProxyServer = _serverTextBox.Text.Trim();
        _config.Save();
        
        AutoStartManager.SetAutoStart(_autostartCheckBox.Checked);

        MessageBox.Show("設定を保存しました。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}
