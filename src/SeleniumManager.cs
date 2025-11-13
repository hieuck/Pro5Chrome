
using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

public class SeleniumManager
{
    /// <summary>
    /// Attempts to log into a Google account using Selenium.
    /// Assumes the ChromeDriver is available in the execution path or managed by a package.
    /// </summary>
    /// <param name="email">The Google account email.</param>
    /// <param name="password">The Google account password.</param>
    public static void LoginGoogle(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Email và mật khẩu không được để trống.");
        }

        IWebDriver driver = null;
        try
        {
            // The NuGet package Selenium.WebDriver.ChromeDriver will automatically place
            // the correct chromedriver.exe in the build directory and the service will find it.
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-gpu"); // Recommended for stability
            options.AddArgument("--start-maximized");

            // We can now use the default service, which is much cleaner.
            driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl("https://accounts.google.com");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // Wait for the email field to be present and enter the email
            IWebElement emailField = wait.Until(EC.ElementIsVisible(By.Id("identifierId")));
            emailField.SendKeys(email);
            emailField.SendKeys(Keys.Enter);

            // Wait for the password field. Google often loads this on a new page.
            // Using a more robust locator that waits for the element to be clickable.
            IWebElement passwordField = wait.Until(EC.ElementToBeClickable(By.Name("Passwd")));
            passwordField.SendKeys(password);
            passwordField.SendKeys(Keys.Enter);
            
            // Wait for a URL that indicates a successful login. This is more reliable.
            wait.Until(d => d.Url.Contains("myaccount.google.com"));
            Console.WriteLine("Selenium login process completed successfully.");
        }
        catch (Exception ex)
        {
            // Rethrow the exception to be handled by the service layer
            throw new Exception($"Lỗi trong quá trình tự động hóa Selenium: {ex.Message}", ex);
        }
        finally
        {
            // Let the browser stay open for a few seconds to observe the result, then close.
            Thread.Sleep(5000);
            driver?.Quit();
        }
    }
}

// Helper class for WebDriverWait until conditions
public static class EC
{
    public static Func<IWebDriver, IWebElement> ElementIsVisible(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        };
    }
    
    public static Func<IWebDriver, IWebElement> ElementToBeClickable(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                if (element.Displayed && element.Enabled)
                {
                    return element;
                }
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        };
    }
}
