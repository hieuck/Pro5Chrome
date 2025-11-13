
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System; 
using System.IO;
using System.Threading;
using System.Windows.Forms;

// Manages all Selenium-based browser automation.
public static class SeleniumManager
{
    // Main method to automate Google login and handle disabled account appeals.
    public static void LoginGoogle(string email, string password)
    {
        IWebDriver driver = null;
        try
        {
            driver = InitializeDriver();
            driver.Navigate().GoToUrl("https://accounts.google.com/");

            // 1. Enter Email
            EnterEmail(driver, email);

            // 2. Enter Password
            EnterPassword(driver, password);

            // 3. Check the outcome of the login attempt
            CheckLoginStatusAndAppealIfNeeded(driver);
        }
        catch (Exception ex)
        {
            // Throw a more specific exception to be caught by the calling manager
            throw new Exception($"Selenium automation failed. Details: {ex.Message}", ex);
        }
        finally
        {
            // Ensure the browser closes even if errors occur
            driver?.Quit();
        }
    }

    // Sets up the ChromeDriver with necessary options.
    private static IWebDriver InitializeDriver()
    {
        var options = new ChromeOptions();
        // The 'Selenium.WebDriver.ChromeDriver' NuGet package automatically places
        // the correct chromedriver.exe in the build output directory.
        // No need to specify the driver path manually.
        return new ChromeDriver(options);
    }

    // Enters the email address and proceeds.
    private static void EnterEmail(IWebDriver driver, string email)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var emailInput = wait.Until(d => d.FindElement(By.Id("identifierId")));
        emailInput.SendKeys(email);
        driver.FindElement(By.Id("identifierNext")).Click();
        Thread.Sleep(2000); // Wait for the next page to load
    }

    // Enters the password and proceeds.
    private static void EnterPassword(IWebDriver driver, string password)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var passwordInput = wait.Until(d => d.FindElement(By.Name("Passwd"))); // Changed from "password" to "Passwd"
        passwordInput.SendKeys(password);
        // Using a more reliable way to find the 'Next' button
        wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Next') or contains(., 'Tiếp theo')]"))).Click();
        Thread.Sleep(3000); // Wait for post-login page evaluation
    }

    // Checks the current page to see if login was successful or if the account is disabled.
    private static void CheckLoginStatusAndAppealIfNeeded(IWebDriver driver)
    {
        string currentUrl = driver.Url;
        var pageSource = driver.PageSource.ToLower();

        // Scenario 1: Login successful
        if (currentUrl.Contains("myaccount.google.com"))
        {
            MessageBox.Show("Đăng nhập thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Scenario 2: Account is disabled
        if (pageSource.Contains("tài khoản của bạn đã bị vô hiệu hóa") || pageSource.Contains("account has been disabled"))
        {
            MessageBox.Show("Tài khoản bị vô hiệu hóa. Bắt đầu quá trình kháng nghị tự động.", "Phát hiện Vô hiệu hóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            StartAppealProcess(driver);
        }
        else
        {
            // Scenario 3: Other unexpected page (e.g., 2FA, incorrect password)
            throw new Exception("Không nhận dạng được trang sau khi đăng nhập. Vui lòng kiểm tra thủ công.");
        }
    }

    // Navigates the appeal form.
    private static void StartAppealProcess(IWebDriver driver)
    {
        try
        {
            // Find and click the "Try to restore" or "Thử khôi phục" button/link
            var appealButton = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.FindElement(By.XPath("//button[contains(., 'Try to restore') or contains(., 'Thử khôi phục')] | //a[contains(., 'Try to restore') or contains(., 'Thử khôi phục')]")));
            appealButton.Click();
            Thread.Sleep(2000);

            // Now on the appeal form, fill in the reason
            FillAppealForm(driver);
        }
        catch (Exception ex)
        { 
            throw new Exception("Không tìm thấy nút để bắt đầu quá trình kháng nghị. " + ex.Message);
        }
    }

    // Fills the appeal reason textbox.
    private static void FillAppealForm(IWebDriver driver)
    {
        try
        {
            string appealText = LoadAppealText();
            if (string.IsNullOrEmpty(appealText))
            {
                throw new InvalidOperationException("Không thể đọc được nội dung từ file Kháng.txt.");
            }

            // The textbox for the appeal reason often has a specific aria-label or name.
            // This XPath is more robust, looking for a textarea.
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var reasonTextBox = wait.Until(d => d.FindElement(By.TagName("textarea")));

            reasonTextBox.SendKeys(appealText);

            // At this point, the user needs to manually complete the process (e.g., Captcha)
            MessageBox.Show("Đã tự động điền đơn kháng nghị. Vui lòng hoàn tất các bước còn lại (ví dụ: Captcha) và tự nhấn gửi.", "Hoàn tất Tự động hóa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        { 
            throw new Exception("Lỗi khi điền đơn kháng nghị: " + ex.Message);
        }
    }

    // Loads the Vietnamese appeal text from Khang.txt.
    private static string LoadAppealText()
    {
        const string fileName = "Kháng.txt";
        if (!File.Exists(fileName)) return null;

        try
        {
            string[] lines = File.ReadAllLines(fileName);
            bool isVietnameseSection = false;
            string appealContent = "";

            foreach (var line in lines)
            {
                if (line.Trim().Equals("Tiếng Việt:", StringComparison.OrdinalIgnoreCase))
                {
                    isVietnameseSection = true;
                    continue;
                }
                if (line.Trim().StartsWith("Make sure to include", StringComparison.OrdinalIgnoreCase))
                {
                    break; // Stop before the footer
                }
                if (isVietnameseSection && !string.IsNullOrWhiteSpace(line) && !line.Contains("Kính gửi") && !line.Contains("Trân trọng") && !line.Contains("[Tên của bạn]") && !line.Contains("[Địa chỉ email của bạn]"))
                {
                    appealContent += line.Trim() + " ";
                }
            }
            return appealContent.Trim();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đọc file {fileName}: {ex.Message}");
            return null;
        }
    }
}
