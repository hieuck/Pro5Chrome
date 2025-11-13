
import time
import sys
import os
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

def main():
    """
    Main function to handle Google login using Selenium.
    Expects 4 command-line arguments:
    1. user_data_path: The absolute path to the Chrome User Data directory.
    2. profile_directory: The name of the profile folder (e.g., "Default", "Profile 1").
    3. username: The Google account email.
    4. password: The Google account password.
    """
    # --- Get arguments from command line ---
    if len(sys.argv) != 5:
        print(f"Usage: python {sys.argv[0]} <user_data_path> <profile_directory> <username> <password>")
        sys.exit(1)

    user_data_path = sys.argv[1]
    profile_directory = sys.argv[2]
    USERNAME = sys.argv[3]
    PASSWORD = sys.argv[4]
    
    # URL for Google's sign-in page, specifically for post-login sync confirmation
    URL = "https://accounts.google.com/v3/signin/identifier?continue=https%3A%2F%2Faccounts.google.com%2Fsignin%2Fchrome%2Fsync%2Ffinish"

    # --- Configure Chrome Options to use a specific profile ---
    options = Options()
    options.add_argument(f"--user-data-dir={user_data_path}")
    options.add_argument(f"--profile-directory={profile_directory}")
    # Detach the browser from the script process, so it stays open
    options.add_experimental_option("detach", True)

    driver = None
    try:
        # Use webdriver-manager to automatically handle chromedriver
        from webdriver_manager.chrome import ChromeDriverManager
        service = ChromeService(ChromeDriverManager().install())
        driver = webdriver.Chrome(service=service, options=options)

        driver.get(URL)

        # Wait for the email input field, enter username, and proceed
        email_input = WebDriverWait(driver, 15).until(
            EC.visibility_of_element_located((By.XPATH, '//*[@id="identifierId"]'))
        )
        email_input.send_keys(USERNAME)
        email_input.send_keys(Keys.RETURN)

        # Wait for the password input field, enter password, and proceed
        password_input = WebDriverWait(driver, 15).until(
            EC.visibility_of_element_located((By.XPATH, '//div[@id="password"]//input[@name="Passwd" or @name="password"]'))
        )
        time.sleep(1) # A small delay can help evade bot detection
        password_input.send_keys(PASSWORD)
        password_input.send_keys(Keys.RETURN)
        
        print("Login process initiated. Please check the browser to complete any further steps (e.g., 2FA).")

    except Exception as e:
        print(f"An error occurred during the login process: {e}", file=sys.stderr)
        # The 'detach' option will leave the browser open for debugging even on failure.

if __name__ == "__main__":
    main()
