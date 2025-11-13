
using System;
using System.Collections.Generic;
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
    private AutomationManager _automationManager; 
    private System.Windows.Forms.Timer statusUpdateTimer;

    // --- UI Controls ---
    private ComboBox chromePathComboBox;
    private ListBox profilesListBox, urlsListBox;
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox, currentProfileTextBox, closingProfileTextBox;
    private Button saveProfileButton, addUrlButton, deleteSelectedUrlButton, openUrlWithSelectedProfileButton, deleteAllUrlsButton;
    private Button arrangeButton, maximizeAllButton, minimizeAllButton, restoreAllButton, closeAllButton, switchTabButton;
    private Button openSelectedProfileButton, closeSelectedProfileButton, loginGoogleButton;
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

    private void WireUpEventHandlers()
    {
        // Profile Tab
        profilesListBox.SelectedIndexChanged += ProfilesListBox_SelectedIndexChanged;
        saveProfileButton.Click += SaveProfileButton_Click;
        openSelectedProfileButton.Click += OpenSelectedProfileButton_Click;
        closeSelectedProfileButton.Click += CloseSelectedProfileButton_Click;

        // URL Tab
        addUrlButton.Click += AddUrlButton_Click;
        deleteSelectedUrlButton.Click += DeleteSelectedUrlButton_Click;
        deleteAllUrlsButton.Click += DeleteAllUrlsButton_Click;
        openUrlWithSelectedProfileButton.Click += OpenUrlWithSelectedProfileButton_Click;
        urlsListBox.DoubleClick += OpenUrlWithSelectedProfileButton_Click;

        // Automation Tab
        loginGoogleButton.Click += LoginGoogleButton_Click;

        // Window Management Tab
        arrangeButton.Click += (s, e) => WindowManager.ArrangeChromeWindows(2, 10, _profileManager.GetSelectedChromePath());
        maximizeAllButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 3); // 3 = SW_MAXIMIZE
        minimizeAllButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 6); // 6 = SW_MINIMIZE
        restoreAllButton.Click += (s, e) => WindowManager.PerformGlobalAction(_profileManager.GetSelectedChromePath(), 9); // 9 = SW_RESTORE
        switchTabButton.Click += (s, e) => WindowManager.CycleToNextChromeWindow(_profileManager.GetSelectedChromePath());
        closeAllButton.Click += (s, e) => {
            if (MessageBox.Show("Bạn có chắc chắn muốn đóng TẤT CẢ các cửa sổ trình duyệt không?", "Xác nhận đóng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _profileManager.CloseAllChrome();
            }
        };

        // Top Bar
        alwaysOnTopCheckBox.CheckedChanged += AlwaysOnTopCheckBox_CheckedChanged;

        // Timer for status updates
        statusUpdateTimer = new System.Windows.Forms.Timer();
        statusUpdateTimer.Interval = 1000; // 1 second
        statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
        statusUpdateTimer.Start();
    }

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
        // This method is now focused purely on layout
        this.Text = "Pro5Chrome Manager by hieuck";
        this.Size = new Size(1150, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        Padding controlMargin = new Padding(5);

        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var pathFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        var openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = controlMargin };
        openConfigButton.Click += (s, e) => SafeOpenFile("config.json");
        var openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = controlMargin };
        openProfilesJsonButton.Click += (s, e) => SafeOpenFile("profiles.json");
        var openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = controlMargin };
        openUrlJsonButton.Click += (s, e) => SafeOpenFile("URL.json");
        alwaysOnTopCheckBox = new CheckBox { Text = "Luôn trên cùng", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        topFlowPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox });

        var pathLabel = new Label { Text = "Đường dẫn trình duyệt:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        chromePathComboBox = new ComboBox { Width = 450, DropDownStyle = ComboBoxStyle.DropDown, Margin = new Padding(5, 5, 0, 0) };
        var openUserDataButton = new Button { Text = "Mở User Data", AutoSize = true, Margin = controlMargin };
        var deletePathButton = new Button { Text = "Xóa", AutoSize = true, Margin = controlMargin };
        var discoverProfilesButton = new Button { Text = "Đọc Profiles", AutoSize = true, Margin = controlMargin };
        pathFlowPanel.Controls.AddRange(new Control[] { pathLabel, chromePathComboBox, openUserDataButton, deletePathButton, discoverProfilesButton });

        var mainSplitContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 350, BorderStyle = BorderStyle.Fixed3D };
        
        var profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0,0,0,5) };
        profilesListBox = new ListBox { Dock = DockStyle.Fill };
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListBox, profileCountLabel });
        mainSplitContainer.Panel1.Controls.Add(profilesGroupBox);
        mainSplitContainer.Panel1.Padding = new Padding(5);

        var mainTabControl = new TabControl { Dock = DockStyle.Fill };
        mainSplitContainer.Panel2.Controls.Add(mainTabControl);
        mainSplitContainer.Panel2.Padding = new Padding(5);

        var detailsTabPage = new TabPage("Chi tiết & Tác vụ");
        var detailsTablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        detailsTablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
        detailsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsTabPage.Controls.Add(detailsTablePanel);

        var profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Thông tin Profile", Padding = new Padding(10) };
        var emailLabel = new Label { Text = "Email:", Location = new Point(15, 30), AutoSize = true };
        emailTextBox = new TextBox { Location = new Point(100, 27), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450 };
        var passwordLabel = new Label { Text = "Password:", Location = new Point(15, 60), AutoSize = true };
        passwordTextBox = new TextBox { Location = new Point(100, 57), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450, UseSystemPasswordChar = true };
        var otpLabel = new Label { Text = "OTP Secret:", Location = new Point(15, 90), AutoSize = true };
        otpTextBox = new TextBox { Location = new Point(100, 87), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450 };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", Location = new Point(98, 125), Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true, Enabled = false };
        profileDetailsGroupBox.Controls.AddRange(new Control[] { emailLabel, emailTextBox, passwordLabel, passwordTextBox, otpLabel, otpTextBox, saveProfileButton });
        
        var singleProfileActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Tác vụ cho Profile đã chọn", Padding = new Padding(10) };
        var actionsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        openSelectedProfileButton = new Button { Text = "Mở Profile", AutoSize = true, Margin = controlMargin };
        closeSelectedProfileButton = new Button { Text = "Đóng Profile", AutoSize = true, Margin = controlMargin };
        actionsFlowPanel.Controls.AddRange(new Control[] { openSelectedProfileButton, closeSelectedProfileButton });
        singleProfileActionsGroupBox.Controls.Add(actionsFlowPanel);

        detailsTablePanel.Controls.Add(profileDetailsGroupBox, 0, 0);
        detailsTablePanel.Controls.Add(singleProfileActionsGroupBox, 0, 1);
        mainTabControl.TabPages.Add(detailsTabPage);

        var urlTabPage = new TabPage("Quản lý URL");
        var urlSplitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300, BorderStyle = BorderStyle.Fixed3D };
        urlTabPage.Controls.Add(urlSplitContainer);
        var urlsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách URLs", Padding = new Padding(10) };
        urlsListBox = new ListBox { Dock = DockStyle.Fill };
        urlsGroupBox.Controls.Add(urlsListBox);
        urlSplitContainer.Panel1.Controls.Add(urlsGroupBox);
        var urlActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Hành động", Padding = new Padding(10) };
        var urlActionsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        newUrlTextBox = new TextBox { Width = 350, Margin = controlMargin };
        addUrlButton = new Button { Text = "Thêm URL", AutoSize = true, Margin = controlMargin };
        deleteSelectedUrlButton = new Button { Text = "Xóa URL đã chọn", AutoSize = true, Margin = controlMargin };
        deleteAllUrlsButton = new Button { Text = "Xóa tất cả URL", AutoSize = true, Margin = controlMargin, ForeColor = Color.Red };
        openUrlWithSelectedProfileButton = new Button { Text = "Mở URL bằng Profile đã chọn", AutoSize = true, Margin = new Padding(10, 2, 2, 2) };
        urlActionsFlowPanel.Controls.AddRange(new Control[] { newUrlTextBox, addUrlButton, deleteSelectedUrlButton, deleteAllUrlsButton });
        var openUrlFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), Height = 40 };
        openUrlFlowPanel.Controls.Add(openUrlWithSelectedProfileButton);
        urlActionsGroupBox.Controls.Add(urlActionsFlowPanel);
        urlActionsGroupBox.Controls.Add(openUrlFlowPanel);
        urlSplitContainer.Panel2.Controls.Add(urlActionsGroupBox);
        mainTabControl.TabPages.Add(urlTabPage);

        var automationTabPage = new TabPage("Tự động hóa");
        var automationGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Kịch bản Tự động hóa", Padding = new Padding(20) };
        loginGoogleButton = new Button { Text = "Tự động Đăng nhập Google (với Profile đã chọn)", AutoSize = true, Height = 30, Padding = controlMargin };
        automationGroupBox.Controls.Add(loginGoogleButton);
        automationTabPage.Controls.Add(automationGroupBox);
        mainTabControl.TabPages.Add(automationTabPage);

        var windowTabPage = new TabPage("Quản lý Cửa sổ");
        var windowTablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        windowTablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        windowTablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        windowTabPage.Controls.Add(windowTablePanel);
        var statusGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Trạng thái Cửa sổ", Padding = new Padding(10) };
        currentProfileTextBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true, Margin = new Padding(0, 0, 0, 5), PlaceholderText = "Cửa sổ Active..." };
        closingProfileTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Các cửa sổ khác..." };
        statusGroupBox.Controls.AddRange(new Control[] { closingProfileTextBox, currentProfileTextBox });
        var windowActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Hành động Chung", Padding = new Padding(10) };
        var windowActionsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        arrangeButton = new Button { Text = "Sắp xếp", AutoSize = true, Margin = controlMargin };
        maximizeAllButton = new Button { Text = "Phóng to Tất cả", AutoSize = true, Margin = controlMargin };
        minimizeAllButton = new Button { Text = "Thu nhỏ Tất cả", AutoSize = true, Margin = controlMargin };
        restoreAllButton = new Button { Text = "Khôi phục Tất cả", AutoSize = true, Margin = controlMargin };
        switchTabButton = new Button { Text = "Chuyển cửa sổ", AutoSize = true, Margin = controlMargin };
        closeAllButton = new Button { Text = "Đóng Tất cả", AutoSize = true, Margin = controlMargin, ForeColor = Color.Red };
        windowActionsFlowPanel.Controls.AddRange(new Control[] { arrangeButton, maximizeAllButton, minimizeAllButton, restoreAllButton, switchTabButton, closeAllButton });
        windowActionsGroupBox.Controls.Add(windowActionsFlowPanel);
        windowTablePanel.Controls.Add(statusGroupBox, 0, 0);
        windowTablePanel.Controls.Add(windowActionsGroupBox, 0, 1);
        mainTabControl.TabPages.Add(windowTabPage);

        this.Controls.AddRange(new Control[] { mainSplitContainer, pathFlowPanel, topFlowPanel });
        this.ResumeLayout(false);
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        this.TopMost = _profileManager.IsAlwaysOnTop();
        alwaysOnTopCheckBox.Checked = _profileManager.IsAlwaysOnTop();

        RefreshProfileLists();
        RefreshUrlList();
        RefreshChromePathList();
    }

    private void RefreshProfileLists()
    {
        string selectedProfile = profilesListBox.SelectedItem as string;
        var profiles = _profileManager.GetProfiles();
        profilesListBox.DataSource = profiles;
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        if (selectedProfile != null && profiles.Contains(selectedProfile))
        {
            profilesListBox.SelectedItem = selectedProfile;
        }
    }

    private void RefreshUrlList()
    {
        string selectedUrl = urlsListBox.SelectedItem as string;
        var urls = _urlManager.GetUrls();
        urlsListBox.DataSource = null;
        urlsListBox.DataSource = urls;
        if (selectedUrl != null && urls.Contains(selectedUrl))
        {
            urlsListBox.SelectedItem = selectedUrl;
        }
    }
    
    private void RefreshChromePathList() 
    {
        // Logic will be added later
    }

    private void ProfilesListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isProfileSelected = profilesListBox.SelectedItem is string;
        saveProfileButton.Enabled = isProfileSelected;
        openSelectedProfileButton.Enabled = isProfileSelected;
        closeSelectedProfileButton.Enabled = isProfileSelected;
        
        if (isProfileSelected)
        {
            string profileName = profilesListBox.SelectedItem.ToString();
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
    }

    private void SaveProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem is string profileName)
        {
            _profileManager.UpdateProfileDetails(profileName, emailTextBox.Text, passwordTextBox.Text, otpTextBox.Text);
            MessageBox.Show("Đã lưu thông tin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OpenSelectedProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem is string profileName)
        {
            _profileManager.OpenChrome(profileName);
        }
    }

    private void CloseSelectedProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem is string profileName)
        {
            _profileManager.CloseProfileWindow(profileName);
        }
    }

    private void AddUrlButton_Click(object sender, EventArgs e)
    {
        string newUrl = newUrlTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(newUrl))
        {
            _urlManager.AddUrl(newUrl);
            newUrlTextBox.Clear();
            RefreshUrlList();
        }
    }

    private void DeleteSelectedUrlButton_Click(object sender, EventArgs e)
    {
        if (urlsListBox.SelectedItem is string selectedUrl)
        {
            _urlManager.DeleteUrl(selectedUrl);
            RefreshUrlList();
        }
        else
        {
            MessageBox.Show("Vui lòng chọn một URL để xóa.", "Chưa chọn URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DeleteAllUrlsButton_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc chắn muốn xóa TẤT CẢ các URL đã lưu không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _urlManager.ClearAllUrls();
            RefreshUrlList();
        }
    }

    private void OpenUrlWithSelectedProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn một Profile từ danh sách bên trái.", "Chưa chọn Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (urlsListBox.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn một URL từ danh sách.", "Chưa chọn URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string profileName = profilesListBox.SelectedItem.ToString();
        string url = urlsListBox.SelectedItem.ToString();
        _profileManager.OpenChrome(profileName, url);
    }
    
    private async void LoginGoogleButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn một profile từ danh sách bên trái.", "Chưa chọn Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        string profileName = profilesListBox.SelectedItem.ToString();
        var profileDetails = _profileManager.GetProfileDetails(profileName);

        if (profileDetails == null || string.IsNullOrWhiteSpace(profileDetails.Email) || string.IsNullOrWhiteSpace(profileDetails.Password))
        {
            MessageBox.Show("Vui lòng lưu Email và Mật khẩu cho profile này trong tab 'Chi tiết & Tác vụ' trước khi tự động đăng nhập.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string userDataPath = _profileManager.GetEffectiveUserDataPath();
        string chromeDriverPath = Path.GetDirectoryName(Application.ExecutablePath);

        try
        {
            this.Enabled = false; // Disable form during automation
            await _automationManager.LoginToGoogleAsync(profileName, userDataPath, chromeDriverPath, profileDetails.Email, profileDetails.Password);
            MessageBox.Show($"Quá trình tự động đăng nhập cho profile '{profileName}' đã hoàn tất.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        { 
            MessageBox.Show($"Đã xảy ra lỗi trong quá trình tự động hóa:\n{ex.Message}", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Enabled = true; // Re-enable form
        }
    }

    private void StatusUpdateTimer_Tick(object sender, EventArgs e)
    {
        var (activeTitle, inactiveTitles) = WindowManager.GetChromeWindowStates(_profileManager.GetSelectedChromePath());
        currentProfileTextBox.Text = activeTitle;
        closingProfileTextBox.Text = string.Join(Environment.NewLine, inactiveTitles);
    }

    private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        _profileManager.SetAlwaysOnTop(alwaysOnTopCheckBox.Checked);
        this.TopMost = alwaysOnTopCheckBox.Checked;
    }
}
