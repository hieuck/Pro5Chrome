
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Profile
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Otp { get; set; } // Stores the OTP Secret
}

public class AppConfig
{
    public List<string> ChromePaths { get; set; } = new List<string>();
    public string SelectedChromePath { get; set; }
    public bool AlwaysOnTop { get; set; } = false;
}

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

    #region Configuration Management

    private void LoadConfig()
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
        catch (Exception ex) { MessageBox.Show($"Lỗi khi đọc file config.json: {ex.Message}"); _config = new AppConfig(); }
    }

    private void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFileName, json);
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi khi lưu file config.json: {ex.Message}"); }
    }

    public List<string> GetAllChromePaths() => _config.ChromePaths;
    public string GetSelectedChromePath() => _config.SelectedChromePath;
    public bool IsAlwaysOnTop() => _config.AlwaysOnTop;
    public void SetSelectedChromePath(string path) { if (_config.ChromePaths.Contains(path)) { _config.SelectedChromePath = path; SaveConfig(); } }
    public void AddChromePath(string path) { if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) { path = Path.GetFullPath(path); if (!_config.ChromePaths.Contains(path)) { _config.ChromePaths.Add(path); SaveConfig(); } } }
    public void DeleteChromePath(string path) { if (_config.ChromePaths.Remove(path)) { if (_config.SelectedChromePath == path) { _config.SelectedChromePath = _config.ChromePaths.FirstOrDefault(); } SaveConfig(); } }
    public void SetAlwaysOnTop(bool value) { _config.AlwaysOnTop = value; SaveConfig(); }

    public string GetEffectiveUserDataPath()
    {
        if (!string.IsNullOrEmpty(_config.SelectedChromePath) && File.Exists(_config.SelectedChromePath))
        {
            DirectoryInfo exeDir = new DirectoryInfo(Path.GetDirectoryName(_config.SelectedChromePath));
            string potentialPath = Path.Combine(exeDir.FullName, "User Data");
            if (Directory.Exists(potentialPath)) return potentialPath;
            if (exeDir.Parent != null) { potentialPath = Path.Combine(exeDir.Parent.FullName, "User Data"); if (Directory.Exists(potentialPath)) return potentialPath; }
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }

    #endregion

    #region Automation

    // MODIFIED: Corrected the method call to include userDataPath and profileName
    public void AutomateLogin(string profileName)
    {
        var profileDetails = GetProfileDetails(profileName);
        if (profileDetails == null || string.IsNullOrEmpty(profileDetails.Email) || string.IsNullOrEmpty(profileDetails.Password))
        {
            MessageBox.Show("Vui lòng lưu email và mật khẩu cho profile này trước.", "Thiếu thông tin");
            return;
        }

        try
        {
            string userDataPath = GetEffectiveUserDataPath();
            SeleniumManager.LoginGoogle(userDataPath, profileName, profileDetails.Email, profileDetails.Password, profileDetails.Otp);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi khi tự động đăng nhập:\n\n{ex.Message}", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // MODIFIED: Corrected the order of arguments in the method call
    public void WarmUpAccount(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
            MessageBox.Show("Vui lòng chọn một profile để nuôi.", "Chưa chọn Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string userDataPath = GetEffectiveUserDataPath();
            // The original call had the arguments swapped. This is the correct order.
            SeleniumManager.WarmUpAccount(userDataPath, profileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi khi chuẩn bị quá trình nuôi tài khoản:\n\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Profile Data Management

    private void LoadProfiles()
    {
        if (!File.Exists(ProfilesFileName)) { _profiles = new List<Profile>(); return; }
        try
        {
            string json = File.ReadAllText(ProfilesFileName);
            _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi không xác định khi đọc profiles.json: {ex.Message}");
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
        catch (Exception ex) { MessageBox.Show($"Lỗi khi lưu file profiles.json: {ex.Message}"); }
    }

    public List<string> GetProfiles() => _profiles.Select(p => p.Name).ToList();
    public Profile GetProfileDetails(string profileName) => _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));

    public void DiscoverAndAddProfiles()
    {
        string userDataPath = GetEffectiveUserDataPath();
        if (!Directory.Exists(userDataPath)) { MessageBox.Show($"Không tìm thấy thư mục User Data tại: {userDataPath}"); return; }
        int newProfilesCount = 0;
        try
        {
            var existingProfileNames = new HashSet<string>(this.GetProfiles(), StringComparer.OrdinalIgnoreCase);
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
            if (newProfilesCount > 0) { SaveProfiles(); MessageBox.Show($"Đã tìm thấy và thêm {newProfilesCount} profile mới."); }
            else { MessageBox.Show("Không tìm thấy profile mới nào."); }
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi khi quét thư mục profile: {ex.Message}"); }
    }

    public void UpdateProfileDetails(string profileName, string email, string password, string otp)
    {
        var profileToUpdate = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToUpdate == null)
        {
            profileToUpdate = new Profile { Name = profileName };
            _profiles.Add(profileToUpdate);
        }
        profileToUpdate.Email = email;
        profileToUpdate.Password = password;
        profileToUpdate.Otp = otp;
        SaveProfiles();
    }

    public bool DeleteProfile(string profileName, bool deleteDirectory)
    {
        var profileToRemove = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToRemove == null) return false;
        _profiles.Remove(profileToRemove);
        SaveProfiles();
        if (deleteDirectory)
        {
            try
            {
                string profilePath = Path.Combine(GetEffectiveUserDataPath(), profileName);
                if (Directory.Exists(profilePath)) { Directory.Delete(profilePath, true); }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi khi xóa thư mục profile '{profileName}':\n{ex.Message}"); }
        }
        return true;
    }

    #endregion

    #region Browser Process & Window Management

    public void OpenChrome(string profileName, string url = null)
    {
        if (string.IsNullOrWhiteSpace(_config.SelectedChromePath) || !File.Exists(_config.SelectedChromePath)) { MessageBox.Show("Vui lòng chọn một đường dẫn trình duyệt hợp lệ."); return; }
        try
        {
            string arguments = $"--profile-directory="{profileName}" ";
            if (!string.IsNullOrEmpty(url)) { arguments += $""{url}""; }
            ProcessStartInfo startInfo = new ProcessStartInfo(_config.SelectedChromePath, arguments);
            Process proc = Process.Start(startInfo);
            if (proc != null) { WindowManager.RegisterProfileProcess(profileName, proc); }
        }
        catch (Exception ex) { MessageBox.Show($"Không thể mở trình duyệt: {ex.Message}"); }
    }

    public void CloseProfileWindow(string profileName)
    {
        if (!string.IsNullOrWhiteSpace(profileName)) { WindowManager.CloseProfileWindow(profileName); }
    }

    #endregion
}
