
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class Pro5ChromeManager
{
    // --- Constants ---
    private const string CONFIG_FILE = "config.json";
    private const string PROFILES_FILE = "profiles.json";
    public const string GoogleLoginUrl = "https://accounts.google.com/";

    // --- Private Fields ---
    private string _chromePath;
    private List<string> _profiles;

    // --- Constructor ---
    public Pro5ChromeManager()
    {
        LoadConfig();
        LoadProfiles();
    }

    // --- Configuration Management ---
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

    // --- Profile Management ---
    private void LoadProfiles()
    {
        if (File.Exists(PROFILES_FILE))
        {
            _profiles = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(PROFILES_FILE));
        }
        else
        {
            _profiles = new List<string>();
        }
    }

    private void SaveProfiles()
    {
        File.WriteAllText(PROFILES_FILE, JsonConvert.SerializeObject(_profiles, Formatting.Indented));
    }

    public List<string> GetProfiles()
    {
        return _profiles.ToList(); // Return a copy
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
    
    public static string GetUserDataPath()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Google", "Chrome", "User Data");
    }

    // --- Chrome Process Management ---
    public void OpenChrome(string profileName, string url = null)
    {
        AddProfile(profileName); // Ensure profile exists in our list before opening
        
        string arguments = $"--profile-directory={profileName}";
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
            try
            {
                process.Kill();
            }
            catch (Exception) // Now the compiler can find Exception
            {
                // Ignore errors if the process has already exited
            }
        }
    }
}
