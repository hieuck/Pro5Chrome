
using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class SeleniumManager
{
    /// <summary>
    /// Attempts to log into a Google account using Selenium.
    /// </summary>
    /// <param name="email">The Google account email.</param>
    /// <param name="password">The Google account password.</param>
    /// <param name="chromeDriverPath">The absolute path to chromedriver.exe.</param>
    /// <param name="chromeExePath">Optional. The path to the Chrome browser executable.</param>
    public static void LoginGoogle(string email, string password, string chromeDriverPath, string chromeExePath = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Email and password cannot be empty.");
            return;
        }

        if (!File.Exists(chromeDriverPath))
        {
            Console.WriteLine($"ChromeDriver not found at: {chromeDriverPath}");
            // In a real app, you might want to throw an exception here
            return;
        }
        
        IWebDriver driver = null;
        try
        {
            ChromeOptions options = new ChromeOptions();
            if (!string.IsNullOrEmpty(chromeExePath) && File.Exists(chromeExePath))
            {
                options.BinaryLocation = chromeExePath;
            }

            // The directory containing chromedriver.exe is passed to the service
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(chromeDriverPath));
            driver = new ChromeDriver(service, options);

            driver.Navigate().GoToUrl("https://accounts.google.com");

            // Wait for the email field to be present and enter the email
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement emailField = wait.Until(d => d.FindElement(By.Id("identifierId")));
            emailField.SendKeys(email);
            emailField.SendKeys(Keys.Enter);

            // Wait for the password field to be present and enter the password
            // Note: Google's password field might have a different identifier after the page transition
            IWebElement passwordField = wait.Until(d => d.FindElement(By.Name("Passwd"))); // Google uses "Passwd"
            passwordField.SendKeys(password);
            passwordField.SendKeys(Keys.Enter);
            
            // A simple check to see if login was likely successful
            // Wait for a URL that indicates a successful login. This is more reliable than just sleeping.
            wait.Until(d => d.Url.Contains("myaccount.google.com"));
            Console.WriteLine("Selenium login process completed.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during Selenium login: {ex.Message}");
            // In a real app, you would log this exception
        }
        finally
        {
            // Close the browser
            driver?.Quit();
        }
    }
}
