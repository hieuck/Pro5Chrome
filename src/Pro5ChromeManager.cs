
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

// Represents the structure of a single profile with its details.
public class Profile
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Otp { get; set; }
}

// Represents the application's configuration data stored in config.json.
public class AppConfig
{
    public List<string> ChromePaths { get; set; } = new List<string>();
    public string SelectedChromePath { get; set; }
    public bool AlwaysOnTop { get; set; } = false;
}

// Manages all business logic for profiles, configuration, and browser interaction.
public class Pro5ChromeManager
{
    private const string ConfigFileName = "config.json";
    private const string ProfilesFileName = "profiles.json";

    private List<Profile> _profiles = new List<Profile>();
    private AppConfig _config = new AppConfig();

    public Pro5ChromeManager()
    {
        LoadConfig();
        LoadProfiles();
    }

    #region Configuration Management (config.json)

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFileName))
            {
                string json = File.ReadAllText(ConfigFileName);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                // Create a default config file if it doesn't exist
                SaveConfig();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đọc file config.json: {ex.Message}", "Lỗi Cấu hình");
            _config = new AppConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFileName, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu file config.json: {ex.Message}", "Lỗi Cấu hình");
        }
    }

    public List<string> GetAllChromePaths() => _config.ChromePaths;
    public string GetSelectedChromePath() => _config.SelectedChromePath;
    public bool IsAlwaysOnTop() => _config.AlwaysOnTop;

    public void SetSelectedChromePath(string path)
    {
        if (_config.ChromePaths.Contains(path))
        {
            _config.SelectedChromePath = path;
            SaveConfig();
        }
    }

    public void AddChromePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        path = Path.GetFullPath(path);
        if (!_config.ChromePaths.Contains(path))
        {
            _config.ChromePaths.Add(path);
            SaveConfig();
        }
    }

    public void DeleteChromePath(string path)
    {
        if (_config.ChromePaths.Remove(path))
        {
            if (_config.SelectedChromePath == path)
            {
                _config.SelectedChromePath = _config.ChromePaths.FirstOrDefault();
            }
            SaveConfig();
        }
    }

    public void SetAlwaysOnTop(bool value)
    {
        _config.AlwaysOnTop = value;
        SaveConfig();
    }

    public string GetEffectiveUserDataPath()
    {
        if (!string.IsNullOrEmpty(_config.SelectedChromePath) && File.Exists(_config.SelectedChromePath))
        {
            // Typically, User Data is in the same directory as the executable (e.g., portable installs)
            // or one level above it.
            DirectoryInfo exeDir = new DirectoryInfo(Path.GetDirectoryName(_config.SelectedChromePath));
            string potentialPath = Path.Combine(exeDir.FullName, "User Data");
            if (Directory.Exists(potentialPath)) return potentialPath;

            if (exeDir.Parent != null)
            {
                potentialPath = Path.Combine(exeDir.Parent.FullName, "User Data");
                if (Directory.Exists(potentialPath)) return potentialPath;
            }
        }
        // Fallback to the default local app data location
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }

    #endregion

    #region Profile Data Management (profiles.json)

    private void LoadProfiles()
    {
        if (!File.Exists(ProfilesFileName)) { _profiles = new List<Profile>(); return; }
        try
        {
            string json = File.ReadAllText(ProfilesFileName);
            _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
        }
        catch (JsonException)
        { 
            // Handle case where profiles.json might be a simple string array from a previous version
            try
            {
                 string json = File.ReadAllText(ProfilesFileName);
                 var stringProfiles = JsonSerializer.Deserialize<List<string>>(json);
                 _profiles = stringProfiles.Select(name => new Profile { Name = name }).ToList();
                 SaveProfiles(); // Resave in the new object format
            }
            catch (Exception ex2)
            {
                MessageBox.Show($"Lỗi khi phân tích profiles.json: {ex2.Message}", "Lỗi đọc file");
                 _profiles = new List<Profile>();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi không xác định khi đọc profiles.json: {ex.Message}", "Lỗi đọc file");
            _profiles = new List<Profile>();
        }
    }

    private void SaveProfiles()
    {
        try
        {
            string json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(ProfilesFileName, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu file profiles.json: {ex.Message}", "Lỗi ghi file");
        }
    }

    public List<string> GetProfiles() => _profiles.Select(p => p.Name).ToList();

    public Profile GetProfileDetails(string profileName) => _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));

    public void DiscoverAndAddProfiles()
    {
        string userDataPath = GetEffectiveUserDataPath();
        if (!Directory.Exists(userDataPath)){
            MessageBox.Show($"Không tìm thấy thư mục User Data tại: {userDataPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int newProfilesCount = 0;
        try
        {
            var existingProfileNames = new HashSet<string>(this.GetProfiles(), StringComparer.OrdinalIgnoreCase);
            var directories = Directory.GetDirectories(userDataPath, "Profile *", SearchOption.TopDirectoryOnly)
                                     .Concat(Directory.GetDirectories(userDataPath, "Default", SearchOption.TopDirectoryOnly));

            foreach (var dir in directories)
            {
                string profileFolderName = new DirectoryInfo(dir).Name;
                if (!existingProfileNames.Contains(profileFolderName))
                {
                    _profiles.Add(new Profile { Name = profileFolderName });
                    newProfilesCount++;
                }
            }

            if (newProfilesCount > 0) 
            {
                SaveProfiles();
                MessageBox.Show($"Đã tìm thấy và thêm {newProfilesCount} profile mới.", "Thành công");
            }
            else
            {
                MessageBox.Show("Không tìm thấy profile mới nào.", "Hoàn tất");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi khi quét thư mục profile: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    public void UpdateProfileDetails(string profileName, string email, string password, string otp)
    {
        var profileToUpdate = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToUpdate == null)
        {
             // This case should ideally not happen if called from the UI, but as a safeguard:
             profileToUpdate = new Profile { Name = profileName };
            _profiles.Add(profileToUpdate);
        }
        profileToUpdate.Email = email;
        profileToUpdate.Password = password;
        profileToUpdate.Otp = otp;
        SaveProfiles();
    }

    #endregion

    #region Browser Process & Window Management

    public void OpenChrome(string profileName, string url = null)
    {
        if (string.IsNullOrWhiteSpace(_config.SelectedChromePath) || !File.Exists(_config.SelectedChromePath)){
            MessageBox.Show("Vui lòng chọn một đường dẫn trình duyệt hợp lệ trong file config.json hoặc trên giao diện.", "Lỗi đường dẫn");
            return;
        }
        try
        {
            string arguments = $"--profile-directory=\"{profileName}\" ";
            if (!string.IsNullOrEmpty(url))
            {
                arguments += $"\"{url}\"";
            }
            
            ProcessStartInfo startInfo = new ProcessStartInfo(_config.SelectedChromePath, arguments);
            Process proc = Process.Start(startInfo);

            // --- Integration with WindowManager ---
            if (proc != null)
            {
                WindowManager.RegisterProfileProcess(profileName, proc);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Không thể mở trình duyệt: {ex.Message}"); }
    }

    // Calls the updated WindowManager method which now handles unregistering the profile.
    public void CloseProfileWindow(string profileName) 
    { 
        if (!string.IsNullOrWhiteSpace(profileName))
        {
             WindowManager.CloseProfileWindow(profileName); 
        }
    }

    #endregion
}
