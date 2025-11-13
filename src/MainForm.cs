
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

public class MainForm : Form
{
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;
    private Timer statusUpdateTimer;

    // --- UI CONTROLS ---

    // Top Bar
    private Button openConfigButton, openProfilesJsonButton, openUrlJsonButton;
    private CheckBox alwaysOnTopCheckBox, showInTaskbarCheckBox;
    private ComboBox chromePathComboBox;
    private Button openUserDataButton, deletePathButton;

    // Profile Quick Actions
    private ComboBox profileComboBox;
    private Button openChromeButton, loginGoogleButton, closeChromeButton;

    // Left Column: Profiles
    private GroupBox profilesGroupBox;
    private Label profileCountLabel;
    private ListBox profilesListBox;
    private ContextMenuStrip profileContextMenu;

    // Left Column: Profile Details (for future use)
    private GroupBox profileDetailsGroupBox;
    private Label emailLabel, passwordLabel;
    private TextBox emailTextBox, passwordTextBox;
    private Button saveProfileButton; 

    // Right Column: URLs
    private GroupBox urlsGroupBox;
    private ListBox urlsListBox;
    
    // Right Column: URL Actions
    private GroupBox urlActionsGroupBox;
    private TextBox newUrlTextBox;
    private Button addUrlButton, saveAndOpenUrlButton;
    private Button openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton;

    // Right Column: Window & Profile Actions
    private GroupBox windowActionsGroupBox;
    private Button arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, closeWindowButton;
    private Label columnsLabel, gapLabel;
    private NumericUpDown columnsNumericUpDown, gapNumericUpDown;
    private Button loginGoogleListButton, openAllProfilesButton;
        
    // Right Column: Status
    private GroupBox statusGroupBox;
    private Label currentProfileLabel, closingProfileLabel;
    private TextBox currentProfileTextBox, closingProfileTextBox;


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
        this.Size = new Size(1024, 768);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        // --- Top Bar (Config, Paths) ---
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 70, BorderStyle = BorderStyle.FixedSingle };
        
        openConfigButton = new Button { Text = "Mở config.json", Location = new Point(10, 10), AutoSize = true };
        openConfigButton.Click += (s, e) => { try { Process.Start("config.json"); } catch { } };

        openProfilesJsonButton = new Button { Text = "Mở profiles.json", Location = new Point(openConfigButton.Right + 5, 10), AutoSize = true };
        openProfilesJsonButton.Click += (s, e) => { try { Process.Start("profiles.json"); } catch { } };

        openUrlJsonButton = new Button { Text = "Mở URL.json", Location = new Point(openProfilesJsonButton.Right + 5, 10), AutoSize = true };
        openUrlJsonButton.Click += (s, e) => { try { Process.Start("URL.json"); } catch { } };
        
        alwaysOnTopCheckBox = new CheckBox { Text = "Luôn hiển thị trên cùng", Location = new Point(openUrlJsonButton.Right + 20, 12), AutoSize = true };
        alwaysOnTopCheckBox.CheckedChanged += (s, e) => { this.TopMost = alwaysOnTopCheckBox.Checked; };
        
        showInTaskbarCheckBox = new CheckBox { Text = "Ẩn thanh tác vụ", Location = new Point(alwaysOnTopCheckBox.Right + 10, 12), AutoSize = true };
        showInTaskbarCheckBox.CheckedChanged += (s, e) => { this.ShowInTaskbar = !showInTaskbarCheckBox.Checked; };

        chromePathComboBox = new ComboBox { Location = new Point(10, 40), Width = 400, DropDownStyle = ComboBoxStyle.DropDown };
        chromePathComboBox.SelectedIndexChanged += (s, e) => _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
        chromePathComboBox.TextChanged += (s, e) => _profileManager.AddAndSelectChromePath(chromePathComboBox.Text);
        
        openUserDataButton = new Button { Text = "Mở User Data", Location = new Point(chromePathComboBox.Right + 5, 39), AutoSize = true };
        openUserDataButton.Click += (s, e) => { try { Process.Start(Pro5ChromeManager.GetUserDataPath()); } catch { } };

        deletePathButton = new Button { Text = "Xóa đường dẫn đã chọn", Location = new Point(openUserDataButton.Right + 5, 39), AutoSize = true };
        deletePathButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text) && MessageBox.Show("Bạn có chắc muốn xóa đường dẫn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                _profileManager.DeleteChromePath(chromePathComboBox.Text);
                RefreshChromePathList();
            }
        };

        topPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox, showInTaskbarCheckBox, chromePathComboBox, openUserDataButton, deletePathButton });

        // --- Profile Quick Action Bar ---
        var profileSelectionPanel = new Panel { Dock = DockStyle.Top, Height = 40, BorderStyle = BorderStyle.FixedSingle };
        
        profileComboBox = new ComboBox { Location = new Point(150, 8), Width = 300, DropDownStyle = ComboBoxStyle.DropDown };
        profileComboBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) openChromeButton.PerformClick(); };
        
        openChromeButton = new Button { Text = "Mở Chrome", Location = new Point(profileComboBox.Right + 10, 7), AutoSize = true };
        openChromeButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
                _profileManager.OpenChrome(profileComboBox.Text);
                _profileManager.AddProfile(profileComboBox.Text); // Add profile if it's new
                RefreshProfileLists();
            }
        };
        
        loginGoogleButton = new Button { Text = "Đăng Nhập Google", Location = new Point(openChromeButton.Right + 5, 7), AutoSize = true };
        loginGoogleButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
                _profileManager.OpenChrome(profileComboBox.Text, Pro5ChromeManager.GoogleLoginUrl);
                _profileManager.AddProfile(profileComboBox.Text);
                RefreshProfileLists();
            } else {
                MessageBox.Show("Vui lòng chọn hoặc nhập một profile.");
            }
        };

        closeChromeButton = new Button { Text = "Đóng Chrome", Location = new Point(loginGoogleButton.Right + 5, 7), AutoSize = true };
        closeChromeButton.Click += (s, e) => _profileManager.CloseAllChrome();

        profileSelectionPanel.Controls.AddRange(new Control[] { new Label { Text = "Chọn hoặc Nhập Profile:", Location = new Point(10, 10), AutoSize=true}, profileComboBox, openChromeButton, loginGoogleButton, closeChromeButton });

        // --- Main Content Area ---
        var mainPanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
        
        // --- Right Panel (Column for actions) ---
        var rightPanel = new Panel { Dock = DockStyle.Right, Width = 550, Padding = new Padding(5) };
        mainPanel.Controls.Add(rightPanel);

        // --- Left Panel (Column for lists) ---
        var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        mainPanel.Controls.Add(leftPanel);

        // --- LEFT COLUMN CONTROLS ---

        // Profile Details GroupBox
        profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Thông tin Profile", Height = 110, Padding = new Padding(10) };
        emailLabel = new Label { Text = "Email:", Location = new Point(10, 25), AutoSize = true };
        emailTextBox = new TextBox { Location = new Point(emailLabel.Right + 5, 22), Width = 250 };
        passwordLabel = new Label { Text = "Password:", Location = new Point(10, 55), AutoSize = true };
        passwordTextBox = new TextBox { Location = new Point(passwordLabel.Right + 5, 52), Width = 250, UseSystemPasswordChar = true };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", Location = new Point(passwordTextBox.Right + 10, 51), AutoSize = true, Enabled = false }; // Initially disabled
        saveProfileButton.Click += SaveProfileButton_Click;
        profileDetailsGroupBox.Controls.AddRange(new Control[] { emailLabel, emailTextBox, passwordLabel, passwordTextBox, saveProfileButton });
        leftPanel.Controls.Add(profileDetailsGroupBox);

        // Profiles GroupBox
        profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0,0,0,5) };
        profilesListBox = new ListBox { Dock = DockStyle.Fill };
        profilesListBox.DoubleClick += (s, e) => { if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString()); };
        profilesListBox.SelectedIndexChanged += (s, e) => DisplayProfileDetails();
        profilesGroupBox.Controls.Add(profilesListBox);
        profilesGroupBox.Controls.Add(profileCountLabel);
        leftPanel.Controls.Add(profilesGroupBox);
        
        // --- RIGHT COLUMN CONTROLS ---

        // Status GroupBox
        statusGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Trạng thái Cửa sổ Chrome", Height = 120, Padding = new Padding(10) };
        currentProfileLabel = new Label { Text = "Cửa sổ Active:", Dock = DockStyle.Top };
        currentProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
        closingProfileLabel = new Label { Text = "Các cửa sổ khác:", Dock = DockStyle.Top };
        closingProfileTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
        statusGroupBox.Controls.AddRange(new Control[] { closingProfileTextBox, closingProfileLabel, currentProfileTextBox, currentProfileLabel });
        rightPanel.Controls.Add(statusGroupBox);

        // Window Actions GroupBox
        windowActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Quản lý Cửa sổ & Profiles", Height = 150, Padding = new Padding(10) };
        var windowFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        
        loginGoogleListButton = new Button { Text = "Đăng Nhập Google (Profile đã chọn)", AutoSize = true };
        loginGoogleListButton.Click += (s, e) => {
            if (profilesListBox.SelectedItem != null) {
                _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl);
            } else {
                MessageBox.Show("Vui lòng chọn một profile từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        openAllProfilesButton = new Button { Text = "Mở Toàn Bộ Profiles", AutoSize = true };
        openAllProfilesButton.Click += (s, e) => { foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile); };

        arrangeButton = new Button { Text = "Sắp xếp", AutoSize = true };
        arrangeButton.Click += (s, e) => WindowManager.ArrangeChromeWindows((int)columnsNumericUpDown.Value, (int)gapNumericUpDown.Value);
        maximizeButton = new Button { Text = "Phóng to", AutoSize = true };
        maximizeButton.Click += (s, e) => WindowManager.MaximizeAllWindows();
        minimizeButton = new Button { Text = "Thu nhỏ", AutoSize = true };
        minimizeButton.Click += (s, e) => WindowManager.MinimizeAllWindows();
        restoreButton = new Button { Text = "Khôi Phục", AutoSize = true };
        restoreButton.Click += (s, e) => WindowManager.RestoreAllWindows();
        switchTabButton = new Button { Text = "Chuyển Tab", AutoSize = true };
        switchTabButton.Click += (s, e) => WindowManager.CycleToNextChromeWindow();
        closeWindowButton = new Button { Text = "Đóng Tất Cả", AutoSize = true };
        closeWindowButton.Click += (s, e) => _profileManager.CloseAllChrome();

        columnsLabel = new Label { Text = "Số Cột:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
        columnsNumericUpDown = new NumericUpDown { Width = 50, Value = 2, Minimum = 1, Maximum = 20 };
        gapLabel = new Label { Text = "G.Cách:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
        gapNumericUpDown = new NumericUpDown { Width = 50, Value = 0, Minimum = 0, Maximum = 100 };

        windowFlowPanel.Controls.AddRange(new Control[] { loginGoogleListButton, openAllProfilesButton, arrangeButton, maximizeButton, minimizeButton, restoreButton, switchTabButton, closeWindowButton, columnsLabel, columnsNumericUpDown, gapLabel, gapNumericUpDown });
        windowActionsGroupBox.Controls.Add(windowFlowPanel);
        rightPanel.Controls.Add(windowActionsGroupBox);
        
        // URLs GroupBox
        urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsListBox.DoubleClick += (s, e) => openSelectedUrlButton.PerformClick();
        urlsGroupBox.Controls.Add(urlsListBox);
        
        // URL Actions GroupBox
        urlActionsGroupBox = new GroupBox { Dock = DockStyle.Bottom, Text = "Hành động với URL", Height = 140, Padding = new Padding(10) };
        newUrlTextBox = new TextBox { Location = new Point(10, 20), Width = 300 };
        addUrlButton = new Button { Text = "Thêm URL", Location = new Point(newUrlTextBox.Right + 5, 19), AutoSize = true };
        addUrlButton.Click += (s, e) => {
            if(!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) {
                _urlManager.AddUrl(newUrlTextBox.Text);
                newUrlTextBox.Clear();
                RefreshUrlList();
            }
        };
        saveAndOpenUrlButton = new Button { Text = "Mở & Lưu URL", Location = new Point(addUrlButton.Right + 5, 19), AutoSize = true };
        saveAndOpenUrlButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(newUrlTextBox.Text)) {
                string url = newUrlTextBox.Text;
                _urlManager.AddUrl(url);
                newUrlTextBox.Clear();
                RefreshUrlList();
                if (!string.IsNullOrWhiteSpace(profileComboBox.Text)) {
                    _profileManager.OpenChrome(profileComboBox.Text, url);
                } else {
                    MessageBox.Show("Vui lòng chọn một Profile để mở URL.", "Thông báo");
                }
            }
        };

        var urlButtonsPanel = new FlowLayoutPanel { Location = new Point(10, 50), Size = new Size(500, 80), FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        openSelectedUrlButton = new Button { Text = "Mở URL đã chọn (với Profile ở ô nhập)", AutoSize = true };
        openSelectedUrlButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null && !string.IsNullOrWhiteSpace(profileComboBox.Text)) {
                _profileManager.OpenChrome(profileComboBox.Text, urlsListBox.SelectedItem.ToString());
            } else {
                MessageBox.Show("Vui lòng chọn một URL và một Profile ở ô nhập liệu.", "Thông báo");
            }
        };
        deleteSelectedUrlButton = new Button { Text = "Xóa URL đã chọn", AutoSize = true };
        deleteSelectedUrlButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null) {
                _urlManager.DeleteUrl(urlsListBox.SelectedItem.ToString());
                RefreshUrlList();
            }
        };
        openUrlWithAllProfilesButton = new Button { Text = "Mở URL với Toàn Bộ Profiles", AutoSize = true };
        openUrlWithAllProfilesButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null) {
                foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile, urlsListBox.SelectedItem.ToString());
            } else {
                 MessageBox.Show("Vui lòng chọn một URL.", "Thông báo");
            }
        };
        deleteAllUrlsButton = new Button { Text = "Xóa Tất Cả URLs", AutoSize = true };
        deleteAllUrlsButton.Click += (s, e) => {
            if (MessageBox.Show("Bạn có chắc muốn xóa tất cả URL?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                 _urlManager.ClearAllUrls();
                 RefreshUrlList();
            }
        };
        urlButtonsPanel.Controls.AddRange(new Control[] { openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton });

        urlActionsGroupBox.Controls.AddRange(new Control[] { newUrlTextBox, addUrlButton, saveAndOpenUrlButton, urlButtonsPanel });
        
        // Add URL Groups to Right Panel
        rightPanel.Controls.Add(urlsGroupBox);
        rightPanel.Controls.Add(urlActionsGroupBox);


        // --- Add Panels to Form ---
        this.Controls.Add(mainPanel);
        this.Controls.Add(profileSelectionPanel); // This stays below the top panel
        this.Controls.Add(topPanel);

        // --- Context Menu for Profiles --
        profileContextMenu = new ContextMenuStrip();
        var loginItem = new ToolStripMenuItem("Đăng nhập Google...");
        loginItem.Click += (s, e) => { if(profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString(), Pro5ChromeManager.GoogleLoginUrl); };
        var copyItem = new ToolStripMenuItem("Sao chép Tên Profile");
        copyItem.Click += (s, e) => { if(profilesListBox.SelectedItem != null) Clipboard.SetText(profilesListBox.SelectedItem.ToString()); };
        var deleteItem = new ToolStripMenuItem("Xóa Profile...");
        deleteItem.Click += (s, e) => {
            if(profilesListBox.SelectedItem != null) {
                _profileManager.DeleteProfile(profilesListBox.SelectedItem.ToString());
                RefreshProfileLists();
            }
        };
        profileContextMenu.Items.AddRange(new ToolStripItem[] { loginItem, copyItem, new ToolStripSeparator(), deleteItem });
        profilesListBox.ContextMenuStrip = profileContextMenu;
        profilesListBox.MouseDown += (s, e) => {
            if (e.Button == MouseButtons.Right) {
                int index = profilesListBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches) profilesListBox.SelectedIndex = index;
            }
        };

        // --- Timer for Status Updates ---
        statusUpdateTimer = new Timer { Interval = 1000 };
        statusUpdateTimer.Tick += (s, e) => UpdateChromeWindowStatus();
        statusUpdateTimer.Start();

        this.ResumeLayout(false);
    }
    
    // --- EVENT HANDLERS & METHODS ---

    private void MainForm_Load(object sender, EventArgs e)
    {
        RefreshChromePathList();
        RefreshProfileLists();
        RefreshUrlList();
    }

    private void SaveProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem == null) return;
        
        string selectedProfileName = profilesListBox.SelectedItem.ToString();
        _profileManager.UpdateProfileDetails(selectedProfileName, emailTextBox.Text, passwordTextBox.Text);
        
        MessageBox.Show($"Đã cập nhật thông tin cho profile: {selectedProfileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DisplayProfileDetails()
    {
        if (profilesListBox.SelectedItem == null)
        {
            emailTextBox.Clear();
            passwordTextBox.Clear();
            saveProfileButton.Enabled = false;
            return;
        }

        string selectedProfileName = profilesListBox.SelectedItem.ToString();
        var profile = _profileManager.GetProfileDetails(selectedProfileName);

        if (profile != null)
        {
            emailTextBox.Text = profile.Email;
            passwordTextBox.Text = profile.Password;
        }
        else
        {
            emailTextBox.Clear();
            passwordTextBox.Clear();
        }
        saveProfileButton.Enabled = true;
    }

    private void UpdateChromeWindowStatus()
    {
        var states = WindowManager.GetChromeWindowStates();
        currentProfileTextBox.Text = states.ActiveWindowTitle;
        closingProfileTextBox.Lines = states.InactiveWindowTitles.ToArray();
    }

    private void RefreshChromePathList()
    {
        var paths = _profileManager.GetChromePaths();
        var selectedPath = _profileManager.GetSelectedChromePath();

        chromePathComboBox.Items.Clear();
        foreach (var path in paths)
        {
            chromePathComboBox.Items.Add(path);
        }
        chromePathComboBox.Text = selectedPath;
    }

    private void RefreshProfileLists()
    {
        var profiles = _profileManager.GetProfiles();
        string currentComboBoxText = profileComboBox.Text;
        string selectedListBoxItem = profilesListBox.SelectedItem as string;

        profilesListBox.Items.Clear();
        profileComboBox.Items.Clear();
        
        var sortedProfiles = profiles.OrderBy(p => {
             int.TryParse(p.Replace("profile", "").Trim(), out int n);
             return n;
        }).ToList();
        
        foreach (var profile in sortedProfiles)
        {
            profilesListBox.Items.Add(profile);
            profileComboBox.Items.Add(profile);
        }
        
        profileComboBox.Text = currentComboBoxText;
        if(selectedListBoxItem != null && profilesListBox.Items.Contains(selectedListBoxItem)) {
             profilesListBox.SelectedItem = selectedListBoxItem;
        }
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        DisplayProfileDetails(); // Refresh details view
    }

    private void RefreshUrlList()
    {
        var urls = _urlManager.GetUrls();
        urlsListBox.Items.Clear();
        foreach (var url in urls)
        {
            urlsListBox.Items.Add(url);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            statusUpdateTimer?.Stop();
            statusUpdateTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
