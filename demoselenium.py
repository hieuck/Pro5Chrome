from selenium import webdriver
from selenium.webdriver.chrome.service import Service

# Đường dẫn đến file driver của Chrome
chrome_driver_path = r'G:\GitHub\Pro5Chrome\chrome-win64\chromedriver.exe'

# Khởi tạo service Chrome
service = Service(chrome_driver_path)

# Khởi tạo driver Chrome
driver = webdriver.Chrome(service=service)

# Mở một trang web
driver.get("https://www.google.com")

# In ra tiêu đề của trang để đảm bảo rằng đã mở đúng
print("Page title is:", driver.title)

# Đóng trình duyệt sau khi hoàn thành công việc
driver.quit()
