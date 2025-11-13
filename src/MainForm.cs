
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net; // Added for URL Encoding
using System.Windows.Forms;

public class MainForm : Form
{
    // Business Logic Managers
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;

    // --- UI CONTROLS ---
    private Timer statusUpdateTimer;

    // Top Bar
    private Button openConfigButton;
    private Button openProfilesJsonButton;
    private Button openUrlJsonButton;
    private CheckBox alwaysOnTopCheckBox;
    private CheckBox showInTaskbarCheckBox;

    // Chrome Path
    private ComboBox chromePathComboBox;
    private Button openUserDataButton;
    private Button deletePathButton;

    // Profile Selection
    private ComboBox profileComboBox;
    private Button openChromeButton;
    private Button loginGoogleButton; 
    private Button closeChromeButton;

    // --- Main Area ---

    // Left Column
    private GroupBox profilesGroupBox;
    private Label profileCountLabel;
    private ListBox profilesListBox;
    private ContextMenuStrip profileContextMenu;

    private TextBox newUrlTextBox;
    private Button saveAndOpenUrlButton;
    private Button addUrlButton;

    private GroupBox urlsGroupBox;
    private ListBox urlsListBox;
    
    // Right Column
    private Button loginGoogleListButton;
    private Button openAllProfilesButton;
    private Button maximizeButton;
    private Button switchTabButton; 
    private Button arrangeButton;
    private Button minimizeButton;
    private Button restoreButton;
    private Button closeWindowButton; 
    private Label columnsLabel;
    private NumericUpDown columnsNumericUpDown;
    private Label gapLabel;
    private NumericUpDown gapNumericUpDown;

    private Label currentProfileLabel;
    private TextBox currentProfileTextBox;
    private Label closingProfileLabel;
    private TextBox closingProfileTextBox;

    private Button openSelectedUrlButton;
    private Button deleteSelectedUrlButton;
    private Button openUrlWithAllProfilesButton;
    private Button deleteAllUrlsButton;

    // Bottom Bar
    private Label emailLabel;
    private TextBox emailTextBox;
    private Label passwordLabel;
    private TextBox passwordTextBox;
    private Button prefillEmailLoginButton; // Renamed from seleniumLoginButton

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

        // --- Top Bar ---
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
            if (!string.IsNullOrWhiteSpace(chromePathComboBox.Text)) {
                 if (MessageBox.Show("Bạn có chắc muốn xóa đường dẫn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                 {
                    _profileManager.DeleteChromePath(chromePathComboBox.Text);
                    RefreshChromePathList();
                 }
            }
        };

        topPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox, showInTaskbarCheckBox, chromePathComboBox, openUserDataButton, deletePathButton });

        // --- Profile Selection Bar ---
        var profileSelectionPanel = new Panel { Dock = DockStyle.Top, Height = 40, BorderStyle = BorderStyle.FixedSingle, Top = topPanel.Height };
        
        profileComboBox = new ComboBox { Location = new Point(150, 8), Width = 300, DropDownStyle = ComboBoxStyle.DropDown };
        openChromeButton = new Button { Text = "Mở Chrome", Location = new Point(profileComboBox.Right + 10, 7), AutoSize = true };
        openChromeButton.Click += (s, e) => 
        {
            if (!string.IsNullOrWhiteSpace(profileComboBox.Text))
            {
                 _profileManager.OpenChrome(profileComboBox.Text);
                 RefreshProfileLists();
            }
        };
        
        loginGoogleButton = new Button { Text = "Đăng Nhập Google", Location = new Point(openChromeButton.Right + 5, 7), AutoSize = true };
        loginGoogleButton.Click += (s, e) => 
        {
             if (!string.IsNullOrWhiteSpace(profileComboBox.Text))
            {
                _profileManager.OpenChrome(profileComboBox.Text, Pro5ChromeManager.GoogleLoginUrl);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một profile.");
            }
        };

        closeChromeButton = new Button { Text = "Đóng Chrome", Location = new Point(loginGoogleButton.Right + 5, 7), AutoSize = true };
        closeChromeButton.Click += (s, e) => _profileManager.CloseAllChrome();

        profileSelectionPanel.Controls.AddRange(new Control[] { new Label { Text = "Chọn hoặc Nhập Profile:", Location = new Point(10, 10), AutoSize=true}, profileComboBox, openChromeButton, loginGoogleButton, closeChromeButton });

        // --- Bottom Bar ---
        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BorderStyle = BorderStyle.FixedSingle };
        
        emailLabel = new Label { Text = "Email:", Location = new Point(10, 12), AutoSize = true };
        emailTextBox = new TextBox { Location = new Point(emailLabel.Right + 5, 10), Width = 200 };
        passwordLabel = new Label { Text = "Password:", Location = new Point(emailTextBox.Right + 10, 12), AutoSize = true };
        passwordTextBox = new TextBox { Location = new Point(passwordLabel.Right + 5, 10), Width = 200, UseSystemPasswordChar = true };
        
        prefillEmailLoginButton = new Button { Text = "Điền Email & Đăng nhập", Location = new Point(passwordTextBox.Right + 10, 9), AutoSize = true, Enabled = true }; // Changed
        prefillEmailLoginButton.Click += PrefillEmailLoginButton_Click; // Added

        bottomPanel.Controls.AddRange(new Control[] { emailLabel, emailTextBox, passwordLabel, passwordTextBox, prefillEmailLoginButton });

        // --- Main Content Area ---
        var mainPanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
        
        // --- Left Column ---
        var leftPanel = new Panel { Dock = DockStyle.Left, Width = 450, Padding = new Padding(5) };

        // --- Context Menu for Profiles ---
        profileContextMenu = new ContextMenuStrip();
        var loginItem = new ToolStripMenuItem("Đăng nhập Google...");
        var copyItem = new ToolStripMenuItem("Sao chép Tên Profile");
        var deleteItem = new ToolStripMenuItem("Xóa Profile...");
        profileContextMenu.Items.AddRange(new ToolStripItem[] { loginItem, copyItem, new ToolStripSeparator(), deleteItem });

        profileContextMenu.Opening += (s, e) => {
            if (profilesListBox.SelectedItem == null) {
                e.Cancel = true; // Don't show menu if no item is selected
            }
        };

        loginItem.Click += (s, e) => {
            string selectedProfile = profilesListBox.SelectedItem.ToString();
            _profileManager.OpenChrome(selectedProfile, Pro5ChromeManager.GoogleLoginUrl);
        };

        copyItem.Click += (s, e) => {
            if(profilesListBox.SelectedItem != null) Clipboard.SetText(profilesListBox.SelectedItem.ToString());
        };

        deleteItem.Click += (s, e) => {
            if(profilesListBox.SelectedItem != null) {
                string selectedProfile = profilesListBox.SelectedItem.ToString();
                _profileManager.DeleteProfile(selectedProfile);
                RefreshProfileLists();
            }
        };

        profilesGroupBox = new GroupBox { Dock = DockStyle.Top, Text = "Danh sách Profiles", Height = 300, Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0,0,0,5) };
        profilesListBox = new ListBox { Dock = DockStyle.Fill, ContextMenuStrip = profileContextMenu };
        profilesListBox.MouseDown += (s, e) => {
            if (e.Button == MouseButtons.Right) {
                int index = profilesListBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches) {
                    profilesListBox.SelectedIndex = index;
                }
            }
        };

        profilesListBox.DoubleClick += (s, e) => 
        {
             if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString());
        };
        profilesGroupBox.Controls.Add(profilesListBox);
        profilesGroupBox.Controls.Add(profileCountLabel);
        
        var newUrlPanel = new Panel { Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 10, 0, 0), Top = profilesGroupBox.Bottom + 10 };
        newUrlTextBox = new TextBox { Dock = DockStyle.Fill };
        addUrlButton = new Button { Text = "Thêm URL mới", Dock = DockStyle.Right, AutoSize = true };
        addUrlButton.Click += (s, e) => 
        {
            if(!string.IsNullOrWhiteSpace(newUrlTextBox.Text))
            {
                _urlManager.AddUrl(newUrlTextBox.Text);
                newUrlTextBox.Clear();
                RefreshUrlList();
            }
        };
        saveAndOpenUrlButton = new Button { Text = "Mở và Lưu URL", Dock = DockStyle.Right, AutoSize = true };
        saveAndOpenUrlButton.Click += (s, e) => 
        {
            if (!string.IsNullOrWhiteSpace(newUrlTextBox.Text))
            {
                string url = newUrlTextBox.Text;
                _urlManager.AddUrl(url);
                newUrlTextBox.Clear();
                RefreshUrlList();
                
                if (!string.IsNullOrWhiteSpace(profileComboBox.Text))
                {
                    _profileManager.OpenChrome(profileComboBox.Text, url);
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một Profile để mở URL.", "Thông báo");
                }
            }
        };
        newUrlPanel.Controls.Add(newUrlTextBox);
        newUrlPanel.Controls.Add(addUrlButton);
        newUrlPanel.Controls.Add(saveAndOpenUrlButton);

        urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10), Top = newUrlPanel.Bottom + 10 };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsGroupBox.Controls.Add(urlsListBox);
        
        leftPanel.Controls.Add(urlsGroupBox);
        leftPanel.Controls.Add(newUrlPanel);
        leftPanel.Controls.Add(profilesGroupBox);

        // --- Right Column ---
        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        var rightTopButtonsPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.LeftToRight, WrapContents = true};
        loginGoogleListButton = new Button { Text = "Đăng Nhập Google (Danh sách)", AutoSize = true };
        loginGoogleListButton.Click += (s, e) => {
            if (profilesListBox.SelectedItem != null)
            {
                string selectedProfile = profilesListBox.SelectedItem.ToString();
                _profileManager.OpenChrome(selectedProfile, Pro5ChromeManager.GoogleLoginUrl);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một profile từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };

        openAllProfilesButton = new Button { Text = "Mở Toàn Bộ Chrome", AutoSize = true };
        openAllProfilesButton.Click += (s, e) => 
        {
            foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile);
        };
        
        maximizeButton = new Button { Text = "Phóng to", AutoSize = true };
        maximizeButton.Click += (s, e) => WindowManager.MaximizeAllWindows();
        
        switchTabButton = new Button { Text = "Chuyển Tab", AutoSize = true };
        switchTabButton.Click += (s, e) => WindowManager.CycleToNextChromeWindow();
        
        arrangeButton = new Button { Text = "Sắp xếp", AutoSize = true };
        arrangeButton.Click += (s, e) => WindowManager.ArrangeChromeWindows((int)columnsNumericUpDown.Value, (int)gapNumericUpDown.Value);
        
        minimizeButton = new Button { Text = "Thu nhỏ", AutoSize = true };
        minimizeButton.Click += (s, e) => WindowManager.MinimizeAllWindows();
        
        restoreButton = new Button { Text = "Khôi Phục", AutoSize = true };
        restoreButton.Click += (s, e) => WindowManager.RestoreAllWindows();
        
        closeWindowButton = new Button { Text = "Đóng", AutoSize = true };
        closeWindowButton.Click += (s, e) => _profileManager.CloseAllChrome();

        rightTopButtonsPanel.Controls.AddRange(new Control[] { loginGoogleListButton, openAllProfilesButton, maximizeButton, switchTabButton, arrangeButton, minimizeButton, restoreButton, closeWindowButton });

        var arrangeOptionsPanel = new Panel { Dock = DockStyle.Top, Height = 30 };
        columnsLabel = new Label { Text = "Số Cột:", Location = new Point(5, 6), AutoSize = true };
        columnsNumericUpDown = new NumericUpDown { Location = new Point(columnsLabel.Right + 5, 4), Width = 50, Value = 2, Minimum = 1, Maximum = 20 };
        gapLabel = new Label { Text = "G.Cách:", Location = new Point(columnsNumericUpDown.Right + 10, 6), AutoSize = true };
        gapNumericUpDown = new NumericUpDown { Location = new Point(gapLabel.Right + 5, 4), Width = 50, Value = 0, Minimum = 0, Maximum = 100 };
        arrangeOptionsPanel.Controls.AddRange(new Control[] { columnsLabel, columnsNumericUpDown, gapLabel, gapNumericUpDown });
        
        var statusPanel = new GroupBox { Dock = DockStyle.Top, Text = "Trạng thái", Height = 120, Padding = new Padding(10) };
        currentProfileLabel = new Label { Text = "Profile đang mở", Dock = DockStyle.Top };
        currentProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
        closingProfileLabel = new Label { Text = "Profile chuẩn bị đóng", Dock = DockStyle.Top };
        closingProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 60 };
        statusPanel.Controls.Add(closingProfileTextBox);
        statusPanel.Controls.Add(closingProfileLabel);
        statusPanel.Controls.Add(currentProfileTextBox);
        statusPanel.Controls.Add(currentProfileLabel);

        var urlButtonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(0, 20, 0, 0) };
        openSelectedUrlButton = new Button { Text = "Mở URL được chọn", AutoSize = true };
        openSelectedUrlButton.Click += (s, e) => 
        {
            if (urlsListBox.SelectedItem != null && !string.IsNullOrWhiteSpace(profileComboBox.Text))
            {
                _profileManager.OpenChrome(profileComboBox.Text, urlsListBox.SelectedItem.ToString());
            } 
            else 
            {
                MessageBox.Show("Vui lòng chọn một URL và một Profile.", "Thông báo");
            }
        };
        
        deleteSelectedUrlButton = new Button { Text = "Xóa URL được chọn", AutoSize = true };
        deleteSelectedUrlButton.Click += (s, e) => 
        {
            if (urlsListBox.SelectedItem != null)
            {
                _urlManager.DeleteUrl(urlsListBox.SelectedItem.ToString());
                RefreshUrlList();
            }
        };
        
        openUrlWithAllProfilesButton = new Button { Text = "Mở URL với Toàn Bộ Profiles", AutoSize = true };
        openUrlWithAllProfilesButton.Click += (s, e) =>
        {
            if (urlsListBox.SelectedItem != null)
            {
                foreach(var profile in _profileManager.GetProfiles())
                {
                    _profileManager.OpenChrome(profile, urlsListBox.SelectedItem.ToString());
                }
            }
            else
            {
                 MessageBox.Show("Vui lòng chọn một URL.", "Thông báo");
            }
        };

        deleteAllUrlsButton = new Button { Text = "Xóa danh sách URLs", AutoSize = true };
        deleteAllUrlsButton.Click += (s, e) => 
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa tất cả URL?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                 _urlManager.ClearAllUrls();
                 RefreshUrlList();
            }
        };
        urlButtonsPanel.Controls.AddRange(new Control[] { openSelectedUrlButton, deleteSelectedUrlButton, openUrlWithAllProfilesButton, deleteAllUrlsButton });

        rightPanel.Controls.Add(urlButtonsPanel);
        rightPanel.Controls.Add(statusPanel);
        rightPanel.Controls.Add(arrangeOptionsPanel);
        rightPanel.Controls.Add(rightTopButtonsPanel);

        mainPanel.Controls.Add(rightPanel);
        mainPanel.Controls.Add(leftPanel);
        
        this.Controls.Add(mainPanel);
        this.Controls.Add(bottomPanel);
        this.Controls.Add(profileSelectionPanel);
        this.Controls.Add(topPanel);

        // --- Timer for Status Updates ---
        statusUpdateTimer = new Timer();
        statusUpdateTimer.Interval = 1000; // 1 second
        statusUpdateTimer.Tick += (s, e) => UpdateChromeWindowStatus();
        statusUpdateTimer.Start();

        this.ResumeLayout(false);
    }

    private void PrefillEmailLoginButton_Click(object sender, EventArgs e)
    {
        string email = emailTextBox.Text;
        if (string.IsNullOrWhiteSpace(email))
        {
            MessageBox.Show("Vui lòng nhập địa chỉ email.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string selectedProfile = profilesListBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(selectedProfile))
        {
            selectedProfile = profileComboBox.Text;
        }

        if (string.IsNullOrWhiteSpace(selectedProfile))
        {
            MessageBox.Show("Vui lòng chọn một profile từ danh sách hoặc ô nhập liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        string baseUrl = "https://accounts.google.com/AccountChooser";
        string continueUrl = "https://www.google.com";
        string loginUrl = $"{baseUrl}?Email={WebUtility.UrlEncode(email)}&continue={WebUtility.UrlEncode(continueUrl)}";

        _profileManager.OpenChrome(selectedProfile, loginUrl);
        _profileManager.AddProfile(selectedProfile);
        RefreshProfileLists();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        RefreshChromePathList();
        RefreshProfileLists();
        RefreshUrlList();
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
