
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
    private Timer statusUpdateTimer;

    // UI Controls
    private ComboBox chromePathComboBox, profileComboBox;
    private ListBox profilesListBox, urlsListBox;
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox, currentProfileTextBox, closingProfileTextBox;
    private Button saveProfileButton, addUrlButton, saveAndOpenUrlButton, openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton;
    private Button arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, loginGoogleListButton, openAllProfilesButton;
    private Button openChromeButton, loginGoogleButton, closeChromeButton;
    private CheckBox alwaysOnTopCheckBox;
    private Label profileCountLabel;

    public MainForm()
    {
        _profileManager = new Pro5ChromeManager();
        _urlManager = new UrlManager();
        InitializeComponent();
        this.Load += MainForm_Load;
    }

    private void InitializeComponent()
    {
        this.Text = "Profiles Google Chrome by hieuck";
        this.Size = new Size(1150, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        Padding controlMargin = new Padding(5);

        // --- Top Bar & Path Bar ---
        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var pathFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var quickActionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        var openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = controlMargin };
        openConfigButton.Click += (s, e) => { try { Process.Start("config.json"); } catch { } };
        var openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = controlMargin };
        openProfilesJsonButton.Click += (s, e) => { try { Process.Start("profiles.json"); } catch { } };
        var openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = controlMargin };
        openUrlJsonButton.Click += (s, e) => { try { Process.Start("URL.json"); } catch { } };
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

        // --- LEFT COLUMN ---
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
        var windowFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        loginGoogleListButton = new Button { Text = "Đăng Nhập Google (Đã chọn)", AutoSize = true, Margin = controlMargin };
        openAllProfilesButton = new Button { Text = "Mở Toàn Bộ Profiles", AutoSize = true, Margin = controlMargin };
        arrangeButton = new Button { Text = "Sắp xếp tất cả", AutoSize = true, Margin = controlMargin };
        maximizeButton = new Button { Text = "Phóng to tất cả", AutoSize = true, Margin = controlMargin };
        minimizeButton = new Button { Text = "Thu nhỏ tất cả", AutoSize = true, Margin = controlMargin };
        restoreButton = new Button { Text = "Khôi phục tất cả", AutoSize = true, Margin = controlMargin };
        switchTabButton = new Button { Text = "Chuyển Tab", AutoSize = true, Margin = controlMargin };
        var columnsLabel = new Label { Text = "Số Cột:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) };
        var columnsNumericUpDown = new NumericUpDown { Width = 50, Value = 4, Minimum = 1, Maximum = 20, Margin = new Padding(5, 5, 5, 5) };
        var gapLabel = new Label { Text = "G.Cách:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) };
        var gapNumericUpDown = new NumericUpDown { Width = 50, Value = 5, Minimum = 0, Maximum = 100, Margin = controlMargin };
        windowFlowPanel.Controls.AddRange(new Control[] { loginGoogleListButton, openAllProfilesButton, arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, columnsLabel, columnsNumericUpDown, gapLabel, gapNumericUpDown });
        windowActionsGroupBox.Controls.Add(windowFlowPanel);
        
        var urlActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Hành động với URL", Height = 120, Padding = new Padding(10) };
        var urlFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var newUrlPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
        newUrlTextBox = new TextBox { Width = 380, Margin = new Padding(0,0,5,0) };
        addUrlButton = new Button { Text = "Thêm", AutoSize = true, Margin = new Padding(0, 0, 5, 0) };
        saveAndOpenUrlButton = new Button { Text = "Mở & Lưu", AutoSize = true, Margin = new Padding(0,0,5,0) };
        newUrlPanel.Controls.AddRange(new Control[] { newUrlTextBox, addUrlButton, saveAndOpenUrlButton });
        var urlButtonsPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true };
        openSelectedUrlButton = new Button { Text = "Mở URL (với Profile Nhanh)", AutoSize = true, Margin = controlMargin };
        deleteSelectedUrlButton = new Button { Text = "Xóa URL", AutoSize = true, Margin = controlMargin };
        openUrlWithAllProfilesButton = new Button { Text = "Mở URL với Toàn bộ Profiles", AutoSize = true, Margin = controlMargin };
        deleteAllUrlsButton = new Button { Text = "Xóa Tất Cả URLs", AutoSize = true, Margin = controlMargin };
        urlButtonsPanel.Controls.AddRange(new Control[] { openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton });
        urlFlowPanel.Controls.AddRange(new Control[] { newUrlPanel, urlButtonsPanel });
        urlActionsGroupBox.Controls.Add(urlFlowPanel);

        var urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsGroupBox.Controls.Add(urlsListBox);
        rightPanel.Controls.AddRange(new Control[] { urlsGroupBox, urlActionsGroupBox, windowActionsGroupBox, statusGroupBox });

        // --- Add Panels to Form ---
        this.Controls.AddRange(new Control[] { mainPanel, quickActionFlowPanel, pathFlowPanel, topFlowPanel });

        // --- Event Handlers ---
        chromePathComboBox.TextChanged += (s, e) => _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
        openUserDataButton.Click += (s, e) => { try { Process.Start(_profileManager.GetEffectiveUserDataPath()); } catch (Exception ex) { MessageBox.Show($"Không thể mở thư mục User Data. Lỗi: {ex.Message}");} };
        deletePathButton.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text) && MessageBox.Show("Bạn có chắc muốn xóa đường dẫn này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes) { _profileManager.DeleteChromePath(chromePathComboBox.Text); RefreshChromePathList(); } };
        discoverProfilesButton.Click += DiscoverProfilesButton_Click;
        profileComboBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) openChromeButton.PerformClick(); };
        openChromeButton.Click += OpenChromeFromComboBox;
        loginGoogleButton.Click += LoginGoogleFromComboBox;
        closeChromeButton.Click += (s, e) => _profileManager.CloseAllChrome();
        saveProfileButton.Click += SaveProfileButton_Click;
        profilesListBox.DoubleClick += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString()); };
        profilesListBox.SelectedIndexChanged += (s, e) => DisplayProfileDetails();
        urlsListBox.DoubleClick += (s, e) => openSelectedUrlButton.PerformClick();
        loginGoogleListButton.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl); else MessageBox.Show("Vui lòng chọn profile."); };
        openAllProfilesButton.Click += (s, e) => { foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile); };
        addUrlButton.Click += (s, e) => { if(!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) { _urlManager.AddUrl(newUrlTextBox.Text); newUrlTextBox.Clear(); RefreshUrlList(); } };
        saveAndOpenUrlButton.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) { string url = newUrlTextBox.Text; _urlManager.AddUrl(url); newUrlTextBox.Clear(); RefreshUrlList(); if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) _profileManager.OpenChrome(profileComboBox.Text, url); else MessageBox.Show("Chọn Profile ở ô Profile Nhanh."); } };
        openSelectedUrlButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null && !string.IsNullOrWhiteSpace(profileComboBox.Text)) _profileManager.OpenChrome(profileComboBox.Text, urlsListBox.SelectedItem.ToString()); else MessageBox.Show("Vui lòng chọn URL và nhập Profile ở ô Profile Nhanh."); };
        deleteSelectedUrlButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null) { _urlManager.DeleteUrl(urlsListBox.SelectedItem.ToString()); RefreshUrlList(); } };
        openUrlWithAllProfilesButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null) { foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile, urlsListBox.SelectedItem.ToString()); } else MessageBox.Show("Vui lòng chọn một URL."); };
        deleteAllUrlsButton.Click += (s, e) => { if (MessageBox.Show("Xóa tất cả URL?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes) { _urlManager.ClearAllUrls(); RefreshUrlList(); } };
        
        // Global Window Actions
        arrangeButton.Click += (s, e) => WindowManager.ArrangeChromeWindows((int)columnsNumericUpDown.Value, (int)gapNumericUpDown.Value, _profileManager.GetSelectedChromePath());
        maximizeButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 3); // SW_MAXIMIZE
        minimizeButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 6); // SW_MINIMIZE
        restoreButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 9); // SW_RESTORE
        switchTabButton.Click += (s, e) => WindowManager.CycleToNextChromeWindow(_profileManager.GetSelectedChromePath());

        // --- Context Menu for Profiles ---
        var profileContextMenu = new ContextMenuStrip();
        var openMenuItem = new ToolStripMenuItem("Mở Profile");
        var loginMenuItem = new ToolStripMenuItem("Đăng nhập Google");
        var closeMenuItem = new ToolStripMenuItem("Đóng Profile");
        var maximizeMenuItem = new ToolStripMenuItem("Phóng to");
        var minimizeMenuItem = new ToolStripMenuItem("Thu nhỏ");
        var restoreMenuItem = new ToolStripMenuItem("Khôi phục");
        var copyMenuItem = new ToolStripMenuItem("Sao chép Tên Profile");
        var deleteMenuItem = new ToolStripMenuItem("Xóa Profile...");
        
        openMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString()); };
        loginMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl); };
        closeMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.CloseProfileWindow(profilesListBox.SelectedItem.ToString()); };
        maximizeMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.MaximizeProfileWindow(profilesListBox.SelectedItem.ToString()); };
        minimizeMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.MinimizeProfileWindow(profilesListBox.SelectedItem.ToString()); };
        restoreMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.RestoreProfileWindow(profilesListBox.SelectedItem.ToString()); };
        copyMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) Clipboard.SetText(profilesListBox.SelectedItem.ToString()); };
        deleteMenuItem.Click += (s, e) => { if (profilesListBox.SelectedItem != null) { _profileManager.DeleteProfile(profilesListBox.SelectedItem.ToString()); RefreshProfileLists(); } };

        profileContextMenu.Items.AddRange(new ToolStripItem[] { 
            openMenuItem, 
            loginMenuItem, 
            new ToolStripSeparator(),
            maximizeMenuItem,
            minimizeMenuItem,
            restoreMenuItem,
            closeMenuItem,
            new ToolStripSeparator(), 
            copyMenuItem, 
            new ToolStripSeparator(), 
            deleteMenuItem 
        });
        profilesListBox.ContextMenuStrip = profileContextMenu;
        profilesListBox.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) { int index = profilesListBox.IndexFromPoint(e.Location); if (index != ListBox.NoMatches) profilesListBox.SelectedIndex = index; } };

        // --- Timer ---
        statusUpdateTimer = new Timer { Interval = 1000 };
        statusUpdateTimer.Tick += (s, e) => UpdateChromeWindowStatus();
        statusUpdateTimer.Start();

        this.ResumeLayout(false);
    }
    
    // --- Methods ---
    private void DiscoverProfilesButton_Click(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text) && File.Exists(chromePathComboBox.Text)) {
             _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
             RefreshChromePathList();
        } else {
            MessageBox.Show("Đường dẫn trình duyệt không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        int newCount = _profileManager.DiscoverAndAddProfiles();
        MessageBox.Show(newCount > 0 ? $"Đã tìm thấy và thêm mới {newCount} profile." : "Không tìm thấy profile mới nào.", "Hoàn tất");
        if (newCount > 0) RefreshProfileLists();
    }

    private void OpenChromeFromComboBox(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
            _profileManager.AddProfile(profileComboBox.Text);
            _profileManager.OpenChrome(profileComboBox.Text);
            RefreshProfileLists();
        } else {
             MessageBox.Show("Vui lòng nhập tên profile.");
        }
    }

    private void LoginGoogleFromComboBox(object sender, EventArgs e) {
         if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
            _profileManager.AddProfile(profileComboBox.Text);
            _profileManager.OpenChrome(profileComboBox.Text, Pro5ChromeManager.GoogleLoginUrl);
            RefreshProfileLists();
        } else {
            MessageBox.Show("Vui lòng nhập tên profile.");
        }
    }

    private void SaveProfileButton_Click(object sender, EventArgs e) {
        if (profilesListBox.SelectedItem != null) {
            _profileManager.UpdateProfileDetails(profilesListBox.SelectedItem.ToString(), emailTextBox.Text, passwordTextBox.Text, otpTextBox.Text);
            MessageBox.Show("Đã lưu thông tin.", "Thành công");
        }
    }
    
    private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        bool isChecked = alwaysOnTopCheckBox.Checked;
        this.TopMost = isChecked;
        _profileManager.SetAlwaysOnTop(isChecked);
    }

    private void DisplayProfileDetails() {
        if (profilesListBox.SelectedItem != null) {
            var profile = _profileManager.GetProfileDetails(profilesListBox.SelectedItem.ToString());
            emailTextBox.Text = profile?.Email ?? "";
            passwordTextBox.Text = profile?.Password ?? "";
            otpTextBox.Text = profile?.Otp ?? "";
            saveProfileButton.Enabled = true;
            emailTextBox.Enabled = true;
            passwordTextBox.Enabled = true;
            otpTextBox.Enabled = true;
        } else {
            emailTextBox.Clear();
            passwordTextBox.Clear();
            otpTextBox.Clear();
            saveProfileButton.Enabled = false;
            emailTextBox.Enabled = false;
            passwordTextBox.Enabled = false;
            otpTextBox.Enabled = false;
        }
    }

    private void UpdateChromeWindowStatus() {
        var (activeTitle, inactiveTitles) = WindowManager.GetChromeWindowStates(_profileManager.GetSelectedChromePath());
        currentProfileTextBox.Text = activeTitle;
        closingProfileTextBox.Lines = inactiveTitles.ToArray();
    }

    private void RefreshChromePathList() {
        var paths = _profileManager.GetChromePaths();
        var selectedPath = _profileManager.GetSelectedChromePath();
        string currentText = chromePathComboBox.Text;
        chromePathComboBox.Items.Clear();
        chromePathComboBox.Items.AddRange(paths.ToArray());
        chromePathComboBox.Text = selectedPath ?? currentText;
    }

    private void RefreshProfileLists() {
        var profiles = _profileManager.GetProfiles();
        string selectedListBoxItem = profilesListBox.SelectedItem as string;
        profilesListBox.Items.Clear();
        profileComboBox.Items.Clear();
        var sortedProfiles = profiles.OrderBy(p => p.Equals("Default", StringComparison.OrdinalIgnoreCase) ? -1 : (int.TryParse(p.Replace("Profile ", ""), out int n) ? n : int.MaxValue)).ToArray();
        profilesListBox.Items.AddRange(sortedProfiles);
        profileComboBox.Items.AddRange(sortedProfiles);
        if(selectedListBoxItem != null && profilesListBox.Items.Contains(selectedListBoxItem)) {
             profilesListBox.SelectedItem = selectedListBoxItem;
        }
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        DisplayProfileDetails();
    }

    private void RefreshUrlList() {
        var urls = _urlManager.GetUrls();
        urlsListBox.Items.Clear();
        urlsListBox.Items.AddRange(urls.ToArray());
    }

    private void MainForm_Load(object sender, EventArgs e) {
        RefreshChromePathList();
        RefreshProfileLists();
        RefreshUrlList();

        bool alwaysOnTop = _profileManager.IsAlwaysOnTop();
        this.TopMost = alwaysOnTop;
        alwaysOnTopCheckBox.Checked = alwaysOnTop;

        DisplayProfileDetails(); // Initial state for profile details
    }

    protected override void Dispose(bool disposing) {
        if (disposing && statusUpdateTimer != null) {
            statusUpdateTimer.Stop();
            statusUpdateTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
