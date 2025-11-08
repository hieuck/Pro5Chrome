import os
import logging
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.chrome.options import Options as ChromeOptions
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

logger = logging.getLogger('Pro5Chrome')


def login_google_selenium(email: str, password: str, profile: str = None, use_chrome_path: str = None, chrome_folder: str = None):
    """Attempt to login using Selenium. This function encapsulates the behavior previously in main script.
    It will choose chromedriver in `chrome_folder` if present, otherwise use webdriver_manager.
    """
    chrome_options = ChromeOptions()

    # prefer explicit use_chrome_path if provided
    if chrome_folder:
        chromedriver_path = os.path.join(chrome_folder, 'chromedriver.exe')
        chrome_exe_path = os.path.join(chrome_folder, 'chrome.exe')
    else:
        chromedriver_path = None
        chrome_exe_path = None

    if chromedriver_path and os.path.isfile(chromedriver_path):
        if chrome_exe_path and os.path.isfile(chrome_exe_path):
            use_chrome_path = chrome_exe_path
        else:
            logger.error('chromedriver found but chrome.exe missing in chrome_folder')
            return
    else:
        # fall back to provided path
        pass

    if use_chrome_path and os.path.isfile(use_chrome_path):
        chrome_options.binary_location = use_chrome_path
    else:
        logger.error('Chrome executable not found for Selenium')
        return

    driver = None
    try:
        service = ChromeService(executable_path=chromedriver_path if chromedriver_path and os.path.isfile(chromedriver_path) else ChromeDriverManager().install())
        driver = webdriver.Chrome(service=service, options=chrome_options)
        driver.get('https://accounts.google.com')
        WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.ID, 'identifierId')))
        email_field = driver.find_element(By.ID, 'identifierId')
        email_field.send_keys(email)
        email_field.send_keys(Keys.RETURN)
        WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.NAME, 'password')))
        password_field = driver.find_element(By.NAME, 'password')
        password_field.send_keys(password)
        password_field.send_keys(Keys.RETURN)

        if 'myaccount.google.com' in driver.current_url:
            logger.info('Selenium login succeeded')
        else:
            logger.warning('Selenium login did not reach account page')
    except Exception as e:
        logger.exception(f'Selenium login error: {e}')
    finally:
        if driver:
            try:
                driver.quit()
            except Exception:
                pass


__all__ = ['login_google_selenium']
