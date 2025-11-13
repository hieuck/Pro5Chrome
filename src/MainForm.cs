
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;
    private AutomationManager _automationManager;

    // --- UI Controls ---
    private ComboBox chromePathComboBox;
    private ListView profilesListView;
    private ListBox urlsListBox;
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox;
    private Button saveProfileButton, addUrlButton, deleteSelectedUrlButton, deleteAllUrlsButton, loginGoogleButton;
    private Button minimizeSelectedButton, maximizeSelectedButton, restoreSelectedButton, closeSelectedButton;
    private Button addPathButton, deletePathButton, discoverProfilesButton;
    private Button openConfigButton, openProfilesJsonButton, openUrlJsonButton; // FIX: Declare as class fields
    private CheckBox alwaysOnTopCheckBox;
    private Label profileCountLabel;

    public MainForm()
    {
        _profileManager = new Pro5ChromeManager();
        _urlManager = new UrlManager();
        _automationManager = new AutomationManager();
        InitializeComponent();
        this.Load += MainForm_Load;
        WireUpEventHandlers();
    }

    private void InitializeComponent()
    {
        this.Text = "Pro5Chrome Manager by hieuck";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        var mainTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        this.Controls.Add(mainTableLayout);

        // Top Bars
        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var pathFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        mainTableLayout.Controls.Add(topFlowPanel, 0, 0);
        mainTableLayout.Controls.Add(pathFlowPanel, 0, 1);

        // FIX: Initialize class fields, not local variables
        openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = new Padding(5) };
        openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = new Padding(5) };
        openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = new Padding(5) };
        alwaysOnTopCheckBox = new CheckBox { Text = "Luôn trên cùng", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        topFlowPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox });

        var pathLabel = new Label { Text = "Đường dẫn trình duyệt:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        chromePathComboBox = new ComboBox { Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5, 5, 0, 0) };
        addPathButton = new Button { Text = "Thêm...", AutoSize = true, Margin = new Padding(5) };
        deletePathButton = new Button { Text = "Xóa", AutoSize = true, Margin = new Padding(5) };
        discoverProfilesButton = new Button { Text = "Quét Profiles", AutoSize = true, Margin = new Padding(5) };
        pathFlowPanel.Controls.AddRange(new Control[] { pathLabel, chromePathComboBox, addPathButton, deletePathButton, discoverProfilesButton });

        // Main Split Container
        var mainSplitContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 350, BorderStyle = BorderStyle.Fixed3D };
        mainTableLayout.Controls.Add(mainSplitContainer, 0, 2);

        // Profiles List Panel
        var profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles (Nháy đúp để mở)", Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0, 0, 0, 5) };
        profilesListView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
        profilesListView.Columns.Add("#", 40);
        profilesListView.Columns.Add("Tên Profile", 250);
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListView, profileCountLabel });
        mainSplitContainer.Panel1.Controls.Add(profilesGroupBox);

        // Right Panel (Tabs + Window Management)
        var rightTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        rightTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        mainSplitContainer.Panel2.Controls.Add(rightTableLayout);

        var mainTabControl = new TabControl { Dock = DockStyle.Fill };
        rightTableLayout.Controls.Add(mainTabControl, 0, 0);

        // Window Management Panel
        var windowActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Quản lý Cửa sổ (Profile đã chọn)", Padding = new Padding(10) };
        var windowActionsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        minimizeSelectedButton = new Button { Text = "Thu nhỏ", AutoSize = true, Margin = new Padding(5) };
        maximizeSelectedButton = new Button { Text = "Phóng to", AutoSize = true, Margin = new Padding(5) };
        restoreSelectedButton = new Button { Text = "Khôi phục", AutoSize = true, Margin = new Padding(5) };
        closeSelectedButton = new Button { Text = "Đóng", AutoSize = true, Margin = new Padding(5), ForeColor = Color.Red };
        windowActionsFlowPanel.Controls.AddRange(new Control[] { minimizeSelectedButton, maximizeSelectedButton, restoreSelectedButton, closeSelectedButton });
        windowActionsGroupBox.Controls.Add(windowActionsFlowPanel);
        rightTableLayout.Controls.Add(windowActionsGroupBox, 0, 1);

        // Tabs
        InitializeTabs(mainTabControl);

        this.ResumeLayout(false);
    }
    
    private void InitializeTabs(TabControl mainTabControl)
    {
        // Details & Automation Tab
        var detailsTabPage = new TabPage("Chi tiết & Tự động hóa");
        var detailsTablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        detailsTablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
        detailsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsTabPage.Controls.Add(detailsTablePanel);

        var profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Thông tin Profile", Padding = new Padding(10) };
        emailTextBox = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450, Location = new Point(100, 27) };
        passwordTextBox = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450, Location = new Point(100, 57), UseSystemPasswordChar = true };
        otpTextBox = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450, Location = new Point(100, 87) };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", AutoSize = true, Location = new Point(98, 125) };
        profileDetailsGroupBox.Controls.AddRange(new Control[] { new Label { Text = "Email:", Location = new Point(15, 30), AutoSize = true }, emailTextBox, new Label { Text = "Password:", Location = new Point(15, 60), AutoSize = true }, passwordTextBox, new Label { Text = "OTP Secret:", Location = new Point(15, 90), AutoSize = true }, otpTextBox, saveProfileButton });
        
        var automationGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Kịch bản Tự động hóa", Padding = new Padding(20) };
        loginGoogleButton = new Button { Text = "Tự động Đăng nhập Google", AutoSize = true, Height = 30, Padding = new Padding(5) };
        automationGroupBox.Controls.Add(loginGoogleButton);
        
        detailsTablePanel.Controls.AddRange(new Control[] { profileDetailsGroupBox, automationGroupBox });
        mainTabControl.TabPages.Add(detailsTabPage);

        // URL Management Tab
        var urlTabPage = new TabPage("Quản lý URL");
        var urlTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        urlTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        urlTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        urlTabPage.Controls.Add(urlTableLayout);

        var urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs (Nháy đúp để mở)", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsGroupBox.Controls.Add(urlsListBox);
        urlTableLayout.Controls.Add(urlsGroupBox, 0, 0);

        var urlActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Hành động", Padding = new Padding(10) };
        var urlActionsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        newUrlTextBox = new TextBox { Width = 350, Margin = new Padding(5) };
        addUrlButton = new Button { Text = "Thêm URL", AutoSize = true, Margin = new Padding(5) };
        deleteSelectedUrlButton = new Button { Text = "Xóa URL", AutoSize = true, Margin = new Padding(5) };
        deleteAllUrlsButton = new Button { Text = "Xóa tất cả", AutoSize = true, Margin = new Padding(5), ForeColor = Color.Red };
        urlActionsFlowPanel.Controls.AddRange(new Control[] { newUrlTextBox, addUrlButton, deleteSelectedUrlButton, deleteAllUrlsButton });
        urlActionsGroupBox.Controls.Add(urlActionsFlowPanel);
        urlTableLayout.Controls.Add(urlActionsGroupBox, 0, 1);
        mainTabControl.TabPages.Add(urlTabPage);
    }

    private void WireUpEventHandlers()
    {
        // Path Management
        chromePathComboBox.SelectedIndexChanged += ChromePathComboBox_SelectedIndexChanged;
        addPathButton.Click += AddPathButton_Click;
        deletePathButton.Click += DeletePathButton_Click;
        discoverProfilesButton.Click += DiscoverProfilesButton_Click;

        // Profile List
        profilesListView.SelectedIndexChanged += ProfilesListView_SelectedIndexChanged;
        profilesListView.DoubleClick += (s, e) => OpenSelectedProfile();

        // Profile Details & Automation
        saveProfileButton.Click += SaveProfileButton_Click;
        loginGoogleButton.Click += LoginGoogleButton_Click;

        // URL Management
        addUrlButton.Click += AddUrlButton_Click;
        deleteSelectedUrlButton.Click += DeleteSelectedUrlButton_Click;
        deleteAllUrlsButton.Click += DeleteAllUrlsButton_Click;
        urlsListBox.DoubleClick += OpenUrlWithSelectedProfile_DoubleClick;

        // Window Management
        minimizeSelectedButton.Click += (s, e) => WindowManager.PerformActionOnProfileWindow(GetSelectedProfileName(), WindowManager.SW_MINIMIZE);
        maximizeSelectedButton.Click += (s, e) => WindowManager.PerformActionOnProfileWindow(GetSelectedProfileName(), WindowManager.SW_MAXIMIZE);
        restoreSelectedButton.Click += (s, e) => WindowManager.PerformActionOnProfileWindow(GetSelectedProfileName(), WindowManager.SW_RESTORE);
        closeSelectedButton.Click += (s, e) => _profileManager.CloseProfileWindow(GetSelectedProfileName());

        // General
        alwaysOnTopCheckBox.CheckedChanged += AlwaysOnTopCheckBox_CheckedChanged;
        openConfigButton.Click += (s, e) => SafeOpenFile("config.json");
        openProfilesJsonButton.Click += (s, e) => SafeOpenFile("profiles.json");
        openUrlJsonButton.Click += (s, e) => SafeOpenFile("URL.json");
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        this.TopMost = _profileManager.IsAlwaysOnTop();
        alwaysOnTopCheckBox.Checked = _profileManager.IsAlwaysOnTop();
        RefreshChromePathList();
        RefreshProfileList();
        RefreshUrlList();
        SetControlStates();
    }

    // --- Data & UI Refresh Methods ---
    private void RefreshChromePathList()
    {
        // Unsubscribe to prevent event firing during refresh
        chromePathComboBox.SelectedIndexChanged -= ChromePathComboBox_SelectedIndexChanged;

        var allPaths = _profileManager.GetAllChromePaths();
        var selectedPath = _profileManager.GetSelectedChromePath();
        chromePathComboBox.DataSource = allPaths;
        chromePathComboBox.SelectedItem = selectedPath;

        // Resubscribe
        chromePathComboBox.SelectedIndexChanged += ChromePathComboBox_SelectedIndexChanged;
    }

    private void RefreshProfileList()
    {
        string selectedProfile = GetSelectedProfileName();
        profilesListView.Items.Clear();
        var profiles = _profileManager.GetProfiles();
        for (int i = 0; i < profiles.Count; i++)
        {
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(profiles[i]);
            profilesListView.Items.Add(item);
            if (profiles[i] == selectedProfile)
            {
                item.Selected = true;
            }
        }
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        if (profilesListView.SelectedItems.Count > 0) profilesListView.Focus();
        SetControlStates();
    }

    private void RefreshUrlList()
    {
        urlsListBox.DataSource = _urlManager.GetUrls();
    }

    // --- Event Handlers ---
    private void ChromePathComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (chromePathComboBox.SelectedItem is string selectedPath)
        {
            _profileManager.SetSelectedChromePath(selectedPath);
            RefreshProfileList(); // Profiles depend on the selected path
        }
    }

    private void AddPathButton_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog())
        {
            ofd.Filter = "Browser Executable|chrome.exe;centbrowser.exe|All files (*.*)|*.*";
            ofd.Title = "Chọn file thực thi của trình duyệt (chrome.exe hoặc centbrowser.exe)";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _profileManager.AddChromePath(ofd.FileName);
                _profileManager.SetSelectedChromePath(ofd.FileName);
                RefreshChromePathList();
            }
        }
    }

    private void DeletePathButton_Click(object sender, EventArgs e)
    {
        if (chromePathComboBox.SelectedItem is string selectedPath)
        {
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa đường dẫn '{selectedPath}'?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                 _profileManager.DeleteChromePath(selectedPath);
                 RefreshChromePathList();
            }
        }
    }
    
    private void DiscoverProfilesButton_Click(object sender, EventArgs e)
    {
        _profileManager.DiscoverAndAddProfiles();
        RefreshProfileList();
    }

    private void ProfilesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        string profileName = GetSelectedProfileName();
        if (profileName != null)
        {
            var profileDetails = _profileManager.GetProfileDetails(profileName);
            emailTextBox.Text = profileDetails?.Email ?? "";
            passwordTextBox.Text = profileDetails?.Password ?? "";
            otpTextBox.Text = profileDetails?.Otp ?? "";
        }
        else
        {
            emailTextBox.Text = "";
            passwordTextBox.Text = "";
            otpTextBox.Text = "";
        }
        SetControlStates();
    }

    private void SaveProfileButton_Click(object sender, EventArgs e)
    {
        string profileName = GetSelectedProfileName();
        if (profileName != null)
        {
            _profileManager.UpdateProfileDetails(profileName, emailTextBox.Text, passwordTextBox.Text, otpTextBox.Text);
            MessageBox.Show("Đã lưu thông tin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OpenSelectedProfile()
    {
        string profileName = GetSelectedProfileName();
        if (profileName != null) _profileManager.OpenChrome(profileName);
    }

    private void AddUrlButton_Click(object sender, EventArgs e)
    {
        string newUrl = newUrlTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(newUrl)) { _urlManager.AddUrl(newUrl); newUrlTextBox.Clear(); RefreshUrlList(); }
    }

    private void DeleteSelectedUrlButton_Click(object sender, EventArgs e)
    {
        if (urlsListBox.SelectedItem is string selectedUrl) { _urlManager.DeleteUrl(selectedUrl); RefreshUrlList(); }
    }

    private void DeleteAllUrlsButton_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc chắn muốn xóa TẤT CẢ các URL?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        { _urlManager.ClearAllUrls(); RefreshUrlList(); }
    }

    private void OpenUrlWithSelectedProfile_DoubleClick(object sender, EventArgs e)
    {
        string profileName = GetSelectedProfileName();
        if (profileName == null) { MessageBox.Show("Vui lòng chọn một Profile.", "Chưa chọn Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (urlsListBox.SelectedItem is string url) _profileManager.OpenChrome(profileName, url);
    }

    private async void LoginGoogleButton_Click(object sender, EventArgs e)
    {
        string profileName = GetSelectedProfileName();
        if (profileName == null) return;
        var profileDetails = _profileManager.GetProfileDetails(profileName);
        if (profileDetails == null || string.IsNullOrWhiteSpace(profileDetails.Email) || string.IsNullOrWhiteSpace(profileDetails.Password))
        { MessageBox.Show("Vui lòng lưu Email và Mật khẩu cho profile này.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            this.Enabled = false;
            await _automationManager.LoginToGoogleAsync(profileName, _profileManager.GetEffectiveUserDataPath(), Path.GetDirectoryName(Application.ExecutablePath), profileDetails.Email, profileDetails.Password);
            MessageBox.Show($"Quá trình tự động đăng nhập cho '{profileName}' đã hoàn tất.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { this.Enabled = true; }
    }

    private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        this.TopMost = alwaysOnTopCheckBox.Checked;
        _profileManager.SetAlwaysOnTop(alwaysOnTopCheckBox.Checked);
    }

    // --- Helper Methods ---
    private void SetControlStates()
    {
        bool isProfileSelected = GetSelectedProfileName() != null;
        saveProfileButton.Enabled = isProfileSelected;
        loginGoogleButton.Enabled = isProfileSelected;
        minimizeSelectedButton.Enabled = isProfileSelected;
        maximizeSelectedButton.Enabled = isProfileSelected;
        restoreSelectedButton.Enabled = isProfileSelected;
        closeSelectedButton.Enabled = isProfileSelected;
    }

    private string GetSelectedProfileName()
    {
        return profilesListView.SelectedItems.Count > 0 ? profilesListView.SelectedItems[0].SubItems[1].Text : null;
    }

    private void SafeOpenFile(string fileName)
    {
        try
        {
            if (!File.Exists(fileName) && fileName.EndsWith(".json"))
                File.WriteAllText(fileName, fileName.Contains("config") ? "{}" : "[]");
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show($"Không thể mở file '{fileName}'. Lỗi: {ex.Message}", "Lỗi Mở File", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
