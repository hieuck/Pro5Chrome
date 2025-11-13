
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// Quản lý các tác vụ tự động hóa trình duyệt bằng Selenium.
/// </summary>
public class AutomationManager
{
    /// <summary>
    /// Khởi tạo một AutomationManager mới.
    /// </summary>
    public AutomationManager()
    {
        // Hàm dựng trống vì manager này hoạt động như một dịch vụ.
    }

    /// <summary>
    /// Khởi chạy một cửa sổ Chrome cho một profile cụ thể và điều hướng đến một URL.
    /// Cửa sổ trình duyệt sẽ được để mở để người dùng tương tác.
    /// </summary>
    /// <param name="profileName">Tên thư mục của profile Chrome (ví dụ: "Profile 1", "Default").</param>
    /// <param name="userDataPath">Đường dẫn đầy đủ đến thư mục User Data của Chrome.</param>
    /// <param name="driverPath">Đường dẫn đến thư mục chứa chromedriver.exe.</param>
    /// <param name="url">URL để điều hướng đến.</param>
    /// <returns>Một Task đại diện cho hoạt động bất đồng bộ.</returns>
    public Task NavigateToUrlAsync(string profileName, string userDataPath, string driverPath, string url)
    {
        return Task.Run(() =>
        {
            ChromeDriver driver = null;
            try
            {
                if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(userDataPath))
                {
                    throw new ArgumentException("Tên profile và đường dẫn user data không được để trống.");
                }

                if (!Directory.Exists(userDataPath))
                {
                    throw new DirectoryNotFoundException($"Thư mục User Data không tồn tại: {userDataPath}");
                }
                
                var driverService = ChromeDriverService.CreateDefaultService(driverPath);
                driverService.HideCommandPromptWindow = true;

                var options = new ChromeOptions();
                options.AddArgument($"--user-data-dir={userDataPath}");
                options.AddArgument($"--profile-directory={profileName}");
                options.AddArgument("--start-maximized");
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                // Khởi chạy driver. Cửa sổ sẽ được để mở.
                driver = new ChromeDriver(driverService, options);

                // Điều hướng đến URL mục tiêu.
                driver.Navigate().GoToUrl(url);
            }
            catch (Exception)
            {
                // Nếu có lỗi, đóng trình duyệt (nếu nó đã được mở).
                driver?.Quit();
                // Ném lại lỗi để lớp UI có thể bắt và hiển thị cho người dùng.
                throw;
            }
        });
    }
}
