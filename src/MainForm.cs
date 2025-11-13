
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;
    private AutomationManager _automationManager; // Added for automation tasks
    private Timer statusUpdateTimer;

    // --- UI Controls ---
    // ... (existing controls)
    private ComboBox chromePathComboBox, profileComboBox;
    private ListBox profilesListBox, urlsListBox;
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox, currentProfileTextBox, closingProfileTextBox;
    private Button saveProfileButton, addUrlButton, saveAndOpenUrlButton, openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton;
    private Button arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, loginGoogleListButton, openAllProfilesButton;
    private Button openChromeButton, loginGoogleButton, closeChromeButton;
    private CheckBox alwaysOnTopCheckBox;
    private Label profileCountLabel;

    // New Automation Controls
    private TextBox automationUrlTextBox;
    private Button startAutomationButton;

    public MainForm()
    {
        _profileManager = new Pro5ChromeManager();
        _urlManager = new UrlManager();
        _automationManager = new AutomationManager(); // Instantiate the manager
        InitializeComponent();
        this.Load += MainForm_Load;
    }
    
    // ... (SafeOpenFile method remains the same)
    private void SafeOpenFile(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                 if (fileName.EndsWith(".json")) {
                    File.WriteAllText(fileName, fileName.Contains("config") ? "{}" : "[]");
                 } else {
                    File.Create(fileName).Close(); 
                 }
            }
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        { 
            MessageBox.Show($"Không thể mở file '{fileName}'.\nLỗi: {ex.Message}", "Lỗi Mở File", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeComponent()
    {
        this.Text = "Pro5Chrome Manager by hieuck";
        this.Size = new Size(1150, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        Padding controlMargin = new Padding(5);

        // --- Top Bar & Path Bar ---
        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var pathFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var quickActionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        // ... (Top panel controls remain the same)
        var openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = controlMargin };
        openConfigButton.Click += (s, e) => SafeOpenFile("config.json");
        var openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = controlMargin };
        openProfilesJsonButton.Click += (s, e) => SafeOpenFile("profiles.json");
        var openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = controlMargin };
        openUrlJsonButton.Click += (s, e) => SafeOpenFile("URL.json");
        alwaysOnTopCheckBox = new CheckBox { Text = "Luôn trên cùng", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        alwaysOnTopCheckBox.CheckedChanged += AlwaysOnTopCheckBox_CheckedChanged;
        topFlowPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox });

        var pathLabel = new Label { Text = "Đường dẫn trình duyệt:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        chromePathComboBox = new ComboBox { Width = 450, DropDownStyle = ComboBoxStyle.DropDown, Margin = new Padding(5, 5, 0, 0) };
        var openUserDataButton = new Button { Text = "Mở User Data", AutoSize = true, Margin = controlMargin };
        var deletePathButton = new Button { Text = "Xóa", AutoSize = true, Margin = controlMargin };
        var discoverProfilesButton = new Button { Text = "Đọc Profiles", AutoSize = true, Margin = controlMargin };
        pathFlowPanel.Controls.AddRange(new Control[] { pathLabel, chromePathComboBox, openUserDataButton, deletePathButton, discoverProfilesButton });
        
        var profileLabel = new Label { Text = "Profile Nhanh:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        profileComboBox = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDown, Margin = new Padding(5, 5, 0, 0) };
        openChromeButton = new Button { Text = "Mở", AutoSize = true, Margin = controlMargin };
        loginGoogleButton = new Button { Text = "Đăng Nhập Google", AutoSize = true, Margin = controlMargin };
        closeChromeButton = new Button { Text = "Đóng Tất Cả", AutoSize = true, Margin = controlMargin };
        quickActionFlowPanel.Controls.AddRange(new Control[] { profileLabel, profileComboBox, openChromeButton, loginGoogleButton, closeChromeButton });

        // --- Main Content Panels ---
        var mainPanel = new Panel { Dock = DockStyle.Fill };
        var rightPanel = new Panel { Dock = DockStyle.Right, Width = 580, Padding = new Padding(5) };
        var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        mainPanel.Controls.AddRange(new Control[] { leftPanel, rightPanel });

        // --- LEFT COLUMN (Profile List & Details) ---
        var profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Thông tin Profile", Height = 150, Padding = new Padding(10) };
        var emailLabel = new Label { Text = "Email:", Location = new Point(15, 30), AutoSize = true };
        emailTextBox = new TextBox { Location = new Point(90, 27), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = leftPanel.ClientSize.Width - 110 };
        var passwordLabel = new Label { Text = "Password:", Location = new Point(15, 60), AutoSize = true };
        passwordTextBox = new TextBox { Location = new Point(90, 57), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = emailTextBox.Width, UseSystemPasswordChar = true };
        var otpLabel = new Label { Text = "OTP Secret:", Location = new Point(15, 90), AutoSize = true };
        otpTextBox = new TextBox { Location = new Point(90, 87), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = emailTextBox.Width };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", Location = new Point(88, 115), Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true, Enabled = false };
        profileDetailsGroupBox.Controls.AddRange(new Control[] { emailLabel, emailTextBox, passwordLabel, passwordTextBox, otpLabel, otpTextBox, saveProfileButton });
        
        var profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0,0,0,5) };
        profilesListBox = new ListBox { Dock = DockStyle.Fill };
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListBox, profileCountLabel });
        leftPanel.Controls.AddRange(new Control[] { profilesGroupBox, profileDetailsGroupBox });

        // --- RIGHT COLUMN ---
        var statusGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Trạng thái Cửa sổ", Height = 130, Padding = new Padding(10) };
        currentProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true, Margin = new Padding(0, 0, 0, 5), PlaceholderText = "Cửa sổ Active..." };
        closingProfileTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Các cửa sổ khác..." };
        statusGroupBox.Controls.AddRange(new Control[] { closingProfileTextBox, currentProfileTextBox });

        var windowActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Quản lý Cửa sổ Chung", Height = 130, Padding = new Padding(10) };
        // ... (window actions controls remain the same)

        // --- NEW: Automation GroupBox ---
        var automationGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Tự động hóa & Kịch bản", Height = 90, Padding = new Padding(10) };
        automationUrlTextBox = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Nhập URL để bắt đầu tự động hóa..." };
        startAutomationButton = new Button { Text = "Bắt đầu đến URL", Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
        automationGroupBox.Controls.AddRange(new Control[] { startAutomationButton, automationUrlTextBox });

        var urlActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Hành động với URL", Height = 120, Padding = new Padding(10) };
        // ... (url actions controls remain the same)

        var urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsGroupBox.Controls.Add(urlsListBox);
        rightPanel.Controls.AddRange(new Control[] { urlsGroupBox, urlActionsGroupBox, automationGroupBox, windowActionsGroupBox, statusGroupBox });

        // --- Add Panels to Form ---
        this.Controls.AddRange(new Control[] { mainPanel, quickActionFlowPanel, pathFlowPanel, topFlowPanel });

        // --- EVENT HANDLERS ---
        startAutomationButton.Click += StartAutomationButton_Click;
        // ... (all other event handlers remain the same)
    }

    // --- New Event Handler for Automation ---
    private async void StartAutomationButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn một profile từ danh sách bên trái.", "Chưa chọn Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(automationUrlTextBox.Text))
        {
            MessageBox.Show("Vui lòng nhập một URL vào ô tự động hóa.", "Thiếu URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string profileName = profilesListBox.SelectedItem.ToString();
        string url = automationUrlTextBox.Text;
        string userDataPath = _profileManager.GetEffectiveUserDataPath();
        string chromeDriverPath = Path.GetDirectoryName(Application.ExecutablePath);

        try
        {
            await _automationManager.NavigateToUrlAsync(profileName, userDataPath, chromeDriverPath, url);
            MessageBox.Show($"Đã bắt đầu tự động hóa cho profile '{profileName}' và điều hướng tới URL.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        { 
            MessageBox.Show($"Đã xảy ra lỗi trong quá trình tự động hóa:\n{ex.Message}", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ... (All other methods like RefreshProfileLists, MainForm_Load, etc., remain the same)

}
