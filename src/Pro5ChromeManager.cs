
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Defines the structure for the config.json file.
/// </summary>
public class ChromeConfig
{
    [JsonProperty("selected_chrome_path")]
    public string SelectedChromePath { get; set; }

    [JsonProperty("chrome_paths")]
    public List<string> ChromePaths { get; set; }

    public ChromeConfig()
    {
        ChromePaths = new List<string>();
    }
}

public class Pro5ChromeManager
{
    // --- Constants ---
    private const string CONFIG_FILE = "config.json";
    private const string PROFILES_FILE = "profiles.json";
    public const string GoogleLoginUrl = "https://accounts.google.com/";
    private const string DefaultChromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

    // --- Private Fields ---
    private ChromeConfig _config;
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
            try
            {
                _config = JsonConvert.DeserializeObject<ChromeConfig>(File.ReadAllText(CONFIG_FILE));
                // Handle case where file exists but is empty or invalid
                if (_config == null || _config.ChromePaths == null)
                {
                    InitializeDefaultConfig();
                }
                // Ensure there's always at least one path
                if (_config.ChromePaths.Count == 0)
                {
                     _config.ChromePaths.Add(DefaultChromePath);
                }
                // Ensure a path is selected
                if (string.IsNullOrWhiteSpace(_config.SelectedChromePath) || !_config.ChromePaths.Contains(_config.SelectedChromePath))
                {
                    _config.SelectedChromePath = _config.ChromePaths[0];
                }
            }
            catch (JsonException)
            {
                // If parsing fails (e.g., old format), create a new config
                InitializeDefaultConfig();
            }
        }
        else
        {
            InitializeDefaultConfig();
        }
        SaveConfig(); // Save to ensure file is created and format is updated
    }

    private void InitializeDefaultConfig()
    {
        _config = new ChromeConfig();
        _config.ChromePaths.Add(DefaultChromePath);
        _config.SelectedChromePath = DefaultChromePath;
    }

    private void SaveConfig()
    {
        File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(_config, Formatting.Indented));
    }

    public List<string> GetChromePaths() => _config.ChromePaths.ToList();
    public string GetSelectedChromePath() => _config.SelectedChromePath;

    public void AddAndSelectChromePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
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
            // If the deleted path was the selected one, select another one
            if (_config.SelectedChromePath == path)
            {
                _config.SelectedChromePath = _config.ChromePaths.FirstOrDefault() ?? string.Empty;
            }
            SaveConfig();
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
            FileName = _config.SelectedChromePath, // Use the selected path
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
            catch (Exception)
            {
                // Ignore errors if the process has already exited
            }
        }
    }
}
