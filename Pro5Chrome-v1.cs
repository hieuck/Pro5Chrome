
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

// Main application class to run the form
public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

// The main window of the application
public class MainForm : Form
{
    // UI Controls
    private ListBox profilesListBox;
    private ComboBox profileComboBox;
    private Button openButton;
    private Button closeAllButton;
    private Button deleteProfileButton;
    private Label profileLabel;

    private Pro5ChromeManager _manager;

    public MainForm()
    {
        _manager = new Pro5ChromeManager();
        InitializeComponent();
        LoadProfiles();
    }

    private void InitializeComponent()
    {
        // Form settings
        this.Text = "Pro5Chrome C# by Gemini";
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Profile ComboBox
        profileComboBox = new ComboBox();
        profileComboBox.Location = new Point(10, 30);
        profileComboBox.Size = new Size(200, 25);
        this.Controls.Add(profileComboBox);

        // Open Button
        openButton = new Button();
        openButton.Text = "Mở Chrome";
        openButton.Location = new Point(220, 30);
        openButton.Click += OpenButton_Click;
        this.Controls.Add(openButton);
        
        // Label for profile list
        profileLabel = new Label();
        profileLabel.Text = "Danh sách Profiles:";
        profileLabel.Location = new Point(10, 70);
        profileLabel.AutoSize = true;
        this.Controls.Add(profileLabel);

        // Profiles ListBox
        profilesListBox = new ListBox();
        profilesListBox.Location = new Point(10, 90);
        profilesListBox.Size = new Size(300, 200);
        profilesListBox.DoubleClick += ProfilesListBox_DoubleClick;
        this.Controls.Add(profilesListBox);
        
        // Delete Profile Button
        deleteProfileButton = new Button();
        deleteProfileButton.Text = "Xóa Profile";
        deleteProfileButton.Location = new Point(320, 90);
        deleteProfileButton.Click += DeleteProfileButton_Click;
        this.Controls.Add(deleteProfileButton);

        // Close All Button
        closeAllButton = new Button();
        closeAllButton.Text = "Đóng tất cả Chrome";
        closeAllButton.Location = new Point(320, 130);
        closeAllButton.Click += CloseAllButton_Click;
        this.Controls.Add(closeAllButton);
    }

    private void LoadProfiles()
    {
        profilesListBox.Items.Clear();
        profileComboBox.Items.Clear();

        var profiles = _manager.GetProfiles();
        foreach (var profile in profiles)
        {
            profilesListBox.Items.Add(profile);
            profileComboBox.Items.Add(profile);
        }
    }

    private void OpenButton_Click(object sender, EventArgs e)
    {
        string profile = profileComboBox.Text;
        if (!string.IsNullOrWhiteSpace(profile))
        {
            _manager.OpenChrome(profile);
            LoadProfiles(); // Refresh lists
        }
        else
        {
            MessageBox.Show("Vui lòng chọn hoặc nhập một profile.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ProfilesListBox_DoubleClick(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem != null)
        {
            string profile = profilesListBox.SelectedItem.ToString();
            _manager.OpenChrome(profile);
        }
    }
    
    private void DeleteProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListBox.SelectedItem != null)
        {
            string profile = profilesListBox.SelectedItem.ToString();
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa profile \'{profile}\' không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                _manager.DeleteProfile(profile);
                LoadProfiles(); // Refresh list
            }
        }
        else
        {
             MessageBox.Show("Vui lòng chọn một profile để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    
    private void CloseAllButton_Click(object sender, EventArgs e)
    {
        _manager.CloseAllChrome();
    }
}

// Logic handler class, similar to the Python script\'s functions
public class Pro5ChromeManager
{
    private string _chromePath;
    private List<string> _profiles;
    private const string CONFIG_FILE = "config.json";
    private const string PROFILES_FILE = "profiles.json";

    public Pro5ChromeManager()
    {
        LoadConfig();
        LoadProfiles();
    }

    private void LoadConfig()
    {
        // Using a simplified config for this example
        if (File.Exists(CONFIG_FILE))
        {
            // Assuming config.json just contains the path in a simple format for now
            // In a real app, use a class like in the previous example
            var configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(CONFIG_FILE));
            _chromePath = configData.ContainsKey("chrome_path") ? configData["chrome_path"] : @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        }
        else
        {
            _chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            // Create a default config file
            var defaultConfig = new Dictionary<string, string> { { "chrome_path", _chromePath } };
            File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
        }
    }

    private void LoadProfiles()
    {
        if (File.Exists(PROFILES_FILE))
        {
            string json = File.ReadAllText(PROFILES_FILE);
            _profiles = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
        else
        {
            _profiles = new List<string>();
        }
    }

    private void SaveProfiles()
    {
        _profiles.Sort();
        string json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
        File.WriteAllText(PROFILES_FILE, json);
    }
    
    public List<string> GetProfiles()
    {
        return _profiles;
    }

    public void AddProfile(string profileName)
    {
        if (!_profiles.Contains(profileName))
        {
            _profiles.Add(profileName);
            SaveProfiles();
        }
    }
    
    public void DeleteProfile(string profileName)
    {
        if (_profiles.Contains(profileName))
        {
            _profiles.Remove(profileName);
            SaveProfiles();
        }
    }

    public void OpenChrome(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return;

        if (!File.Exists(_chromePath))
        {
            MessageBox.Show($"Không tìm thấy Chrome tại: {_chromePath}\nVui lòng kiểm tra lại file config.json.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AddProfile(profileName); // Add profile if it\'s new

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = _chromePath,
            Arguments = $"--profile-directory=\"Profile {profileName}\""
        };
        Process.Start(startInfo);
    }

    public void CloseAllChrome()
    {
        foreach (var process in Process.GetProcessesByName("chrome"))
        {
            try { process.Kill(); }
            catch { /* Ignore errors */ }
        }
    }
}
