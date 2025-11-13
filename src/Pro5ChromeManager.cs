
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

public class Pro5ChromeManager
{
    private static string _chromePath;
    private List<string> _profiles;
    private const string CONFIG_FILE = "config.json";
    private const string PROFILES_FILE = "profiles.json";

    // URL to direct users to for signing in and enabling sync
    public const string GoogleLoginUrl = "https://accounts.google.com/signin/chrome/sync";

    public Pro5ChromeManager()
    {
        LoadConfig();
        LoadProfiles();
    }

    private void LoadConfig()
    {
        if (File.Exists(CONFIG_FILE))
        {
            var configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(CONFIG_FILE));
            _chromePath = configData.ContainsKey("chrome_path") ? configData["chrome_path"] : @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        }
        else
        {
            _chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
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
            File.WriteAllText(PROFILES_FILE, "[]");
        }
    }

    private void SaveProfiles()
    {
        string json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
        File.WriteAllText(PROFILES_FILE, json);
    }
    
    public List<string> GetProfiles()
    {
        LoadProfiles();
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
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa profile '{profileName}' không?\n\nViệc này sẽ xóa profile khỏi danh sách.\n\nBạn có muốn xóa cả thư mục dữ liệu của profile này không? (Hành động này không thể hoàn tác)", 
                "Xác nhận Xóa Profile", 
                MessageBoxButtons.YesNoCancel, 
                MessageBoxIcon.Warning);

            if (result == DialogResult.Cancel) return;
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    string profileDir = Path.Combine(GetUserDataPath(), GetProfileDirectoryName(profileName));
                    if (Directory.Exists(profileDir))
                    {
                        Directory.Delete(profileDir, true);
                        MessageBox.Show($"Đã xóa thành công thư mục cho profile: {profileName}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể xóa thư mục profile. Lỗi: {ex.Message}");
                }
            }
            
            _profiles.Remove(profileName);
            SaveProfiles();
        }
    }

    public void OpenChrome(string profileName, string url = null)
    {
        if (string.IsNullOrEmpty(profileName)) return;

        if (!File.Exists(_chromePath))
        {
            MessageBox.Show($"Không tìm thấy Chrome tại: {_chromePath}\nVui lòng kiểm tra lại file config.json.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Only add profile to list if not just opening a URL for it
        if (url == null) {
            AddProfile(profileName); 
        }

        string profileDirName = GetProfileDirectoryName(profileName);
        string arguments = $"--profile-directory=\"{profileDirName}\"";
        if (!string.IsNullOrEmpty(url))
        {
            arguments += $" \"{url}\"";
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = _chromePath,
            Arguments = arguments
        };
        Process.Start(startInfo);
    }

    public void CloseAllChrome()
    {
        foreach (var process in Process.GetProcessesByName("chrome"))
        {
            try { process.Kill(); }
            catch { /* Ignore errors if the process can't be killed */ }
        }
    }

    public static string GetUserDataPath()
    {
        string chromeDir = Path.GetDirectoryName(_chromePath);
        if (string.IsNullOrEmpty(chromeDir)) return string.Empty;
        return Path.GetFullPath(Path.Combine(chromeDir, "..", "User Data"));
    }

    public static string GetProfileDirectoryName(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName) || profileName.Equals("Default", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Default";
        }

        string trimmed = profileName.Trim();
        if (trimmed.ToLower().StartsWith("profile "))
        {
            return "Profile" + trimmed.Substring(7).Trim();
        }
        
        // If it's just a number, format it as "Profile [number]"
        if(int.TryParse(trimmed, out _))
        {
             return "Profile " + trimmed;
        }

        return trimmed; 
    }
}
