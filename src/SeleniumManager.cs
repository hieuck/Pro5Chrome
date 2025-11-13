
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

public static class SeleniumManager
{
    // Updated to accept an OTP secret key.
    public static void LoginGoogle(string email, string password, string otpSecret)
    {
        IWebDriver driver = null;
        try
        {
            driver = InitializeDriver();
            driver.Navigate().GoToUrl("https://accounts.google.com/");

            EnterEmail(driver, email);
            EnterPassword(driver, password);
            
            // Check the post-password page and handle OTP if required.
            HandlePostPasswordStep(driver, otpSecret);

            // Final check for success or disabled account.
            CheckLoginStatusAndAppealIfNeeded(driver);
        }
        catch (Exception ex)
        {
            throw new Exception($"Selenium automation failed. Details: {ex.Message}", ex);
        }
        finally
        {
            driver?.Quit();
        }
    }

    private static IWebDriver InitializeDriver()
    {
        return new ChromeDriver(new ChromeOptions());
    }

    private static void EnterEmail(IWebDriver driver, string email)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var emailInput = wait.Until(d => d.FindElement(By.Id("identifierId")));
        emailInput.SendKeys(email);
        driver.FindElement(By.Id("identifierNext")).Click();
        Thread.Sleep(2000);
    }

    private static void EnterPassword(IWebDriver driver, string password)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var passwordInput = wait.Until(d => d.FindElement(By.Name("Passwd")));
        passwordInput.SendKeys(password);
        wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Next') or contains(., 'Tiếp theo')]"))).Click();
        Thread.Sleep(3000);
    }

    // NEW: Determines if an OTP is needed and enters it.
    private static void HandlePostPasswordStep(IWebDriver driver, string otpSecret)
    {
        // Check if the page is asking for an OTP.
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5)); // Shorter wait
        try
        {
            // Look for the OTP input field.
            var otpInput = driver.FindElement(By.Id("totpPin"));

            // If we found the input, an OTP is required.
            if (!string.IsNullOrWhiteSpace(otpSecret))
            {
                string otpCode = OtpGenerator.GenerateTotp(otpSecret);
                otpInput.SendKeys(otpCode);
                wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Next') or contains(., 'Tiếp theo')]"))).Click();
                Thread.Sleep(3000);
            }
            else
            {
                // OTP is required, but we don't have a secret key.
                throw new InvalidOperationException("Tài khoản yêu cầu mã OTP, nhưng không có OTP Secret nào được lưu.");
            }
        }
        catch (NoSuchElementException)
        {
            // No OTP input found, so we are likely on another page (success, disabled, etc.)
            // Do nothing and proceed to the next check.
        }
    }

    private static void CheckLoginStatusAndAppealIfNeeded(IWebDriver driver)
    {
        string currentUrl = driver.Url;
        var pageSource = driver.PageSource.ToLower();

        if (currentUrl.Contains("myaccount.google.com"))
        {
            MessageBox.Show("Đăng nhập thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (pageSource.Contains("tài khoản của bạn đã bị vô hiệu hóa") || pageSource.Contains("account has been disabled"))
        {
            MessageBox.Show("Tài khoản bị vô hiệu hóa. Bắt đầu quá trình kháng nghị tự động.", "Phát hiện Vô hiệu hóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            StartAppealProcess(driver);
        }
        else
        {
            // Add a check for wrong password
            if (pageSource.Contains("wrong password") || pageSource.Contains("sai mật khẩu"))
            {
                 throw new Exception("Sai mật khẩu. Vui lòng kiểm tra lại.");
            }
            // If it's not success, disabled, or wrong password, it's an unknown state.
            throw new Exception("Không nhận dạng được trang sau khi đăng nhập. Có thể yêu cầu xác minh thêm mà chưa được hỗ trợ tự động.");
        }
    }

    private static void StartAppealProcess(IWebDriver driver)
    {
        // ... (This part remains the same) ...
        try
        {
            var appealButton = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.FindElement(By.XPath("//button[contains(., 'Try to restore') or contains(., 'Thử khôi phục')] | //a[contains(., 'Try to restore') or contains(., 'Thử khôi phục')]")));
            appealButton.Click();
            Thread.Sleep(2000);
            FillAppealForm(driver);
        }
        catch (Exception ex)
        { 
            throw new Exception("Không tìm thấy nút để bắt đầu quá trình kháng nghị. " + ex.Message);
        }
    }

    private static void FillAppealForm(IWebDriver driver)
    {
        // ... (This part remains the same) ...
        try
        {
            string appealText = LoadAppealText();
            if (string.IsNullOrEmpty(appealText))
            {
                throw new InvalidOperationException("Không thể đọc được nội dung từ file Kháng.txt.");
            }
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var reasonTextBox = wait.Until(d => d.FindElement(By.TagName("textarea")));
            reasonTextBox.SendKeys(appealText);
            MessageBox.Show("Đã tự động điền đơn kháng nghị. Vui lòng hoàn tất các bước còn lại (ví dụ: Captcha) và tự nhấn gửi.", "Hoàn tất Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        { 
            throw new Exception("Lỗi khi điền đơn kháng nghị: " + ex.Message);
        }
    }

    private static string LoadAppealText()
    {
        // ... (This part remains the same) ...
        const string fileName = "Kháng.txt";
        if (!File.Exists(fileName)) return null;
        try
        {
            string[] lines = File.ReadAllLines(fileName);
            bool isVietnameseSection = false;
            string appealContent = "";
            foreach (var line in lines)
            {
                if (line.Trim().Equals("Tiếng Việt:", StringComparison.OrdinalIgnoreCase)) isVietnameseSection = true;
                if (line.Trim().StartsWith("Make sure to include", StringComparison.OrdinalIgnoreCase)) break;
                if (isVietnameseSection && !string.IsNullOrWhiteSpace(line) && !line.Contains("Kính gửi") && !line.Contains("Trân trọng") && !line.Contains("[Tên của bạn]") && !line.Contains("[Địa chỉ email của bạn]"))
                {
                    appealContent += line.Trim() + " ";
                }
            }
            return appealContent.Trim();
        }
        catch { return null; }
    }
}
