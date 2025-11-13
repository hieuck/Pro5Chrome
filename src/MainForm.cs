
using System;
using System.Drawing;
using System.Windows.Forms;

public class MainForm : Form
{
    // Business Logic Managers
    private Pro5ChromeManager _profileManager;
    private UrlManager _urlManager;

    // UI Controls
    private TabControl mainTabControl;
    private TabPage profilesTabPage;
    private TabPage urlsTabPage;

    // Profile Tab Controls
    private ListBox profilesListBox;
    private ComboBox profileComboBox;
    private Button openProfileButton;
    private Button deleteProfileButton;
    private Button openAllProfilesButton;
    private Button closeAllChromeButton;

    // URL Tab Controls
    private ListBox urlsListBox;
    private TextBox newUrlTextBox;
    private Button addUrlButton;
    private Button deleteUrlButton;
    private Button clearUrlsButton;
    private Button openUrlWithProfileButton;
    private Button openUrlWithAllProfilesButton;


    public MainForm()
    {
        _profileManager = new Pro5ChromeManager();
        _urlManager = new UrlManager();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Form settings
        this.Text = "Pro5Chrome C# Edition";
        this.Size = new Size(600, 450);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Load += MainForm_Load;

        // Tab Control
        mainTabControl = new TabControl() { Dock = DockStyle.Fill };
        profilesTabPage = new TabPage("Profiles");
        urlsTabPage = new TabPage("URLs");
        mainTabControl.TabPages.Add(profilesTabPage);
        mainTabControl.TabPages.Add(urlsTabPage);
        this.Controls.Add(mainTabControl);

        // --- Profiles Tab --- 
        profilesTabPage.SuspendLayout();
        profileComboBox = new ComboBox() { Location = new Point(10, 15), Width = 200 };
        openProfileButton = new Button() { Text = "Mở / Thêm", Location = new Point(220, 14) };
        openProfileButton.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(profileComboBox.Text)){
                 _profileManager.OpenChrome(profileComboBox.Text);
                 RefreshProfileLists();
            }
        };
        profilesListBox = new ListBox() { Location = new Point(10, 50), Size = new Size(390, 300) };
        profilesListBox.DoubleClick += (s, e) => {
             if (profilesListBox.SelectedItem != null) _profileManager.OpenChrome(profilesListBox.SelectedItem.ToString());
        };

        deleteProfileButton = new Button() { Text = "Xóa Profile", Location = new Point(410, 50) };
        deleteProfileButton.Click += (s, e) => {
            if (profilesListBox.SelectedItem != null && MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes){
                _profileManager.DeleteProfile(profilesListBox.SelectedItem.ToString());
                RefreshProfileLists();
            }
        };
        
        openAllProfilesButton = new Button() { Text = "Mở tất cả Profile", Location = new Point(410, 90), Size = new Size(120, 23) };
        openAllProfilesButton.Click += (s, e) => {
            foreach(var profile in _profileManager.GetProfiles()) _profileManager.OpenChrome(profile);
        };

        closeAllChromeButton = new Button() { Text = "Đóng tất cả Chrome", Location = new Point(410, 130), Size = new Size(120, 23) };
        closeAllChromeButton.Click += (s, e) => _profileManager.CloseAllChrome();

        profilesTabPage.Controls.AddRange(new Control[] { profileComboBox, openProfileButton, profilesListBox, deleteProfileButton, openAllProfilesButton, closeAllChromeButton });
        profilesTabPage.ResumeLayout();

        // --- URLs Tab ---
        urlsTabPage.SuspendLayout();
        newUrlTextBox = new TextBox() { Location = new Point(10, 15), Width = 300 };
        addUrlButton = new Button() { Text = "Thêm URL", Location = new Point(320, 14) };
        addUrlButton.Click += (s, e) => {
            _urlManager.AddUrl(newUrlTextBox.Text);
            newUrlTextBox.Clear();
            RefreshUrlList();
        };
        urlsListBox = new ListBox() { Location = new Point(10, 50), Size = new Size(390, 300) };

        deleteUrlButton = new Button() { Text = "Xóa URL", Location = new Point(410, 50) };
        deleteUrlButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null){
                _urlManager.DeleteUrl(urlsListBox.SelectedItem.ToString());
                RefreshUrlList();
            }
        };
        
        clearUrlsButton = new Button() { Text = "Xóa tất cả URL", Location = new Point(410, 90), Size = new Size(120, 23) };
        clearUrlsButton.Click += (s, e) => {
            if (MessageBox.Show("Bạn có chắc muốn xóa tất cả URL?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes){
                 _urlManager.ClearAllUrls();
                 RefreshUrlList();
            }
        };

        openUrlWithProfileButton = new Button() { Text = "Mở với Profile đã chọn", Location = new Point(10, 360), AutoSize = true };
        openUrlWithProfileButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null && !string.IsNullOrWhiteSpace(profileComboBox.Text)){
                _profileManager.OpenChrome(profileComboBox.Text, urlsListBox.SelectedItem.ToString());
            } else {
                MessageBox.Show("Vui lòng chọn một URL và một Profile (ở tab Profiles).");
            }
        };
        
        openUrlWithAllProfilesButton = new Button() { Text = "Mở với tất cả Profile", Location = new Point(180, 360), AutoSize = true };
        openUrlWithAllProfilesButton.Click += (s, e) => {
            if (urlsListBox.SelectedItem != null) {
                foreach(var profile in _profileManager.GetProfiles()){
                    _profileManager.OpenChrome(profile, urlsListBox.SelectedItem.ToString());
                }
            } else {
                 MessageBox.Show("Vui lòng chọn một URL.");
            }
        };

        urlsTabPage.Controls.AddRange(new Control[] { newUrlTextBox, addUrlButton, urlsListBox, deleteUrlButton, clearUrlsButton, openUrlWithProfileButton, openUrlWithAllProfilesButton });
        urlsTabPage.ResumeLayout();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        RefreshProfileLists();
        RefreshUrlList();
    }

    private void RefreshProfileLists()
    {
        var profiles = _profileManager.GetProfiles();
        string currentComboBoxText = profileComboBox.Text;
        profilesListBox.Items.Clear();
        profileComboBox.Items.Clear();
        foreach (var profile in profiles)
        {
            profilesListBox.Items.Add(profile);
            profileComboBox.Items.Add(profile);
        }
        profileComboBox.Text = currentComboBoxText;
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
}
