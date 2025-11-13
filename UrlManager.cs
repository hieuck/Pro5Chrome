
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class UrlManager
{
    private List<string> _urls;
    private const string URL_FILE = "URL.json";

    public UrlManager()
    {
        LoadUrls();
    }

    private void LoadUrls()
    {
        if (File.Exists(URL_FILE))
        {
            string json = File.ReadAllText(URL_FILE);
            _urls = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
        else
        {
            _urls = new List<string>();
            File.WriteAllText(URL_FILE, "[]"); // Create the file if it doesn't exist
        }
    }

    private void SaveUrls()
    {
        string json = JsonConvert.SerializeObject(_urls, Formatting.Indented);
        File.WriteAllText(URL_FILE, json);
    }

    public List<string> GetUrls()
    {
        LoadUrls();
        return _urls;
    }

    public void AddUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url) && !_urls.Contains(url))
        {
            _urls.Add(url);
            SaveUrls();
        }
    }

    public void DeleteUrl(string url)
    {
        if (_urls.Contains(url))
        {
            _urls.Remove(url);
            SaveUrls();
        }
    }

    public void ClearAllUrls()
    {
        _urls.Clear();
        SaveUrls();
    }
}
