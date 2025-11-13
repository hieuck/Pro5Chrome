
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;

public class AutomationManager
{
    private readonly Profile _profile;
    private readonly string _chromeExecutablePath;
    private readonly string _userDataPath;

    public AutomationManager(Profile profile, string chromeExecutablePath, string userDataPath)
    {
        _profile = profile;
        _chromeExecutablePath = chromeExecutablePath;
        _userDataPath = userDataPath;
    }

    public void RunAutoLogin()
    {
        if (string.IsNullOrEmpty(_profile.Email) || string.IsNullOrEmpty(_profile.Password))
        {
            MessageBox.Show($"Email hoặc mật khẩu chưa được thiết lập cho profile '{_profile.Name}'.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ChromeDriver driver = null;
        try
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.BinaryLocation = _chromeExecutablePath;
            options.AddArgument($"--user-data-dir={_userDataPath}");
            options.AddArgument($"--profile-directory={_profile.Name}");
            options.AddArgument("--start-maximized");
            options.AddExcludedArgument("enable-automation"); // Giúp tránh bị một số trang web phát hiện

            driver = new ChromeDriver(driverService, options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            driver.Navigate().GoToUrl(Pro5ChromeManager.GoogleLoginUrl);

            // --- Step 1: Enter Email ---
            var emailInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='email']")));
            emailInput.SendKeys(_profile.Email);
            driver.FindElement(By.XPath("//*[@id='identifierNext']//button")).Click();

            // --- Step 2: Enter Password ---
            var passwordInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='password']")));
            passwordInput.SendKeys(_profile.Password);
            driver.FindElement(By.XPath("//*[@id='passwordNext']//button")).Click();
            
            // --- Step 3: Handle 2FA/OTP (Two-Factor Authentication) ---
            // Future implementation: Check if an element related to 2FA appears.
            // For now, the process will stop here, and the user can manually enter the OTP
            // in the browser window that Selenium is controlling.

            // The browser will remain open for manual intervention or further steps.
            // We don't call driver.Quit() here so the user can continue the session.
        }
        catch (WebDriverTimeoutException)
        {
             MessageBox.Show($"Thao tác vượt quá thời gian chờ. Trang web có thể đã thay đổi hoặc tải quá chậm.\nKiểm tra lại trang đăng nhập Google và đảm bảo các phần tử (email, password) vẫn còn tồn tại.", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
             driver?.Quit();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi trong quá trình tự động hóa: {ex.Message}", "Lỗi Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            driver?.Quit(); // Close the browser on error
        }
    }
}
