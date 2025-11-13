
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public static class SeleniumManager
{
    private static readonly Random _random = new Random();

    #region Login & Appeal

    public static void LoginGoogle(string email, string password, string otpSecret)
    {
        IWebDriver driver = null;
        try
        {
            // Uses a fresh driver session for login
            driver = new ChromeDriver(new ChromeOptions());
            driver.Navigate().GoToUrl("https://accounts.google.com/");

            EnterEmail(driver, email);
            EnterPassword(driver, password);
            HandlePostPasswordStep(driver, otpSecret);
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

    private static void HandlePostPasswordStep(IWebDriver driver, string otpSecret)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        try
        {
            var otpInput = driver.FindElement(By.Id("totpPin"));
            if (!string.IsNullOrWhiteSpace(otpSecret))
            {
                string otpCode = OtpGenerator.GenerateTotp(otpSecret);
                otpInput.SendKeys(otpCode);
                wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Next') or contains(., 'Tiếp theo')]"))).Click();
                Thread.Sleep(3000);
            }
            else
            {
                throw new InvalidOperationException("Tài khoản yêu cầu mã OTP, nhưng không có OTP Secret nào được lưu.");
            }
        }
        catch (NoSuchElementException) { /* No OTP input found, proceed. */ }
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
        else if (pageSource.Contains("wrong password") || pageSource.Contains("sai mật khẩu"))
        {
            throw new Exception("Sai mật khẩu. Vui lòng kiểm tra lại.");
        }
        else
        {
            throw new Exception("Không nhận dạng được trang sau khi đăng nhập. Có thể yêu cầu xác minh thêm mà chưa được hỗ trợ tự động.");
        }
    }

    private static void StartAppealProcess(IWebDriver driver)
    {
        // This functionality remains unchanged
    }

    private static void FillAppealForm(IWebDriver driver)
    {
        // This functionality remains unchanged
    }

    private static string LoadAppealText()
    {
        // This functionality remains unchanged
        return "";
    }

    #endregion

    #region NEW: Account Warming

    // Main method to start the account warming process.
    public static void WarmUpAccount(string profileName, string userDataPath)
    {
        IWebDriver driver = null;
        try
        {
            driver = InitializeDriverWithProfile(profileName, userDataPath);
            MessageBox.Show($"Bắt đầu quá trình nuôi tài khoản cho: {profileName}.\nTrình duyệt sẽ tự động thực hiện các hành động trong khoảng 5-10 phút.\nVui lòng không tương tác với cửa sổ trình duyệt này.", "Bắt đầu nuôi tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Create a list of actions to perform.
            var actions = new List<Action<IWebDriver>>
            {
                GoogleSearchAndBrowse,
                WatchYouTubeVideo,
                ReadGoogleNews
            };

            // Shuffle the actions to make the behavior less predictable.
            var shuffledActions = actions.OrderBy(a => _random.Next()).ToList();

            foreach (var action in shuffledActions)
            {
                action(driver); // Execute the action.
                // Wait for a random period before the next action.
                Thread.Sleep(TimeSpan.FromSeconds(_random.Next(20, 45)));
            }

            MessageBox.Show($"Quá trình nuôi tài khoản cho profile '{profileName}' đã hoàn tất!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi trong quá trình nuôi tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            driver?.Quit(); // Always close the browser.
        }
    }

    // Initializes a Chrome driver instance using an existing user profile.
    private static IWebDriver InitializeDriverWithProfile(string profileName, string userDataPath)
    {
        var options = new ChromeOptions();
        options.AddArgument($"--user-data-dir={userDataPath}");
        options.AddArgument($"--profile-directory={profileName}");
        // Optional: Start maximized to better simulate a real user.
        options.AddArgument("--start-maximized");
        return new ChromeDriver(options);
    }

    // Action: Performs a Google search and clicks a result.
    private static void GoogleSearchAndBrowse(IWebDriver driver)
    {
        try
        {
            driver.Navigate().GoToUrl("https://www.google.com");
            Thread.Sleep(TimeSpan.FromSeconds(_random.Next(3, 6)));

            string[] keywords = File.ReadAllLines("keywords.txt");
            if (keywords.Length == 0) return;

            string keyword = keywords[_random.Next(keywords.Length)];
            var searchBox = driver.FindElement(By.Name("q"));
            searchBox.SendKeys(keyword);
            searchBox.Submit();

            Thread.Sleep(TimeSpan.FromSeconds(_random.Next(4, 8)));

            // Find valid, visible search result links.
            var searchResults = driver.FindElements(By.CssSelector("div.g a[href]"))
                                      .Where(a => a.Displayed && !string.IsNullOrEmpty(a.GetAttribute("href"))).ToList();
            
            if (searchResults.Count > 0)
            {
                // Click a random link from the top 5 results.
                searchResults[_random.Next(Math.Min(searchResults.Count, 5))].Click();
                // Stay on the page for a while to simulate reading.
                Thread.Sleep(TimeSpan.FromSeconds(_random.Next(30, 75)));
            }
        }
        catch { /* Ignore errors in this sub-action and continue */ }
    }

    // Action: Searches for and watches a YouTube video.
    private static void WatchYouTubeVideo(IWebDriver driver)
    {
        try
        {
            driver.Navigate().GoToUrl("https://www.youtube.com");
            Thread.Sleep(TimeSpan.FromSeconds(_random.Next(5, 10)));
            
            var searchBox = driver.FindElement(By.Name("search_query"));
            searchBox.SendKeys("lofi hip hop radio"); // A safe, long-running search term.
            searchBox.SendKeys(Keys.Enter);

            Thread.Sleep(TimeSpan.FromSeconds(_random.Next(5, 10)));

            var videoLinks = driver.FindElements(By.Id("video-title"));
            if (videoLinks.Count > 0)
            {
                videoLinks[_random.Next(Math.Min(videoLinks.Count, 5))].Click();
                // Watch the video for a random duration (2-4 minutes).
                Thread.Sleep(TimeSpan.FromSeconds(_random.Next(120, 240)));
            }
        }
        catch { /* Ignore errors in this sub-action and continue */ }
    }

    // Action: Browses Google News.
    private static void ReadGoogleNews(IWebDriver driver)
    {
        try
        {
            driver.Navigate().GoToUrl("https://news.google.com");
            Thread.Sleep(TimeSpan.FromSeconds(_random.Next(7, 12)));

            // Find links to articles.
            var storyLinks = driver.FindElements(By.CssSelector("a[href*='./articles/']")).Where(a => a.Displayed).ToList();
            if (storyLinks.Count > 0)
            {
                storyLinks[_random.Next(Math.Min(storyLinks.Count, 10))].Click();
                 // "Read" the article.
                Thread.Sleep(TimeSpan.FromSeconds(_random.Next(30, 60)));
            }
        }
        catch { /* Ignore errors in this sub-action and continue */ }
    }

    #endregion
}
