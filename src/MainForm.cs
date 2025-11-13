
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
    private TextBox emailTextBox, passwordTextBox, otpTextBox, newUrlTextBox;
    private Button saveProfileButton, addUrlButton, deleteSelectedUrlButton, deleteAllUrlsButton, loginGoogleButton;
    private Button minimizeSelectedButton, maximizeSelectedButton, restoreSelectedButton, closeSelectedButton;
    private Button addPathButton, deletePathButton, discoverProfilesButton;
    private Button openConfigButton, openProfilesJsonButton, openUrlJsonButton;
    private CheckBox alwaysOnTopCheckBox, hideProfileNamesCheckBox;
    private Label profileCountLabel;
    private ContextMenuStrip profileContextMenuStrip;

    public MainForm()
    {
        _manager = new Pro5ChromeManager();
        _urlManager = new UrlManager();
        InitializeComponent();
        this.Load += MainForm_Load;
        WireUpEventHandlers();
    }

    private void InitializeComponent()
    {
        this.Text = "Pro5Chrome Manager by hieuck";
        this.Size = new Size(1280, 800);
        this.MinimumSize = new Size(1024, 768);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.SuspendLayout();

        // Main layout: Top bars and main content area
        var mainTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
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

        // --- NEW 3-COLUMN LAYOUT ---
        var contentTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(5) };
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F)); // A bit more for profiles
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        mainTableLayout.Controls.Add(contentTableLayout, 0, 2);

        InitializeProfilesList(contentTableLayout, 0, 0);
        InitializeMiddleColumn(contentTableLayout, 1, 0);
        InitializeUrlManagement(contentTableLayout, 2, 0);
        
        InitializeContextMenu();
        this.ResumeLayout(false);
    }

    private void InitializeProfilesList(TableLayoutPanel parent, int column, int row)
    {
        var profilesGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Danh sách Profiles", Padding = new Padding(10) };
        parent.Controls.Add(profilesGroupBox, column, row);

        profileCountLabel = new Label { Dock = DockStyle.Top, Text = "Số lượng Profiles: 0", Padding = new Padding(0, 0, 0, 5) };
        profilesListView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = true, BorderStyle = BorderStyle.None };
        profilesListView.Columns.Add("#", 40, HorizontalAlignment.Left);
        profilesListView.Columns.Add("Tên Profile", -2, HorizontalAlignment.Left); // -2 makes it auto-fill
        profilesGroupBox.Controls.AddRange(new Control[] { profilesListView, profileCountLabel });
    }

    private void InitializeMiddleColumn(TableLayoutPanel parent, int column, int row)
    {
        var middleColumnLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        middleColumnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F)); // Details
        middleColumnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F)); // Window Actions
        parent.Controls.Add(middleColumnLayout, column, row);

        // Top part: Details & Automation
        var profileDetailsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Chi tiết & Tự động hóa", Padding = new Padding(10) };
        middleColumnLayout.Controls.Add(profileDetailsGroupBox, 0, 0);
        
        var detailsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        profileDetailsGroupBox.Controls.Add(detailsLayout);

        emailTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };
        passwordTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5), UseSystemPasswordChar = true };
        otpTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5), UseSystemPasswordChar = true };
        saveProfileButton = new Button { Text = "Lưu Thông Tin", Anchor = AnchorStyles.None, AutoSize = true, Margin = new Padding(5) };
        loginGoogleButton = new Button { Text = "Tự động Đăng nhập/Kháng nghị", Dock = DockStyle.Fill, Height = 40, Margin = new Padding(5) };

        detailsLayout.Controls.Add(new Label { Text = "Email:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 0);
        detailsLayout.Controls.Add(emailTextBox, 1, 0);
        detailsLayout.Controls.Add(new Label { Text = "Password:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 1);
        detailsLayout.Controls.Add(passwordTextBox, 1, 1);
        detailsLayout.Controls.Add(new Label { Text = "OTP Secret:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3) }, 0, 2);
        detailsLayout.Controls.Add(otpTextBox, 1, 2);
        
        detailsLayout.Controls.Add(saveProfileButton, 1, 3);
        detailsLayout.SetColumnSpan(loginGoogleButton, 2);
        detailsLayout.Controls.Add(loginGoogleButton, 0, 4);

        // Bottom part: Window Management
        var windowActionsGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Quản lý Cửa sổ", Padding = new Padding(8) };
        middleColumnLayout.Controls.Add(windowActionsGroupBox, 0, 1);

        var windowActionsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        windowActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        windowActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        windowActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        windowActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        windowActionsGroupBox.Controls.Add(windowActionsLayout);

        minimizeSelectedButton = new Button { Text = "Thu nhỏ", Dock = DockStyle.Fill, Margin = new Padding(3) };
        maximizeSelectedButton = new Button { Text = "Phóng to", Dock = DockStyle.Fill, Margin = new Padding(3) };
        restoreSelectedButton = new Button { Text = "Khôi phục", Dock = DockStyle.Fill, Margin = new Padding(3) };
        closeSelectedButton = new Button { Text = "Đóng", Dock = DockStyle.Fill, Margin = new Padding(3), ForeColor = Color.Red };
        
        windowActionsLayout.Controls.Add(minimizeSelectedButton, 0, 0);
        windowActionsLayout.Controls.Add(maximizeSelectedButton, 1, 0);
        windowActionsLayout.Controls.Add(restoreSelectedButton, 0, 1);
        windowActionsLayout.Controls.Add(closeSelectedButton, 1, 1);
    }

    private void InitializeUrlManagement(TableLayoutPanel parent, int column, int row)
    {
        var urlGroupBox = new GroupBox { Dock = DockStyle.Fill, Text = "Quản lý URL", Padding = new Padding(10) };
        parent.Controls.Add(urlGroupBox, column, row);
        
        var urlMainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        urlMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
        urlMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
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
    
    private void InitializeContextMenu()
    {
        profileContextMenuStrip = new ContextMenuStrip();
        var openItem = profileContextMenuStrip.Items.Add("Mở Profile");
        var closeItem = profileContextMenuStrip.Items.Add("Đóng Profile");
        profileContextMenuStrip.Items.Add(new ToolStripSeparator());
        var copyNameItem = profileContextMenuStrip.Items.Add("Sao chép tên Profile");
        var deleteItem = profileContextMenuStrip.Items.Add("Xóa Profile...");
        deleteItem.ForeColor = Color.Red;
        profileContextMenuStrip.Items.Add(new ToolStripSeparator());
        var minimizeItem = profileContextMenuStrip.Items.Add("Thu nhỏ");
        var maximizeItem = profileContextMenuStrip.Items.Add("Phóng to");
        var restoreItem = profileContextMenuStrip.Items.Add("Khôi phục");

        openItem.Click += (s, e) => ForEachSelectedProfile(p => _manager.OpenChrome(p));
        closeItem.Click += (s, e) => ForEachSelectedProfile(p => _manager.CloseProfileWindow(p));
        copyNameItem.Click += CopySelectedProfileNames_Click;
        deleteItem.Click += DeleteSelectedProfiles_Click;
        minimizeItem.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_MINIMIZE));
        maximizeItem.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_MAXIMIZE));
        restoreItem.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_RESTORE));

        profilesListView.ContextMenuStrip = profileContextMenuStrip;
    }

    private void WireUpEventHandlers()
    {
        chromePathComboBox.SelectedIndexChanged += (s, e) => { if (chromePathComboBox.SelectedItem is string path) _manager.SetSelectedChromePath(path); RefreshProfileList(); };
        addPathButton.Click += AddPathButton_Click;
        deletePathButton.Click += DeletePathButton_Click;
        discoverProfilesButton.Click += (s, e) => { _manager.DiscoverAndAddProfiles(); RefreshProfileList(); };

        profilesListView.SelectedIndexChanged += ProfilesListView_SelectedIndexChanged;
        profilesListView.DoubleClick += (s, e) => ForEachSelectedProfile(p => _manager.OpenChrome(p));

        saveProfileButton.Click += SaveProfileButton_Click;
        loginGoogleButton.Click += LoginGoogleButton_Click;

        addUrlButton.Click += (s, e) => { if (!string.IsNullOrEmpty(newUrlTextBox.Text.Trim())) { _urlManager.AddUrl(newUrlTextBox.Text.Trim()); newUrlTextBox.Clear(); RefreshUrlList(); } };
        deleteSelectedUrlButton.Click += (s, e) => { if (urlsListBox.SelectedItem is string url) { _urlManager.DeleteUrl(url); RefreshUrlList(); } };
        deleteAllUrlsButton.Click += (s, e) => { if (MessageBox.Show("Xóa tất cả URLs?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _urlManager.ClearAllUrls(); RefreshUrlList(); } };
        urlsListBox.DoubleClick += (s, e) => { if (GetFirstSelectedProfile() != null && urlsListBox.SelectedItem is string url) _manager.OpenChrome(GetFirstSelectedProfile(), url); };

        minimizeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_MINIMIZE));
        maximizeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_MAXIMIZE));
        restoreSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => WindowManager.PerformActionOnProfileWindow(p, WindowManager.SW_RESTORE));
        closeSelectedButton.Click += (s, e) => ForEachSelectedProfile(p => _manager.CloseProfileWindow(p));

        alwaysOnTopCheckBox.CheckedChanged += (s, e) => { this.TopMost = alwaysOnTopCheckBox.Checked; _manager.SetAlwaysOnTop(alwaysOnTopCheckBox.Checked); };
        hideProfileNamesCheckBox.CheckedChanged += (s, e) => RefreshProfileList();

        openConfigButton.Click += (s, e) => SafeOpenFile("config.json");
        openProfilesJsonButton.Click += (s, e) => SafeOpenFile("profiles.json");
        openUrlJsonButton.Click += (s, e) => SafeOpenFile("URL.json");
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        this.TopMost = _manager.IsAlwaysOnTop();
        alwaysOnTopCheckBox.Checked = _manager.IsAlwaysOnTop();
        RefreshChromePathList();
        RefreshProfileList();
        RefreshUrlList();
        SetControlStates();
    }

    private void RefreshChromePathList()
    {
        var allPaths = _manager.GetAllChromePaths();
        var selectedPath = _manager.GetSelectedChromePath();
        chromePathComboBox.DataSource = allPaths;
        chromePathComboBox.SelectedItem = selectedPath;
    }

    private void RefreshProfileList()
    {
        var selectedProfileNames = GetSelectedProfileNames(); 
        profilesListView.Items.Clear();
        var profiles = _manager.GetProfiles();
        bool hideNames = hideProfileNamesCheckBox.Checked;

        for (int i = 0; i < profiles.Count; i++)
        {
            var profileName = profiles[i];
            var item = new ListViewItem((i + 1).ToString());
            item.Tag = profileName;
            item.SubItems.Add(hideNames ? "******" : profileName);
            profilesListView.Items.Add(item);
            if (selectedProfileNames.Contains(profileName)) item.Selected = true;
        }

        profileCountLabel.Text = $"Số lượng Profiles: {profiles.Count}";
        if (profilesListView.SelectedItems.Count == 0 && profilesListView.Items.Count > 0) profilesListView.Items[0].Selected = true;
        profilesListView.Focus();
        SetControlStates();
    }

    private void RefreshUrlList() => urlsListBox.DataSource = _urlManager.GetUrls();

    private void AddPathButton_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog { Filter = "Browser Executable|chrome.exe;centbrowser.exe|All files (*.*)|*.*", Title = "Chọn file thực thi của trình duyệt" })
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _manager.AddChromePath(ofd.FileName);
                _manager.SetSelectedChromePath(ofd.FileName);
                RefreshChromePathList();
            }
        }
    }

    private void DeletePathButton_Click(object sender, EventArgs e)
    {
        if (chromePathComboBox.SelectedItem is string path && MessageBox.Show($"Bạn có chắc muốn xóa đường dẫn '{path}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _manager.DeleteChromePath(path);
            RefreshChromePathList();
        }
    }
    private void CopySelectedProfileNames_Click(object sender, EventArgs e)
    {
        string[] selectedNames = GetSelectedProfileNames();
        if (selectedNames.Length > 0)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, selectedNames));
        }
    }

    private void DeleteSelectedProfiles_Click(object sender, EventArgs e)
    {
        string[] profilesToDelete = GetSelectedProfileNames();
        if (profilesToDelete.Length == 0) return;

        string firstProfiles = string.Join(", ", profilesToDelete.Take(3));
        if (profilesToDelete.Length > 3) firstProfiles += ", ...";

        string question = profilesToDelete.Length > 1
            ? $"Bạn có muốn xóa luôn thư mục dữ liệu của {profilesToDelete.Length} profiles đã chọn không?\n({firstProfiles})"
            : $"Bạn có muốn xóa luôn thư mục dữ liệu của profile '{profilesToDelete[0]}' không?";

        string text = $@"{question}

• Yes: Xóa profile khỏi danh sách VÀ xóa thư mục dữ liệu.
• No: Chỉ xóa profile khỏi danh sách.
• Cancel: Hủy bỏ hành động.

CẢNH BÁO: Việc xóa thư mục dữ liệu không thể hoàn tác.";

        string caption = "Xác nhận Xóa Profile";
        DialogResult result = MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel) return;

        bool deleteDirectory = (result == DialogResult.Yes);

        foreach (var profileName in profilesToDelete)
        {
            _manager.CloseProfileWindow(profileName);
            _manager.DeleteProfile(profileName, deleteDirectory);
        }

        RefreshProfileList();
    }

    private void ProfilesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (profileName != null)
        {
            var profileDetails = _manager.GetProfileDetails(profileName);
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
        string profileName = GetFirstSelectedProfile();
        if (profileName != null)
        {
            _manager.UpdateProfileDetails(profileName, emailTextBox.Text, passwordTextBox.Text, otpTextBox.Text);
            MessageBox.Show("Đã lưu thông tin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void LoginGoogleButton_Click(object sender, EventArgs e)
    {
        string profileName = GetFirstSelectedProfile();
        if (string.IsNullOrEmpty(profileName))
        {
            MessageBox.Show("Vui lòng chọn một profile.", "Chưa chọn profile");
            return;
        }

        // Disable the button to prevent multiple clicks
        loginGoogleButton.Enabled = false;
        this.Cursor = Cursors.WaitCursor;

        try
        {
            // Run the automation task in the background to keep the UI responsive
            await Task.Run(() => _manager.AutomateLogin(profileName));
        }
        finally
        {
            // Re-enable the button and restore cursor on completion or failure
            loginGoogleButton.Enabled = true;
            this.Cursor = Cursors.Default;
        }
    }

    private void SetControlStates()
    {
        bool isProfileSelected = GetFirstSelectedProfile() != null;
        saveProfileButton.Enabled = isProfileSelected;
        loginGoogleButton.Enabled = isProfileSelected;
        minimizeSelectedButton.Enabled = isProfileSelected;
        maximizeSelectedButton.Enabled = is-selected;
        restoreSelectedButton.Enabled = isProfileSelected;
        closeSelectedButton.Enabled = isProfileSelected;
    }

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
