
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public class UrlManager
{
    private List<string> _urls = new List<string>();
    private const string URL_FILE = "urls.json";

    public UrlManager()
    {
        // Constructor is fine, LoadUrls will be called from MainForm
    }

    public void LoadUrls()
    {
        if (File.Exists(URL_FILE))
        {
            try
            {
                string json = File.ReadAllText(URL_FILE);
                if (string.IsNullOrWhiteSpace(json)){
                     _urls = new List<string>();
                     return;
                }
                _urls = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Tệp 'URL.json' có định dạng không hợp lệ và sẽ được tạo lại.\nLỗi: {ex.Message}", "Lỗi Phân tích JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _urls = new List<string>();
                SaveUrls(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi đọc tệp 'URL.json'.\nLỗi: {ex.Message}", "Lỗi Đọc File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _urls = new List<string>();
            }
        }
        else
        {
            _urls = new List<string>();
            SaveUrls(); 
        }
    }

    private void SaveUrls()
    {
        try
        {
            string json = JsonSerializer.Serialize(_urls, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(URL_FILE, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi khi lưu tệp 'URL.json'.\nLỗi: {ex.Message}", "Lỗi Ghi File", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public List<string> GetUrls()
    {
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

    public void RemoveUrl(string url)
    {
        if (_urls.Contains(url))
        {
            _urls.Remove(url);
            SaveUrls();
        }
    }

    public void ClearUrls()
    {
        _urls.Clear();
        SaveUrls();
    }
}
