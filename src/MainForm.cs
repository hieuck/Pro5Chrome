
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MainForm : Form
{
    private Pro5ChromeManager _manager;
    private UrlManager _urlManager;

    // --- UI Controls ---
    private ComboBox chromePathComboBox;
    private ListView profilesListView;
    private ListBox urlsListBox;
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox, logTextBox;
    private Button saveProfileButton, addUrlButton, deleteSelectedUrlButton, deleteAllUrlsButton, loginGoogleButton, btnWarmUp;
    private Button minimizeSelectedButton, maximizeSelectedButton, restoreSelectedButton, closeSelectedButton, switchToNextTabButton, arrangeCascadeButton;
    private Button addPathButton, deletePathButton, discoverProfilesButton;
    private Button openConfigButton, openProfilesJsonButton, openUrlJsonButton;
    private CheckBox alwaysOnTopCheckBox, hideProfileNamesCheckBox;
    private Label profileCountLabel;
    private System.Windows.Forms.Timer statusUpdateTimer;
    private StatusStrip statusBar;
    private ToolStripStatusLabel activeTabPanel;


    public MainForm()
    {
        _manager = new Pro5ChromeManager(Log);
        _urlManager = new UrlManager();
        InitializeComponent();
        this.Load += MainForm_Load;
        WireUpEventHandlers();
    }

    private void Log(string message)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => Log(message)));
            return;
        }
        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
    }

    private void InitializeComponent()
    {
        this.Text = "Pro5Chrome Manager by hieuck";
        this.Size = new Size(1350, 900);
        this.MinimumSize = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        var mainTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Top buttons
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Path bar
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Main content
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Log box
        this.Controls.Add(mainTableLayout);

        var topFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var pathFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        mainTableLayout.Controls.AddRange(new Control[] { topFlowPanel, pathFlowPanel });

        openConfigButton = new Button { Text = "Mở config.json", AutoSize = true, Margin = new Padding(5) };
        openProfilesJsonButton = new Button { Text = "Mở profiles.json", AutoSize = true, Margin = new Padding(5) };
        openUrlJsonButton = new Button { Text = "Mở URL.json", AutoSize = true, Margin = new Padding(5) };
        alwaysOnTopCheckBox = new CheckBox { Text = "Luôn trên cùng", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        hideProfileNamesCheckBox = new CheckBox { Text = "Ẩn tên Profile", AutoSize = true, Margin = new Padding(15, 8, 5, 5) };
        topFlowPanel.Controls.AddRange(new Control[] { openConfigButton, openProfilesJsonButton, openUrlJsonButton, alwaysOnTopCheckBox, hideProfileNamesCheckBox });

        var pathLabel = new Label { Text = "Đường dẫn trình duyệt:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        chromePathComboBox = new ComboBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5, 5, 0, 0) };
        addPathButton = new Button { Text = "Thêm...", AutoSize = true, Margin = new Padding(5) };
        deletePathButton = new Button { Text = "Xóa", AutoSize = true, Margin = new Padding(5) };
        discoverProfilesButton = new Button { Text = "Quét Profiles", AutoSize = true, Margin = new Padding(5) };
        pathFlowPanel.Controls.AddRange(new Control[] { pathLabel, chromePathComboBox, addPathButton, deletePathButton, discoverProfilesButton });

        var contentTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(5) };
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Wider profile list
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        mainTableLayout.Controls.Add(contentTableLayout, 0, 2);

        InitializeProfilesList(contentTableLayout, 0, 0);
        InitializeMiddleColumn(contentTableLayout, 1, 0);
        InitializeUrlManagement(contentTableLayout, 2, 0);
        InitializeLogArea(mainTableLayout, 0, 3);
        InitializeStatusBar();

        this.ResumeLayout(false);
    }

    private void InitializeProfilesList(TableLayoutPanel parent, int column, int row)
    {
        var profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        parent.Controls.Add(profilesGroupBox, column, row);

        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0, 0, 0, 5) };
        profilesListView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = true, BorderStyle = BorderStyle.None };
        profilesListView.Columns.Add("#", 45, HorizontalAlignment.Left);
        profilesListView.Columns.Add("Tên Profile", -2, HorizontalAlignment.Left);
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListView, profileCountLabel });
    }

    private void InitializeMiddleColumn(TableLayoutPanel parent, int column, int row)
    {
        var middleColumnLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        middleColumnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Details
        middleColumnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // Window Actions
        parent.Controls.Add(middleColumnLayout, column, row);

        var profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Chi tiết & Tự động hóa", Padding = new Padding(10) };
        middleColumnLayout.Controls.Add(profileDetailsGroupBox, 0, 0);

        var detailsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileDetailsGroupBox.Controls.Add(detailsLayout);

        emailTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };
        passwordTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5), UseSystemPasswordChar = true };
        otpTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5), UseSystemPasswordChar = true };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", Anchor = AnchorStyles.None, AutoSize = true, Margin = new Padding(5) };
        loginGoogleButton = new Button { Text = "Tự động Đăng nhập/Kháng nghị", Dock = DockStyle.Fill, Height = 35, Margin = new Padding(5) };
        btnWarmUp = new Button { Text = "Nuôi tài khoản", Dock = DockStyle.Fill, Height = 30, Margin = new Padding(5) }; // Resized button

        detailsLayout.Controls.Add(new Label { Text = "Email:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 0);
        detailsLayout.Controls.Add(emailTextBox, 1, 0);
        detailsLayout.Controls.Add(new Label { Text = "Password:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 1);
        detailsLayout.Controls.Add(passwordTextBox, 1, 1);
        detailsLayout.Controls.Add(new Label { Text = "OTP Secret:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 2);
        detailsLayout.Controls.Add(otpTextBox, 1, 2);
        detailsLayout.Controls.Add(saveProfileButton, 1, 3);
        detailsLayout.SetColumnSpan(loginGoogleButton, 2);
        detailsLayout.Controls.Add(loginGoogleButton, 0, 4);
        detailsLayout.SetColumnSpan(btnWarmUp, 2);
        detailsLayout.Controls.Add(btnWarmUp, 0, 5);

        var windowActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Quản lý Cửa sổ", Padding = new Padding(8) };
        middleColumnLayout.Controls.Add(windowActionsGroupBox, 0, 1);

        var windowActionsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 }; // Changed to 3 rows
        windowActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        windowActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        windowActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        windowActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        windowActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        windowActionsGroupBox.Controls.Add(windowActionsLayout);

        minimizeSelectedButton = new Button { Text = "Thu nhỏ", Dock = DockStyle.Fill, Margin = new Padding(3) };
        maximizeSelectedButton = new Button { Text = "Phóng to", Dock = DockStyle.Fill, Margin = new Padding(3) };
        restoreSelectedButton = new Button { Text = "Khôi phục", Dock = DockStyle.Fill, Margin = new Padding(3) };
        closeSelectedButton = new Button { Text = "Đóng", Dock = DockStyle.Fill, Margin = new Padding(3), ForeColor = Color.Red };
        switchToNextTabButton = new Button { Text = "Chuyển Tab", Dock = DockStyle.Fill, Margin = new Padding(3) }; // New button
        arrangeCascadeButton = new Button { Text = "Sắp xếp (Cascade)", Dock = DockStyle.Fill, Margin = new Padding(3) }; // New button
        
        windowActionsLayout.Controls.Add(minimizeSelectedButton, 0, 0);
        windowActionsLayout.Controls.Add(maximizeSelectedButton, 1, 0);
        windowActionsLayout.Controls.Add(restoreSelectedButton, 0, 1);
        windowActionsLayout.Controls.Add(switchToNextTabButton, 1, 1);
        windowActionsLayout.Controls.Add(arrangeCascadeButton, 0, 2);
        windowActionsLayout.Controls.Add(closeSelectedButton, 1, 2);
    }

    private void InitializeUrlManagement(TableLayoutPanel parent, int column, int row)
    {
        var urlGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Quản lý URL", Padding = new Padding(10) };
        parent.Controls.Add(urlGroupBox, column, row);

        var urlMainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        urlMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        urlMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        urlGroupBox.Controls.Add(urlMainLayout);

        urlsListBox = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, IntegralHeight = false };
        var listWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BorderStyle = BorderStyle.FixedSingle };
        listWrapper.Controls.Add(urlsListBox);
        urlMainLayout.Controls.Add(listWrapper, 0, 0);

        var urlActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Hành động", Padding = new Padding(8) };
        urlMainLayout.Controls.Add(urlActionsGroupBox, 0, 1);

        var urlActionsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        urlActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        urlActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        urlActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        urlActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        urlActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        urlActionsGroupBox.Controls.Add(urlActionsLayout);

        newUrlTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
        addUrlButton = new Button { Text = "Thêm", Dock = DockStyle.Fill, Margin = new Padding(3) };
        deleteSelectedUrlButton = new Button { Text = "Xóa", Dock = DockStyle.Fill, Margin = new Padding(3) };
        deleteAllUrlsButton = new Button { Text = "Xóa hết", Dock = DockStyle.Fill, Margin = new Padding(3), ForeColor = Color.Red };

        urlActionsLayout.SetColumnSpan(newUrlTextBox, 3);
        urlActionsLayout.Controls.Add(newUrlTextBox, 0, 0);
        urlActionsLayout.Controls.Add(addUrlButton, 0, 1);
        urlActionsLayout.Controls.Add(deleteSelectedUrlButton, 1, 1);
        urlActionsLayout.Controls.Add(deleteAllUrlsButton, 2, 1);
    }

    private void InitializeLogArea(TableLayoutPanel parent, int column, int row)
    {
        var logGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Logs", Padding = new Padding(10) };
        parent.Controls.Add(logGroupBox, column, row);
        logTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Font = new Font("Consolas", 8.25F) };
        logGroupBox.Controls.Add(logTextBox);
    }

    private void InitializeStatusBar()
    {
        statusBar = new StatusStrip();
        activeTabPanel = new ToolStripStatusLabel { Text = "Tab đang hoạt động: N/A", Spring = true };
        statusBar.Items.Add(activeTabPanel);
        this.Controls.Add(statusBar);
        statusUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
    }

    private void WireUpEventHandlers()
    {
        openConfigButton.Click += (s, e) => SafeOpenFile("config.json");
        openProfilesJsonButton.Click += (s, e) => SafeOpenFile("profiles.json");
        openUrlJsonButton.Click += (s, e) => SafeOpenFile("urls.json");

        addPathButton.Click += AddPathButton_Click;
        deletePathButton.Click += DeletePathButton_Click;
        discoverProfilesButton.Click += DiscoverProfilesButton_Click;
        chromePathComboBox.SelectedIndexChanged += ChromePathComboBox_SelectedIndexChanged;

        alwaysOnTopCheckBox.CheckedChanged += (s, e) => { this.TopMost = alwaysOnTopCheckBox.Checked; };
        hideProfileNamesCheckBox.CheckedChanged += (s, e) => _manager.SetHideProfileNames(hideProfileNamesCheckBox.Checked);

        profilesListView.ItemSelectionChanged += ProfilesListView_ItemSelectionChanged;
        profilesListView.MouseDoubleClick += ProfilesListView_MouseDoubleClick;

        saveProfileButton.Click += SaveProfileButton_Click;
        loginGoogleButton.Click += LoginGoogleButton_Click;
        btnWarmUp.Click += BtnWarmUp_Click;

        addUrlButton.Click += AddUrlButton_Click;
        deleteSelectedUrlButton.Click += DeleteSelectedUrlButton_Click;
        deleteAllUrlsButton.Click += DeleteAllUrlsButton_Click;
        urlsListBox.MouseDoubleClick += UrlsListBox_MouseDoubleClick;

        minimizeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.MinimizeWindow(p));
        maximizeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.MaximizeWindow(p));
        restoreSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.RestoreWindow(p));
        closeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => _manager.CloseChromeProfile(p));
        switchToNextTabButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.SwitchToNextTab(p));
        arrangeCascadeButton.Click += (s, e) => WindowManager.ArrangeWindows(true);

        statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
        this.FormClosing += (s, e) => statusUpdateTimer.Stop();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        _manager.LoadConfig();
        _manager.LoadProfiles();
        _urlManager.LoadUrls();
        RefreshChromePathComboBox();
        RefreshProfileListView();
        RefreshUrlsListBox();
        SetControlStates();
        statusUpdateTimer.Start();
    }

    // ... Refresh methods
    private void RefreshProfileListView()
    {
        profilesListView.Items.Clear();
        var profiles = _manager.GetProfiles();
        for (int i = 0; i < profiles.Count; i++)
        {
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(profiles[i].Name);
            item.Tag = profiles[i].Name;
            profilesListView.Items.Add(item);
        }
        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
    }

    private void RefreshUrlsListBox()
    {
        urlsListBox.Items.Clear();
        urlsListBox.Items.AddRange(_urlManager.GetUrls().ToArray());
    }

    private void RefreshChromePathComboBox()
    {
        chromePathComboBox.Items.Clear();
        chromePathComboBox.Items.AddRange(_manager.GetChromePaths().ToArray());
        if (_manager.GetConfig().SelectedIndex < chromePathComboBox.Items.Count)
        {
            chromePathComboBox.SelectedIndex = _manager.GetConfig().SelectedIndex;
        }
    }


    private void StatusUpdateTimer_Tick(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (profileName != null)
        {
            string tabTitle = WindowManager.GetActiveTabTitle(profileName);
            activeTabPanel.Text = $"Tab đang hoạt động: {tabTitle}";
        }
        else
        {
            activeTabPanel.Text = "Tab đang hoạt động: N/A";
        }
    }
    
    // Event Handlers

    private void ChromePathComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (chromePathComboBox.SelectedIndex != -1)
        {
            _manager.SetSelectedChromePath(chromePathComboBox.SelectedIndex);
        }
    }

    private void AddPathButton_Click(object sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog() { Description = "Chọn thư mục chứa file thực thi của trình duyệt (chrome.exe, msedge.exe,...)" })
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _manager.AddChromePath(fbd.SelectedPath);
                RefreshChromePathComboBox();
            }
        }
    }

    private void DeletePathButton_Click(object sender, EventArgs e)
    {
        if (chromePathComboBox.SelectedIndex != -1)
        {
            _manager.RemoveChromePath(chromePathComboBox.SelectedIndex);
            RefreshChromePathComboBox();
        }
        else
        {
            MessageBox.Show("Vui lòng chọn một đường dẫn để xóa.", "Chưa chọn đường dẫn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void DiscoverProfilesButton_Click(object sender, EventArgs e)
    {
        _manager.DiscoverProfiles();
        RefreshProfileListView();
    }

    private void ProfilesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        SetControlStates();
        if (e.IsSelected)
        {
            var profile = _manager.GetProfile(e.Item.Tag.ToString());
            if (profile != null)
            {
                emailTextBox.Text = profile.Email;
                passwordTextBox.Text = profile.Password;
                otpTextBox.Text = profile.OtpSecret;
            }
        }
        else
        {
            emailTextBox.Clear();
            passwordTextBox.Clear();
            otpTextBox.Clear();
        }
    }

    private void ProfilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (profilesListView.SelectedItems.Count > 0)
        {
            string profileName = profilesListView.SelectedItems[0].Tag.ToString();
            _manager.OpenChromeProfile(profileName, _urlManager.GetUrls());
        }
    }

    private void SaveProfileButton_Click(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (profileName != null)
        {
            _manager.UpdateProfileDetails(profileName, emailTextBox.Text, passwordTextBox.Text, otpTextBox.Text);
            Log($"Đã lưu thông tin cho profile: {profileName}");
        }
    }

    private async void LoginGoogleButton_Click(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (string.IsNullOrEmpty(profileName)) return;

        var profile = _manager.GetProfile(profileName);
        if (string.IsNullOrEmpty(profile.Email) || string.IsNullOrEmpty(profile.Password))
        {
            MessageBox.Show("Vui lòng nhập Email và Password cho profile này trước.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Log($"Bắt đầu đăng nhập Google cho profile: {profileName}");
        SetAutomationButtonsEnabled(false);
        this.Cursor = Cursors.WaitCursor;

        try
        {
            await Task.Run(() => _manager.LoginGoogle(profileName));
        }
        catch (Exception ex)
        {
            Log($"LỖI khi đăng nhập Google cho {profileName}: {ex.Message}");
            MessageBox.Show(ex.Message, "Lỗi Tự Động Hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetAutomationButtonsEnabled(true);
            this.Cursor = Cursors.Default;
        }
    }

    private async void BtnWarmUp_Click(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (string.IsNullOrEmpty(profileName))
        {
            MessageBox.Show("Vui lòng chọn một profile để nuôi.", "Chưa chọn profile");
            return;
        }

        Log($"Bắt đầu quá trình nuôi tài khoản cho profile: {profileName}");
        SetAutomationButtonsEnabled(false);
        this.Cursor = Cursors.WaitCursor;

        try
        {
            await Task.Run(() => _manager.WarmUpAccount(profileName));
            Log($"Hoàn tất quá trình nuôi tài khoản cho profile: {profileName}");
        }
        catch(Exception ex)
        {
            Log($"LỖI khi nuôi tài khoản {profileName}: {ex.Message}");
        }
        finally
        {
            SetAutomationButtonsEnabled(true);
            this.Cursor = Cursors.Default;
        }
    }

    private void AddUrlButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(newUrlTextBox.Text))
        {
            _urlManager.AddUrl(newUrlTextBox.Text.Trim());
            newUrlTextBox.Clear();
            RefreshUrlsListBox();
        }
    }

    private void DeleteSelectedUrlButton_Click(object sender, EventArgs e)
    {
        if (urlsListBox.SelectedItem != null)
        {
            _urlManager.RemoveUrl(urlsListBox.SelectedItem.ToString());
            RefreshUrlsListBox();
        }
    }

    private void DeleteAllUrlsButton_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc muốn xóa tất cả các URL?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _urlManager.ClearUrls();
            RefreshUrlsListBox();
        }
    }

    private void UrlsListBox_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (urlsListBox.SelectedItem != null)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(urlsListBox.SelectedItem.ToString()) { UseShellExecute = true });
        }
    }


    private void SetControlStates()
    {
        bool isProfileSelected = GetFirstSelectedProfile() != null;
        bool areAnyProfilesSelected = GetSelectedProfileNames().Length > 0;

        saveProfileButton.Enabled = isProfileSelected;
        loginGoogleButton.Enabled = isProfileSelected;
        btnWarmUp.Enabled = isProfileSelected;

        minimizeSelectedButton.Enabled = areAnyProfilesSelected;
        maximizeSelectedButton.Enabled = areAnyProfilesSelected;
        restoreSelectedButton.Enabled = areAnyProfilesSelected;
        closeSelectedButton.Enabled = areAnyProfilesSelected;
        switchToNextTabButton.Enabled = areAnyProfilesSelected;
    }
    
    private void SetAutomationButtonsEnabled(bool enabled)
    {
        loginGoogleButton.Enabled = enabled;
        btnWarmUp.Enabled = enabled;
    }

    // Helper methods
    private string[] GetSelectedProfileNames()
    {
        if (profilesListView.SelectedItems.Count == 0) return new string[0];
        return profilesListView.SelectedItems.Cast<ListViewItem>().Select(item => item.Tag.ToString()).ToArray();
    }

    private string GetFirstSelectedProfile()
    {
        if (profilesListView.SelectedItems.Count == 0) return null;
        return profilesListView.SelectedItems[0].Tag.ToString();
    }

    private void ForEachSelectedProfile(Action<string> action)
    {
        if (profilesListView.SelectedItems.Count == 0) return;
        var profileNames = GetSelectedProfileNames();
        foreach (var name in profileNames)
        {
            action(name);
        }
    }

    private void SafeOpenFile(string fileName)
    {
        try
        {
            if (!File.Exists(fileName) && fileName.EndsWith(".json"))
                File.WriteAllText(fileName, fileName.Contains("config") ? "{}" : "[]");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show($"Không thể mở file '{fileName}'. Lỗi: {ex.Message}", "Lỗi Mở File", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

}
