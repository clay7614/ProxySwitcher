using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using ProxySwitcher.Managers;
using ProxySwitcher.Models;
using ProxySwitcher.Utilities;

namespace ProxySwitcher.UI;

public class SettingsForm : Form
{
    private CheckBox _useSystemProxyCheckBox = null!;
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
        
        // 設定の反映
        _useSystemProxyCheckBox.Checked = _config.UseSystemProxy;
        _serverTextBox.Enabled = !_config.UseSystemProxy;

        // 初回起動時（設定が空）の場合
        if (_config.IsNewInstance)
        {
            _useSystemProxyCheckBox.Checked = true;
            UpdateProxyServerFromSystem();
        }
    }

    private void InitializeComponent()
    {
        this.Font = UIConstants.DefaultFont;
        this.Text = "設定";
        this.ClientSize = new Size(450, 540);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        try {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        } catch { /* Fallback or ignore */ }
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        const int margin = 20;
        const int buttonWidth = 95;
        const int buttonHeight = 35;
        const int buttonGap = 10;
        int contentWidth = this.ClientSize.Width - (margin * 2);
        int rightButtonLeft = this.ClientSize.Width - margin - buttonWidth;
        int secondaryButtonLeft = rightButtonLeft - buttonGap - buttonWidth;
        int footerButtonTop = this.ClientSize.Height - margin - buttonHeight;

        _useSystemProxyCheckBox = new CheckBox() { Text = "Windowsの現在の設定を使用", Left = margin, Top = 20, Width = contentWidth, Height = 25 };
        _useSystemProxyCheckBox.CheckedChanged += (s, e) => {
            _serverTextBox.Enabled = !_useSystemProxyCheckBox.Checked;
            if (_useSystemProxyCheckBox.Checked) UpdateProxyServerFromSystem();
        };

        Label label = new Label() { Text = "プロキシサーバー (host:port):", Left = margin, Top = 55, Width = contentWidth, Height = 25 };
        _serverTextBox = new TextBox() { Left = margin, Top = 85, Width = contentWidth, Text = _config.ProxyServer };

        Label ssidLabel = new Label() { Text = "対象のWiFi(チェックしたWiFiでプロキシを自動切替):", Left = margin, Top = 125, Width = contentWidth, Height = 25 };
        _ssidListBox = new CheckedListBox() { Left = margin, Top = 155, Width = contentWidth - buttonWidth - buttonGap, Height = 140 };
        
        // 保存されているSSIDを追加してチェックを入れる
        foreach (var ssid in _config.TargetSSIDs)
        {
            _ssidListBox.Items.Add(ssid, true);
        }

        _scanButton = new Button() { Text = "スキャン", Left = rightButtonLeft, Top = 155, Width = buttonWidth, Height = buttonHeight };
        _scanButton.Click += ScanButton_Click;

        Label manualLabel = new Label() { Text = "手動追加:", Left = margin, Top = 320, Width = 100, Height = 25 };
        _manualSsidTextBox = new TextBox() { Left = 125, Top = 317, Width = contentWidth - 105 - buttonGap - buttonWidth };
        _addButton = new Button() { Text = "追加", Left = rightButtonLeft, Top = 315, Width = buttonWidth, Height = buttonHeight };
        _addButton.Click += AddButton_Click;

        _wifiAutoCheckBox = new CheckBox() { Text = "チェックしたWiFiへ接続したら、\r\nプロキシを自動で切替", Left = margin, Top = 360, Width = contentWidth, Height = 44, Checked = _config.WifiAutomationEnabled };

        _autostartCheckBox = new CheckBox() { Text = "Windows起動時に自動実行", Left = margin, Top = 410, Width = contentWidth, Height = 30, Checked = AutoStartManager.IsAutoStartEnabled() };

        _saveButton = new Button() { Text = "保存", Left = secondaryButtonLeft, Top = footerButtonTop, Width = buttonWidth, Height = buttonHeight };
        _saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button() { Text = "キャンセル", Left = rightButtonLeft, Top = footerButtonTop, Width = buttonWidth, Height = buttonHeight };
        _cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(_useSystemProxyCheckBox);
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

    private void UpdateProxyServerFromSystem()
    {
        string? currentProxy = ProxyManager.GetCurrentProxyServer();
        if (!string.IsNullOrEmpty(currentProxy))
        {
            _serverTextBox.Text = currentProxy;
        }
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
                MessageBox.Show("SSIDが見つかりませんでした。インターフェースが有効か確認してください。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        _config.UseSystemProxy = _useSystemProxyCheckBox.Checked;
        
        _config.TargetSSIDs.Clear();
        foreach (string item in _ssidListBox.CheckedItems)
        {
            _config.TargetSSIDs.Add(item);
        }
        _config.WifiAutomationEnabled = _wifiAutoCheckBox.Checked;
        _config.Save();
        
        AutoStartManager.SetAutoStart(_autostartCheckBox.Checked);

        MessageBox.Show("設定を保存しました。現在のネットワーク接続状況を確認し、自動適用を開始します。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}
