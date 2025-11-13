
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

public class Profile
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class Pro5ChromeManager
{
    private const string ConfigFileName = "config.json";
    private const string ProfilesFileName = "profiles.json";
    public const string GoogleLoginUrl = "https://accounts.google.com";

    private List<Profile> _profiles = new List<Profile>();
    private Config _config = new Config();

    private class Config
    {
        public List<string> ChromePaths { get; set; } = new List<string>();
        public string SelectedChromePath { get; set; }
    }

    public Pro5ChromeManager()
    {
        LoadConfig();
        LoadProfiles();
    }

    // --- Configuration Management ---

    private void LoadConfig()
    {
        if (File.Exists(ConfigFileName))
        {
            try
            {
                string json = File.ReadAllText(ConfigFileName);
                _config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc file config.json: {ex.Message}");
                _config = new Config();
            }
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
            MessageBox.Show($"Lỗi khi lưu file config.json: {ex.Message}");
        }
    }

    public List<string> GetChromePaths() => _config.ChromePaths;
    public string GetSelectedChromePath() => _config.SelectedChromePath;

    public void AddAndSelectChromePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!path.StartsWith("\\") && !File.Exists(path)) return;
        if (!_config.ChromePaths.Contains(path))
        {
            _config.ChromePaths.Add(path);
        }
        _config.SelectedChromePath = path;
        SaveConfig();
    }

    public void DeleteChromePath(string path)
    {
        if (_config.ChromePaths.Contains(path))
        {
            _config.ChromePaths.Remove(path);
            if (_config.SelectedChromePath == path)
            {
                _config.SelectedChromePath = _config.ChromePaths.FirstOrDefault();
            }
            SaveConfig();
        }
    }

    // --- User Data Path Logic ---
    public string GetEffectiveUserDataPath()
    {
        if (!string.IsNullOrEmpty(_config.SelectedChromePath) && File.Exists(_config.SelectedChromePath))
        {
            DirectoryInfo exeDir = new DirectoryInfo(Path.GetDirectoryName(_config.SelectedChromePath));
            string potentialPath1 = Path.Combine(exeDir.FullName, "User Data");
            if (Directory.Exists(potentialPath1)) return potentialPath1;

            if (exeDir.Parent != null)
            {
                string potentialPath2 = Path.Combine(exeDir.Parent.FullName, "User Data");
                if (Directory.Exists(potentialPath2)) return potentialPath2;
            }
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }


    // --- Profile Data Management ---

    private void LoadProfiles()
    {
        if (!File.Exists(ProfilesFileName)) { _profiles = new List<Profile>(); return; }
        try
        {
            string json = File.ReadAllText(ProfilesFileName);
            if (string.IsNullOrWhiteSpace(json)) { _profiles = new List<Profile>(); return; }

            if (json.Trim().StartsWith("[") && json.Contains("\""))
            {
                try
                {
                    var stringProfiles = JsonSerializer.Deserialize<List<string>>(json);
                    if (stringProfiles != null && !stringProfiles.Any(s => s.Contains("{")))
                    {
                        _profiles = stringProfiles.Select(name => new Profile { Name = name }).ToList();
                        SaveProfiles();
                        return;
                    }
                }
                catch { }
            }
            _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đọc file profiles.json: {ex.Message}", "Lỗi đọc file");
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

    public void AddProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName) || _profiles.Any(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase))) return;
        _profiles.Add(new Profile { Name = profileName });
        SaveProfiles();
    }

    public int DiscoverAndAddProfiles()
    {
        string userDataPath = GetEffectiveUserDataPath();
        if (!Directory.Exists(userDataPath)){
            MessageBox.Show($"Không tìm thấy thư mục User Data tại: {userDataPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 0;
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

            if (newProfilesCount > 0) SaveProfiles();
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi khi quét thư mục profile: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        return newProfilesCount;
    }

    public void DeleteProfile(string profileName)
    {
        var profileToRemove = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToRemove != null && MessageBox.Show($"Bạn có chắc chắn muốn xóa profile '{profileName}'?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _profiles.Remove(profileToRemove);
            SaveProfiles();
        }
    }

    public void UpdateProfileDetails(string profileName, string email, string password)
    {
        var profileToUpdate = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToUpdate == null)
        {
             profileToUpdate = new Profile { Name = profileName };
            _profiles.Add(profileToUpdate);
        }
        profileToUpdate.Email = email;
        profileToUpdate.Password = password;
        SaveProfiles();
    }

    // --- Browser Process & Window Management ---

    public void OpenChrome(string profileName, string url = null)
    {
        if (string.IsNullOrWhiteSpace(_config.SelectedChromePath) || !File.Exists(_config.SelectedChromePath)){
            MessageBox.Show("Đường dẫn đến file thực thi của trình duyệt không hợp lệ.", "Lỗi đường dẫn");
            return;
        }
        try
        {
            Process.Start(_config.SelectedChromePath, $"--profile-directory={profileName} \"{url}\"");
        }
        catch (Exception ex) { MessageBox.Show($"Không thể mở trình duyệt: {ex.Message}"); }
    }

    public void CloseProfileWindow(string profileName) 
    { 
        if (!string.IsNullOrWhiteSpace(profileName)) WindowManager.CloseWindowByProfileName(profileName, _config.SelectedChromePath); 
    }

    public void MaximizeProfileWindow(string profileName) 
    { 
        if (!string.IsNullOrWhiteSpace(profileName)) WindowManager.MaximizeWindowByProfileName(profileName, _config.SelectedChromePath); 
    }

    public void MinimizeProfileWindow(string profileName) 
    { 
        if (!string.IsNullOrWhiteSpace(profileName)) WindowManager.MinimizeWindowByProfileName(profileName, _config.SelectedChromePath); 
    }

    public void RestoreProfileWindow(string profileName) 
    { 
        if (!string.IsNullOrWhiteSpace(profileName)) WindowManager.RestoreWindowByProfileName(profileName, _config.SelectedChromePath); 
    }

    public void CloseAllChrome()
    {
        try
        {
            string processName = "chrome";
            if (!string.IsNullOrEmpty(_config.SelectedChromePath) && File.Exists(_config.SelectedChromePath))
            {
                processName = Path.GetFileNameWithoutExtension(_config.SelectedChromePath);
            }

            foreach (var process in Process.GetProcessesByName(processName))
            {
                if (!process.HasExited) process.Kill();
            }
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi khi đóng trình duyệt: {ex.Message}"); }
    }
}
