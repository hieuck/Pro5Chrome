import time
from selenium import webdriver
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

# Thông tin đăng nhập của bạn
USERNAME = "your_email@gmail.com"
PASSWORD = "your_password"

# URL của trang đăng nhập Google
URL = "https://accounts.google.com/v3/signin/identifier?continue=https%3A%2F%2Faccounts.google.com%2Fsignin%2Fchrome%2Fsync%2Ffinish%3Fcontinue%3Dhttps%253A%252F%252Fwww.google.com%252F%26est%3DAHl3n5DgFpLj1E0fVr05ZIpspzqwg9IdyrTIBI3cSV93wZ8-3RNxIpAPYqfCrR-P7a0gep0mMWkF2QXubPBSPw&ssp=1&flowName=GlifDesktopChromeSync&ddm=0&dsh=S-117083741%3A1719881167251923"

# Khởi động trình duyệt Chrome
driver = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()))

try:
    # Mở trang đăng nhập Google
    driver.get(URL)

    # Nhập email hoặc số điện thoại
    email_input = WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.XPATH, '//*[@id="identifierId"]'))
    )
    email_input.send_keys(USERNAME)
    email_input.send_keys(Keys.RETURN)

    # Chờ trang mật khẩu tải
    password_input = WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.XPATH, '//*[@name="password"]'))
    )
    time.sleep(1)  # Chờ một chút trước khi nhập mật khẩu
    password_input.send_keys(PASSWORD)
    password_input.send_keys(Keys.RETURN)

    # Chờ trang hoàn tất đăng nhập
    WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.XPATH, '//*[@id="profileIdentifier"]'))
    )

    print("Đăng nhập thành công")

finally:
    # Đóng trình duyệt sau 10 giây
    time.sleep(10)
    driver.quit()
