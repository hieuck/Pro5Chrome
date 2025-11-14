
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

/// <summary>
/// Provides functionality to generate Time-based One-Time Passwords (TOTP).
/// </summary>
public static class OtpGenerator
{
    public static string GenerateOtp(string secretKey)
    {
        // Implementation of TOTP generation logic would go here.
        // This is a placeholder for the actual logic.
        // In a real application, you would use a library like Otp.NET.
        // For now, let's return a mock value for testing purposes.
        // NOTE: This will not work for actual 2FA. A real implementation is required.
        Console.WriteLine("Warning: Using placeholder OTP generation. This is not secure and will not work for real accounts.");
        return new Random().Next(100000, 999999).ToString("D6");
    }
}

public static class SeleniumManager
{
    private static ChromeDriverService GetDriverService()
    {
        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        return service;
    }

    private static ChromeOptions GetBaseChromeOptions(string chromeExecutablePath, string userDataDir, string profileName)
    {
        if (string.IsNullOrEmpty(chromeExecutablePath) || !File.Exists(chromeExecutablePath))
        {
            throw new ArgumentException($"Đường dẫn Chrome không hợp lệ: '{chromeExecutablePath}'");
        }

        var options = new ChromeOptions();
        options.BinaryLocation = chromeExecutablePath;
        options.AddArgument($"--user-data-dir={userDataDir}");
        options.AddArgument($"--profile-directory={profileName}");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddArgument("start-maximized");

        return options;
    }

    private static IWebElement FindElementSafe(IWebDriver driver, By locator, int timeoutSeconds = 3)
    {
        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d => {
                var element = d.FindElement(locator);
                return (element.Displayed && element.Enabled) ? element : null;
            });
        }
        catch (WebDriverTimeoutException)
        { 
            return null; 
        }
    }

    private static string ReadAppealText()
    {
        string appealFilePath = "Kháng.txt";
        if (File.Exists(appealFilePath))
        {
            return File.ReadAllText(appealFilePath, Encoding.UTF8);
        }
        return "Lỗi: Không tìm thấy tệp Kháng.txt.";
    }

    public static void LoginGoogle(string chromeExecutablePath, string userDataDir, string profileName, string email, string password, string otpSecret)
    {
        IWebDriver driver = null;
        try
        {
            var options = GetBaseChromeOptions(chromeExecutablePath, userDataDir, profileName);
            var service = GetDriverService();
            driver = new ChromeDriver(service, options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl("https://accounts.google.com");

            // Step 1: Handle Email/Phone input
            var emailInput = FindElementSafe(driver, By.XPath("//input[@type='email']"));
            if (emailInput == null) { /* Already logged in or different page */ } 
            else
            {
                emailInput.SendKeys(email);
                emailInput.SendKeys(Keys.Enter);
            }
            
            Thread.Sleep(2000); // Wait for password page to load

            // Step 2: Handle Password input
            var passwordInput = FindElementSafe(driver, By.XPath("//input[@type='password']"));
            if (passwordInput != null)
            {
                passwordInput.SendKeys(password);
                passwordInput.SendKeys(Keys.Enter);
            }

            Thread.Sleep(5000); // Crucial wait for post-login page

            // Step 3: Check for Appeal Page
            var appealTextArea = FindElementSafe(driver, By.TagName("textarea"), 5);
            if (appealTextArea != null)
            {
                Console.WriteLine("Phát hiện trang kháng nghị. Điền nội dung từ Kháng.txt");
                appealTextArea.SendKeys(ReadAppealText());
                var submitButton = FindElementSafe(driver, By.XPath("//button[contains(., 'Gửi') or contains(., 'Submit')]"));
                submitButton?.Click();
                Console.WriteLine("Đã gửi kháng nghị.");
                return; // End of process, leave browser open
            }

            // Step 4: Check for OTP Page
            var otpInput = FindElementSafe(driver, By.Id("idvPin"), 5) ?? FindElementSafe(driver, By.Name("totp"), 5);
            if (otpInput != null)
            {
                if (string.IsNullOrWhiteSpace(otpSecret)) { Console.WriteLine("Yêu cầu OTP nhưng không có OtpSecret."); return; }
                string otpCode = OtpGenerator.GenerateOtp(otpSecret);
                Console.WriteLine($"Điền mã OTP: {otpCode}");
                otpInput.SendKeys(otpCode);
                otpInput.SendKeys(Keys.Enter);
                return; // End of process
            }
            
            Console.WriteLine("Đăng nhập thành công hoặc gặp trang không xác định.");
        }
        catch (Exception e)
        {
            throw new Exception($"Lỗi Selenium: {e.Message}", e);
        }
        // Do NOT close the driver here. Let the user manage the window.
    }

    public static void WarmUpAccount(string chromeExecutablePath, string userDataDir, string profileName)
    {
        IWebDriver driver = null;
        try
        {
            var options = GetBaseChromeOptions(chromeExecutablePath, userDataDir, profileName);
            var service = GetDriverService();
            driver = new ChromeDriver(service, options);
            var js = (IJavaScriptExecutor)driver;
            var random = new Random();

            var keywords = File.Exists("keywords.txt") ? File.ReadAllLines("keywords.txt", Encoding.UTF8) : new string[] { "tin tức việt nam", "công nghệ mới" };
            if (!keywords.Any()) { Console.WriteLine("keywords.txt rỗng."); return; }

            // Take 2 random keywords to search
            for(int i = 0; i < 2; i++)
            {
                string keyword = keywords[random.Next(keywords.Length)];
                Console.WriteLine($"Đang tìm kiếm với từ khóa: '{keyword}'");
                driver.Navigate().GoToUrl("https://www.google.com");
                var searchBox = FindElementSafe(driver, By.Name("q"));
                searchBox.SendKeys(keyword);
                searchBox.SendKeys(Keys.Enter);
                Thread.Sleep(3000);

                // Find search results (excluding ads)
                var searchResults = driver.FindElements(By.XPath("//div[@id='search']//div[@class='g']//a[@href]"));
                if (searchResults.Any())
                {
                    var resultToClick = searchResults[random.Next(Math.Min(5, searchResults.Count))]; // Click one of top 5
                    string url = resultToClick.GetAttribute("href");
                    Console.WriteLine($"Click vào kết quả: {url}");
                    js.ExecuteScript("arguments[0].click();", resultToClick); // Use JS click to be safer

                    Thread.Sleep(random.Next(8000, 15000)); // Stay on page for 8-15s
                    Console.WriteLine("Đã dành thời gian trên trang.");
                }
            }

            Console.WriteLine("Hoàn tất quá trình nuôi tài khoản.");
        }
        catch (Exception e)
        {
            throw new Exception($"Lỗi khi nuôi tài khoản: {e.Message}", e);
        }
        // Do NOT close the driver here.
    }
}
