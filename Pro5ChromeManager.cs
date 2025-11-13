
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

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
        _profiles.Sort();
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

        AddProfile(profileName); 

        string arguments = $"--profile-directory=\"Profile {profileName}\"";
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
            catch { /* Bỏ qua lỗi nếu không thể đóng tiến trình */ }
        }
    }
}
