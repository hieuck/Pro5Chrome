
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

public class MainForm : Form
{
    // Managers
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;
    private Timer statusUpdateTimer;

    // --- UI CONTROLS ---
    private Button discoverProfilesButton, openUserDataButton, deletePathButton;
    private ComboBox chromePathComboBox;
    // ... other controls
    private GroupBox profilesGroupBox, profileDetailsGroupBox, urlsGroupBox, urlActionsGroupBox, windowActionsGroupBox, statusGroupBox;
    private ListBox profilesListBox, urlsListBox;
    private TextBox emailTextBox, passwordTextBox, newUrlTextBox, currentProfileTextBox, closingProfileTextBox;
    private Button saveProfileButton, addUrlButton, saveAndOpenUrlButton, openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton;
    private Button arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, closeWindowButton, loginGoogleListButton, openAllProfilesButton;
    private ComboBox profileComboBox;
    private Button openChromeButton, loginGoogleButton, closeChromeButton;
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
        this.Size = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        // Define a standard margin for controls
        Padding controlMargin = new Padding(5);

        // --- Top Bar (Config, Paths) ---
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 70, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5)};
        
        // Using a FlowLayoutPanel for auto-arrangement
        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        var openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = controlMargin };
        openConfigButton.Click += (s, e) => { try { Process.Start("config.json"); } catch { } };

        var openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = controlMargin };
        openProfilesJsonButton.Click += (s, e) => { try { Process.Start("profiles.json"); } catch { } };

        var openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = controlMargin };
        openUrlJsonButton.Click += (s, e) => { try { Process.Start("URL.json"); } catch { } };
        
        var alwaysOnTopCheckBox = new CheckBox { Text = "Luôn trên cùng", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        alwaysOnTopCheckBox.CheckedChanged += (s, e) => { this.TopMost = alwaysOnTopCheckBox.Checked; };
        
        chromePathComboBox = new ComboBox { Width = 400, DropDownStyle = ComboBoxStyle.DropDown, Margin = new Padding(5, 5, 0, 0) };
        chromePathComboBox.SelectedIndexChanged += (s, e) => _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
        chromePathComboBox.TextChanged += (s, e) => _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
        
        openUserDataButton = new Button { Text = "Mở User Data", AutoSize = true, Margin = controlMargin };
        openUserDataButton.Click += (s, e) => { try { Process.Start(Pro5ChromeManager.GetUserDataPath()); } catch { } };

        deletePathButton = new Button { Text = "Xóa đường dẫn", AutoSize = true, Margin = controlMargin };
        deletePathButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text) && MessageBox.Show("Bạn có chắc muốn xóa đường dẫn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                _profileManager.DeleteChromePath(chromePathComboBox.Text);
                RefreshChromePathList();
            }
        };
        
        discoverProfilesButton = new Button { Text = "Đọc Profiles", AutoSize = true, Margin = controlMargin };
        discoverProfilesButton.Click += DiscoverProfilesButton_Click;

        topFlowPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox, chromePathComboBox, openUserDataButton, deletePathButton, discoverProfilesButton });
        topPanel.Controls.Add(topFlowPanel);

        // --- Profile Quick Action Bar ---
        var profileSelectionPanel = new Panel { Dock = DockStyle.Top, Height = 40, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5) };
        var quickActionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        
        var profileLabel = new Label { Text = "Profile Nhanh:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        profileComboBox = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDown, Margin = new Padding(5, 5, 0, 0) };
        profileComboBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) openChromeButton.PerformClick(); };
        
        openChromeButton = new Button { Text = "Mở", AutoSize = true, Margin = controlMargin };
        loginGoogleButton = new Button { Text = "Đăng Nhập Google", AutoSize = true, Margin = controlMargin };
        closeChromeButton = new Button { Text = "Đóng Tất Cả", AutoSize = true, Margin = controlMargin };
        
        openChromeButton.Click += OpenChromeFromComboBox;
        loginGoogleButton.Click += LoginGoogleFromComboBox;
        closeChromeButton.Click += (s, e) => _profileManager.CloseAllChrome();

        quickActionFlowPanel.Controls.AddRange(new Control[] { profileLabel, profileComboBox, openChromeButton, loginGoogleButton, closeChromeButton });
        profileSelectionPanel.Controls.Add(quickActionFlowPanel);

        // --- Main Content Area ---
        var mainPanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
        
        var rightPanel = new Panel { Dock = DockStyle.Right, Width = 550, Padding = new Padding(5) };
        var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        mainPanel.Controls.AddRange(new Control[] { leftPanel, rightPanel });

        // --- LEFT COLUMN: Profiles & Details ---

        profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Thông tin Profile", Height = 120, Padding = new Padding(10) };
        var emailLabel = new Label { Text = "Email:", Location = new Point(15, 25), AutoSize = true };
        emailTextBox = new TextBox { Location = new Point(90, 22), Width = 300 };
        var passwordLabel = new Label { Text = "Password:", Location = new Point(15, 55), AutoSize = true };
        passwordTextBox = new TextBox { Location = new Point(90, 52), Width = 220, UseSystemPasswordChar = true };
        saveProfileButton = new Button { Text = "Lưu", Location = new Point(passwordTextBox.Right + 10, 51), AutoSize = true, Enabled = false };
        saveProfileButton.Click += SaveProfileButton_Click;
        profileDetailsGroupBox.Controls.AddRange(new Control[] { emailLabel, emailTextBox, passwordLabel, passwordTextBox, saveProfileButton });
        
        profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0,0,0,5) };
        profilesListBox = new ListBox { Dock = DockStyle.Fill };
        profilesListBox.DoubleClick += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString()); };
        profilesListBox.SelectedIndexChanged += (s, e) => DisplayProfileDetails();
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListBox, profileCountLabel });

        leftPanel.Controls.AddRange(new Control[] { profilesGroupBox, profileDetailsGroupBox });
        
        // --- RIGHT COLUMN: Actions, URLs, Status ---

        statusGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Trạng thái Cửa sổ Chrome", Height = 130, Padding = new Padding(10) };
        var currentProfileLabel = new Label { Text = "Cửa sổ Active:", Dock = DockStyle.Top };
        currentProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true, Margin = new Padding(0, 0, 0, 5) };
        var closingProfileLabel = new Label { Text = "Các cửa sổ khác:", Dock = DockStyle.Top };
        closingProfileTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
        statusGroupBox.Controls.AddRange(new Control[] { closingProfileTextBox, closingProfileLabel, currentProfileTextBox, currentProfileLabel });

        windowActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Quản lý Cửa sổ & Profiles", Height = 130, Padding = new Padding(10) };
        var windowFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        
        loginGoogleListButton = new Button { Text = "Đăng Nhập Google (Đã chọn)", AutoSize = true, Margin = controlMargin };
        openAllProfilesButton = new Button { Text = "Mở Toàn Bộ Profiles", AutoSize = true, Margin = controlMargin };
        arrangeButton = new Button { Text = "Sắp xếp", AutoSize = true, Margin = controlMargin };
        maximizeButton = new Button { Text = "Phóng to", AutoSize = true, Margin = controlMargin };
        minimizeButton = new Button { Text = "Thu nhỏ", AutoSize = true, Margin = controlMargin };
        restoreButton = new Button { Text = "Khôi Phục", AutoSize = true, Margin = controlMargin };
        switchTabButton = new Button { Text = "Chuyển Tab", AutoSize = true, Margin = controlMargin };
        
        var columnsLabel = new Label { Text = "Số Cột:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) };
        var columnsNumericUpDown = new NumericUpDown { Width = 50, Value = 4, Minimum = 1, Maximum = 20, Margin = new Padding(5, 5, 5, 5) };
        var gapLabel = new Label { Text = "G.Cách:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) };
        var gapNumericUpDown = new NumericUpDown { Width = 50, Value = 5, Minimum = 0, Maximum = 100, Margin = controlMargin };

        loginGoogleListButton.Click += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl); else MessageBox.Show("Vui lòng chọn profile."); };
        openAllProfilesButton.Click += (s, e) => { foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile); };
        arrangeButton.Click += (s, e) => WindowManager.ArrangeChromeWindows((int)columnsNumericUpDown.Value, (int)gapNumericUpDown.Value);
        maximizeButton.Click += (s, e) => WindowManager.MaximizeAllWindows();
        minimizeButton.Click += (s, e) => WindowManager.MinimizeAllWindows();
        restoreButton.Click += (s, e) => WindowManager.RestoreAllWindows();
        switchTabButton.Click += (s, e) => WindowManager.CycleToNextChromeWindow();

        windowFlowPanel.Controls.AddRange(new Control[] { loginGoogleListButton, openAllProfilesButton, arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, columnsLabel, columnsNumericUpDown, gapLabel, gapNumericUpDown });
        windowActionsGroupBox.Controls.Add(windowFlowPanel);
        
        urlActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Hành động với URL", Height = 150, Padding = new Padding(10) };
        var newUrlLabel = new Label { Text = "Nhập URL mới:", Location = new Point(15, 25), AutoSize = true };
        newUrlTextBox = new TextBox { Location = new Point(110, 22), Width = 280 };
        addUrlButton = new Button { Text = "Thêm", Location = new Point(newUrlTextBox.Right + 5, 21), AutoSize = true };
        saveAndOpenUrlButton = new Button { Text = "Mở & Lưu", Location = new Point(addUrlButton.Right + 5, 21), AutoSize = true };
        
        addUrlButton.Click += (s, e) => { if(!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) { _urlManager.AddUrl(newUrlTextBox.Text); newUrlTextBox.Clear(); RefreshUrlList(); } };
        saveAndOpenUrlButton.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) { string url = newUrlTextBox.Text; _urlManager.AddUrl(url); newUrlTextBox.Clear(); RefreshUrlList(); if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) _profileManager.OpenChrome(profileComboBox.Text, url); else MessageBox.Show("Chọn Profile ở ô Profile Nhanh."); } };

        var urlButtonsPanel = new FlowLayoutPanel { Location = new Point(10, 55), Size = new Size(500, 85), FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        openSelectedUrlButton = new Button { Text = "Mở URL (với Profile Nhanh)", AutoSize = true, Margin = controlMargin };
        deleteSelectedUrlButton = new Button { Text = "Xóa URL", AutoSize = true, Margin = controlMargin };
        openUrlWithAllProfilesButton = new Button { Text = "Mở URL với Toàn bộ Profiles", AutoSize = true, Margin = controlMargin };
        deleteAllUrlsButton = new Button { Text = "Xóa Tất Cả URLs", AutoSize = true, Margin = controlMargin };

        openSelectedUrlButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null && !string.IsNullOrWhiteSpace(profileComboBox.Text)) _profileManager.OpenChrome(profileComboBox.Text, urlsListBox.SelectedItem.ToString()); else MessageBox.Show("Vui lòng chọn URL và nhập Profile ở ô Profile Nhanh."); };
        deleteSelectedUrlButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null) { _urlManager.DeleteUrl(urlsListBox.SelectedItem.ToString()); RefreshUrlList(); } };
        openUrlWithAllProfilesButton.Click += (s, e) => { if (urlsListBox.SelectedItem != null) { foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile, urlsListBox.SelectedItem.ToString()); } else MessageBox.Show("Vui lòng chọn một URL."); };
        deleteAllUrlsButton.Click += (s, e) => { if (MessageBox.Show("Xóa tất cả URL?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes) { _urlManager.ClearAllUrls(); RefreshUrlList(); } };
        
        urlButtonsPanel.Controls.AddRange(new Control[] { openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton });
        urlActionsGroupBox.Controls.AddRange(new Control[] { newUrlLabel, newUrlTextBox, addUrlButton, saveAndOpenUrlButton, urlButtonsPanel });

        urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsListBox.DoubleClick += (s, e) => openSelectedUrlButton.PerformClick();
        urlsGroupBox.Controls.Add(urlsListBox);
        
        rightPanel.Controls.AddRange(new Control[] { urlsGroupBox, urlActionsGroupBox, windowActionsGroupBox, statusGroupBox });

        // --- Add Panels to Form ---
        this.Controls.AddRange(new Control[] { mainPanel, profileSelectionPanel, topPanel });

        // --- Context Menu for Profiles ---
        var profileContextMenu = new ContextMenuStrip();
        var loginContextMenuItem = new ToolStripMenuItem("Đăng nhập Google...");
        loginContextMenuItem.Click += (s, e) => { if(profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl); };
        var copyContextMenuItem = new ToolStripMenuItem("Sao chép Tên Profile");
        copyContextMenuItem.Click += (s, e) => { if(profilesListBox.SelectedItem != null) Clipboard.SetText(profilesListBox.SelectedItem.ToString()); };
        var deleteContextMenuItem = new ToolStripMenuItem("Xóa Profile...");
        deleteContextMenuItem.Click += (s, e) => { if(profilesListBox.SelectedItem != null) { _profileManager.DeleteProfile(profilesListBox.SelectedItem.ToString()); RefreshProfileLists(); } };
        profileContextMenu.Items.AddRange(new ToolStripItem[] { loginContextMenuItem, copyContextMenuItem, new ToolStripSeparator(), deleteContextMenuItem });
        profilesListBox.ContextMenuStrip = profileContextMenu;
        profilesListBox.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) { int index = profilesListBox.IndexFromPoint(e.Location); if (index != ListBox.NoMatches) profilesListBox.SelectedIndex = index; } };

        // --- Timer for Status Updates ---
        statusUpdateTimer = new Timer { Interval = 1000 };
        statusUpdateTimer.Tick += (s, e) => UpdateChromeWindowStatus();
        statusUpdateTimer.Start();

        this.ResumeLayout(false);
    }
    
    // --- EVENT HANDLERS & METHODS ---

    private void OpenChromeFromComboBox(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
            _profileManager.AddProfile(profileComboBox.Text);
            _profileManager.OpenChrome(profileComboBox.Text);
            RefreshProfileLists();
        }
    }

    private void LoginGoogleFromComboBox(object sender, EventArgs e) {
         if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
            _profileManager.AddProfile(profileComboBox.Text);
            _profileManager.OpenChrome(profileComboBox.Text, Pro5ChromeManager.GoogleLoginUrl);
            RefreshProfileLists();
        } else {
            MessageBox.Show("Vui lòng chọn hoặc nhập một profile.");
        }
    }

    private void DiscoverProfilesButton_Click(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text)) {
             _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
             RefreshChromePathList();
        } else {
            MessageBox.Show("Vui lòng nhập đường dẫn Chrome.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        int newCount = _profileManager.DiscoverAndAddProfiles();
        MessageBox.Show($"Đã quét xong. Tìm thấy và thêm mới {newCount} profile.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshProfileLists();
    }

    private void MainForm_Load(object sender, EventArgs e) {
        RefreshChromePathList();
        RefreshProfileLists();
        RefreshUrlList();
    }

    private void SaveProfileButton_Click(object sender, EventArgs e) {
        if (profilesListBox.SelectedItem == null) return;
        string selectedProfileName = profilesListBox.SelectedItem.ToString();
        _profileManager.UpdateProfileDetails(selectedProfileName, emailTextBox.Text, passwordTextBox.Text);
        MessageBox.Show($"Đã cập nhật thông tin cho: {selectedProfileName}", "Thành công");
    }

    private void DisplayProfileDetails() {
        if (profilesListBox.SelectedItem == null) {
            emailTextBox.Clear();
            passwordTextBox.Clear();
            saveProfileButton.Enabled = false;
            return;
        }
        string selectedProfileName = profilesListBox.SelectedItem.ToString();
        var profile = _profileManager.GetProfileDetails(selectedProfileName);
        emailTextBox.Text = profile?.Email ?? "";
        passwordTextBox.Text = profile?.Password ?? "";
        saveProfileButton.Enabled = true;
    }

    private void UpdateChromeWindowStatus() {
        var states = WindowManager.GetChromeWindowStates();
        currentProfileTextBox.Text = states.ActiveWindowTitle;
        closingProfileTextBox.Lines = states.InactiveWindowTitles.ToArray();
    }

    private void RefreshChromePathList() {
        var paths = _profileManager.GetChromePaths();
        var selectedPath = _profileManager.GetSelectedChromePath();
        chromePathComboBox.Items.Clear();
        foreach (var path in paths) {
            chromePathComboBox.Items.Add(path);
        }
        if(!string.IsNullOrEmpty(selectedPath)) {
            chromePathComboBox.Text = selectedPath;
        }
    }

    private void RefreshProfileLists() {
        var profiles = _profileManager.GetProfiles();
        string currentComboBoxText = profileComboBox.Text;
        string selectedListBoxItem = profilesListBox.SelectedItem as string;

        profilesListBox.Items.Clear();
        profileComboBox.Items.Clear();
        
        var sortedProfiles = profiles.OrderBy(p => {
             if (p.Equals("Default", StringComparison.OrdinalIgnoreCase)) return -1;
             int.TryParse(p.Replace("profile", "").Trim(), out int n);
             return n;
        }).ToList();
        
        foreach (var profile in sortedProfiles) {
            profilesListBox.Items.Add(profile);
            profileComboBox.Items.Add(profile);
        }
        
        profileComboBox.Text = currentComboBoxText;
        if(selectedListBoxItem != null && profilesListBox.Items.Contains(selectedListBoxItem)) {
             profilesListBox.SelectedItem = selectedListBoxItem;
        }
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        DisplayProfileDetails();
    }

    private void RefreshUrlList() {
        var urls = _urlManager.GetUrls();
        urlsListBox.Items.Clear();
        foreach (var url in urls) {
            urlsListBox.Items.Add(url);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing && statusUpdateTimer != null) {
            statusUpdateTimer.Stop();
            statusUpdateTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
