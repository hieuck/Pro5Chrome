
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;
using System.Threading.Tasks;

public class AutomationManager
{
    public async Task LoginToGoogleAsync(string profileName, string userDataPath, string driverPath, string email, string password)
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

            driver = new ChromeDriver(driverService, options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Navigate to Google login page
            driver.Navigate().GoToUrl("https://accounts.google.com/");

            // Find and enter email
            var emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type='email']")));
            emailInput.SendKeys(email);
            emailInput.SendKeys(OpenQA.Selenium.Keys.Enter);

            // Find and enter password
            var passwordInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type='password']")));
            await Task.Delay(1500); // Wait a bit for the UI to be ready for password
            passwordInput.SendKeys(password);
            passwordInput.SendKeys(OpenQA.Selenium.Keys.Enter);

            // Wait for potential 2FA or completion, but don't close the browser.
            // The user can take over from here.
            await Task.Delay(5000); 
        }
        catch (Exception ex)
        {
            // If an error occurs, quit the driver, but do not re-throw the exception to the UI
            // as the user can often resolve it manually (e.g., entering a captcha).
            driver?.Quit();
            throw new Exception("Lỗi tự động hóa. Có thể bạn cần xác minh thủ công (ví dụ: captcha hoặc 2FA). Chi tiết: " + ex.Message);
        }
    }
    
    // The NavigateToUrlAsync method from the previous step remains unchanged.
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

                driver = new ChromeDriver(driverService, options);

                driver.Navigate().GoToUrl(url);
            }
            catch (Exception)
            {
                driver?.Quit();
                throw;
            }
        });
    }
}
