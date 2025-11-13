
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

// Defines the structure for a profile, holding its name, email, and password.
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
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
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
    
    public static string GetUserDataPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }

    // --- Profile Data Management ---

    private void LoadProfiles()
    {
        if (!File.Exists(ProfilesFileName))
        {
            _profiles = new List<Profile>();
            return;
        }

        try
        {
            string json = File.ReadAllText(ProfilesFileName);
            // Handle old format (List<string>)
            if (json.Trim().StartsWith("[") && json.Contains("\""))
            {
                try
                {
                    var stringProfiles = JsonSerializer.Deserialize<List<string>>(json);
                    _profiles = stringProfiles.Select(name => new Profile { Name = name }).ToList();
                    SaveProfiles(); // Resave in new format
                }
                catch
                {
                     // It might be the new format after all, so try that.
                     _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
                }
            }
            else
            {
                 _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đọc file profiles.json: {ex.Message}");
            _profiles = new List<Profile>();
        }
    }

    private void SaveProfiles()
    {
        try
        {
            string json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ProfilesFileName, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu file profiles.json: {ex.Message}");
        }
    }

    public List<string> GetProfiles()
    {
        return _profiles.Select(p => p.Name).ToList();
    }

    public Profile GetProfileDetails(string profileName)
    {
        return _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
    }

    public void AddProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName) || _profiles.Any(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
            return;

        _profiles.Add(new Profile { Name = profileName });
        SaveProfiles();
    }

    public void DeleteProfile(string profileName)
    {
        var profileToRemove = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profileToRemove != null)
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
             // If profile doesn't exist, create it.
             profileToUpdate = new Profile { Name = profileName };
            _profiles.Add(profileToUpdate);
        }

        profileToUpdate.Email = email;
        profileToUpdate.Password = password;
        SaveProfiles();
    }

    // --- Chrome Process Management ---

    public void OpenChrome(string profileName, string url = null)
    {
        if (string.IsNullOrWhiteSpace(_config.SelectedChromePath) || !File.Exists(_config.SelectedChromePath))
        {
            MessageBox.Show("Vui lòng chọn đường dẫn đến file chrome.exe hợp lệ trong file config.json.");
            return;
        }

        string arguments = $"--profile-directory={profileName}";
        if (!string.IsNullOrEmpty(url))
        {
            arguments += $" \"{url}\"";
        }

        try
        {
            Process.Start(_config.SelectedChromePath, arguments);
        }
        catch (Exception ex)
        { 
            MessageBox.Show($"Không thể mở Chrome: {ex.Message}");
        }
    }

    public void CloseAllChrome()
    {
        foreach (var process in Process.GetProcessesByName("chrome"))
        {
            try
            {
                if (!process.HasExited) process.Kill();
            }
            catch { }
        }
    }
}
