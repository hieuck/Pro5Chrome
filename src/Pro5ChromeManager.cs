
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;

public class Profile
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string OtpSecret { get; set; } // Renamed from Otp for clarity
}

public class AppConfig
{
    public List<string> ChromePaths { get; set; } = new List<string>();
    public int SelectedIndex { get; set; } = -1; // Use index for combo box selection
    public bool HideProfileNames { get; set; } // For the feature to hide profile names
}

public class Pro5ChromeManager
{
    private const string ConfigFileName = "config.json";
    private const string ProfilesFileName = "profiles.json";

    private List<Profile> _profiles = new List<Profile>();
    private AppConfig _config = new AppConfig();
    private readonly Action<string> _log;

    public Pro5ChromeManager(Action<string> logger)
    {
        _log = logger ?? (message => { });
    }

    #region Configuration Management

    public void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFileName))
            {
                string json = File.ReadAllText(ConfigFileName);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else { SaveConfig(); }
        }
        catch (Exception ex) { _log($"Lỗi khi đọc file config.json: {ex.Message}"); _config = new AppConfig(); }
    }

    private void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFileName, json);
        }
        catch (Exception ex) { _log($"Lỗi khi lưu file config.json: {ex.Message}"); }
    }
    public AppConfig GetConfig() => _config;
    public List<string> GetChromePaths() => _config.ChromePaths;
    public string GetSelectedChromePath()
    {
        if (_config.SelectedIndex >= 0 && _config.SelectedIndex < _config.ChromePaths.Count)
        {
            return _config.ChromePaths[_config.SelectedIndex];
        }
        return null;
    }
    public void SetSelectedChromePath(int index) { _config.SelectedIndex = index; SaveConfig(); }
    public void AddChromePath(string path) { if (!string.IsNullOrWhiteSpace(path) && !_config.ChromePaths.Contains(path)) { _config.ChromePaths.Add(path); SaveConfig(); _log($"Đã thêm đường dẫn trình duyệt: {path}"); } }
    public void RemoveChromePath(int index) { if (index >= 0 && index < _config.ChromePaths.Count) { string path = _config.ChromePaths[index]; _config.ChromePaths.RemoveAt(index); if (_config.SelectedIndex >= _config.ChromePaths.Count) { _config.SelectedIndex = _config.ChromePaths.Count - 1; } SaveConfig(); _log($"Đã xóa đường dẫn trình duyệt: {path}"); } }
    public void SetHideProfileNames(bool hide) { _config.HideProfileNames = hide; SaveConfig(); }

    public string GetEffectiveUserDataPath()
    {
        string selectedPath = GetSelectedChromePath();
        if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
        {
            DirectoryInfo exeDir = new DirectoryInfo(Path.GetDirectoryName(selectedPath));
            string potentialPath = Path.Combine(exeDir.FullName, "User Data");
            if (Directory.Exists(potentialPath)) return potentialPath;
            if (exeDir.Parent != null) { potentialPath = Path.Combine(exeDir.Parent.FullName, "User Data"); if (Directory.Exists(potentialPath)) return potentialPath; }
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }

    #endregion

    #region Automation

    public void LoginGoogle(string profileName)
    {
        var profileDetails = GetProfile(profileName);
        if (profileDetails == null || string.IsNullOrEmpty(profileDetails.Email) || string.IsNullOrEmpty(profileDetails.Password))
        {
            _log("Vui lòng lưu email và mật khẩu cho profile này trước khi tự động đăng nhập.");
            return;
        }

        try
        {
            string userDataPath = GetEffectiveUserDataPath();
            _log($"Bắt đầu đăng nhập cho profile '{profileName}'...");
            SeleniumManager.LoginGoogle(userDataPath, profileName, profileDetails.Email, profileDetails.Password, profileDetails.OtpSecret);
            _log("Quá trình đăng nhập/kháng nghị đã được Selenium xử lý.");
        }
        catch (Exception ex)
        {
            _log($"LỖI khi tự động đăng nhập cho '{profileName}': {ex.Message}");
            throw; // Re-throw to UI
        }
    }

    public void WarmUpAccount(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
             _log("Profile không được chọn để thực hiện hành động.");
            return;
        }

        try
        {
            string userDataPath = GetEffectiveUserDataPath();
            SeleniumManager.WarmUpAccount(userDataPath, profileName);
        }
        catch (Exception ex)
        {
            _log($"LỖI khi chuẩn bị quá trình nuôi tài khoản cho '{profileName}': {ex.Message}");
             throw; // Re-throw to be caught by the UI thread for cursor/button handling.
        }
    }

    #endregion

    #region Profile Data Management

    public void LoadProfiles()
    {
        if (!File.Exists(ProfilesFileName)) { _profiles = new List<Profile>(); return; }
        try
        {
            string json = File.ReadAllText(ProfilesFileName);
            _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
        }
        catch (Exception ex)
        {
            _log($"Lỗi không xác định khi đọc profiles.json: {ex.Message}");
            _profiles = new List<Profile>();
        }
    }

    private void SaveProfiles()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string json = JsonSerializer.Serialize(_profiles, options);
            File.WriteAllText(ProfilesFileName, json);
        }
        catch (Exception ex) { _log($"Lỗi khi lưu file profiles.json: {ex.Message}"); }
    }

    public List<Profile> GetProfiles() => _profiles;
    public Profile GetProfile(string profileName) => _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));

    public void DiscoverProfiles()
    {
        string userDataPath = GetEffectiveUserDataPath();
        if (!Directory.Exists(userDataPath)) { _log($"Không tìm thấy thư mục User Data tại: {userDataPath}"); return; }
        int newProfilesCount = 0;
        try
        {
            var existingProfileNames = new HashSet<string>(_profiles.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
            var directories = Directory.GetDirectories(userDataPath, "Profile *", SearchOption.TopDirectoryOnly).Concat(Directory.GetDirectories(userDataPath, "Default", SearchOption.TopDirectoryOnly));
            foreach (var dir in directories)
            {
                string profileFolderName = new DirectoryInfo(dir).Name;
                if (!existingProfileNames.Contains(profileFolderName))
                {
                    _profiles.Add(new Profile { Name = profileFolderName });
                    newProfilesCount++;
                }
            }
            if (newProfilesCount > 0) { SaveProfiles(); _log($"Đã tìm thấy và thêm {newProfilesCount} profile mới."); }
            else { _log("Không tìm thấy profile mới nào."); }
        }
        catch (Exception ex) { _log($"Lỗi khi quét thư mục profile: {ex.Message}"); }
    }

    public void UpdateProfileDetails(string profileName, string email, string password, string otp)
    {
        var profileToUpdate = GetProfile(profileName);
        if (profileToUpdate == null)
        {
            profileToUpdate = new Profile { Name = profileName };
            _profiles.Add(profileToUpdate);
            _log($"Đã tạo mục chi tiết mới cho profile: {profileName}");
        }
        profileToUpdate.Email = email;
        profileToUpdate.Password = password;
        profileToUpdate.OtpSecret = otp;
        SaveProfiles();
    }

    #endregion

    #region Browser Process & Window Management

    public void OpenChromeProfile(string profileName, IEnumerable<string> urls = null)
    {
        string chromePath = GetSelectedChromePath();
        if (string.IsNullOrWhiteSpace(chromePath) || !File.Exists(chromePath))
        {
            _log("Vui lòng chọn một đường dẫn trình duyệt hợp lệ trong cài đặt.");
            return;
        }
        try
        {
            string firstUrl = urls?.FirstOrDefault() ?? "";
            string arguments = $"--profile-directory=\"{profileName}\" \"{firstUrl}\"";
            
            _log($"Đang mở profile '{profileName}'...");

            ProcessStartInfo startInfo = new ProcessStartInfo(chromePath, arguments);
            Process proc = Process.Start(startInfo);
            if (proc != null)
            {
                WindowManager.RegisterProfileProcess(profileName, proc);
            }
        }
        catch (Exception ex)
        {
            _log($"Không thể mở trình duyệt cho profile '{profileName}': {ex.Message}");
        }
    }

    public void CloseChromeProfile(string profileName)
    {
        if (!string.IsNullOrWhiteSpace(profileName)) 
        {
            WindowManager.CloseProfileWindow(profileName); 
            _log($"Đã gửi yêu cầu đóng cho profile '{profileName}'.");
        }
    }

    #endregion
}
