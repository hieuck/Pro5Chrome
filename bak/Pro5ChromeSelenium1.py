import tkinter as tk
from tkinter import ttk
import subprocess
import json
import os

# Đường dẫn tệp profiles.json, config.json và URL.json trong cùng thư mục với file .py
PROFILE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'profiles.json')
CONFIG_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'config.json')
URL_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'URL.json')

# Hàm để đọc danh sách profiles từ tệp
def read_profiles():
    if os.path.exists(PROFILE_FILE):
        with open(PROFILE_FILE, 'r') as file:
            return json.load(file)
    else:
        return []

# Hàm để lưu danh sách profiles vào tệp
def save_profiles(profiles):
    with open(PROFILE_FILE, 'w') as file:
        json.dump(profiles, file, indent=4)

# Hàm để đọc đường dẫn Chrome từ config
def read_chrome_path():
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, 'r') as file:
            config = json.load(file)
            return config.get('chrome_path', '')  # Trả về đường dẫn Chrome từ config nếu có
    else:
        return ''

# Hàm để lưu đường dẫn Chrome vào config
def save_chrome_path(chrome_path):
    # Đọc cấu hình hiện tại từ file
    config = {}
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, 'r') as file:
            config = json.load(file)
    
    # Kiểm tra nếu đường dẫn Chrome mới khác với đường dẫn hiện tại thì mới lưu lại
    if chrome_path != config.get('chrome_path'):
        if 'chrome.exe' not in chrome_path.lower():
            chrome_path = os.path.join(chrome_path, 'chrome.exe')
        config['chrome_path'] = chrome_path
        with open(CONFIG_FILE, 'w') as file:
            json.dump(config, file, indent=4)

# Hàm để mở thư mục User Data
def open_user_data_folder():
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    
    print(f"Chrome path used: {chrome_path}")
    
    if 'google' in chrome_path.lower():
        user_data_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Google', 'Chrome', 'User Data')
    elif 'centbrowser' in chrome_path.lower():
        chrome_folder_path = os.path.dirname(chrome_path)
        user_data_path = os.path.join(chrome_folder_path, 'User Data')  # Đường dẫn đến thư mục User Data của Cent Browser
        
        print(f"Cent Browser User Data path: {user_data_path}")
        
        if not os.path.exists(user_data_path):
            print(f"Thư mục User Data không tồn tại: {user_data_path}")
            return
    else:
        print("Không thể mở thư mục User Data cho đường dẫn này.")
        return
    
    user_data_path = os.path.abspath(user_data_path)
    subprocess.Popen(['explorer', user_data_path])

# Hàm để xóa đường dẫn Chrome đã chọn
def delete_selected_chrome_path():
    selected_path = chrome_var.get()
    if selected_path in chrome_paths:
        chrome_paths.remove(selected_path)
        save_chrome_paths_to_config()
        chrome_dropdown['values'] = chrome_paths
        chrome_var.set(chrome_paths[0] if chrome_paths else default_chrome_path)

# Hàm để đọc danh sách URL từ tệp
def read_urls():
    if os.path.exists(URL_FILE):
        with open(URL_FILE, 'r') as file:
            return json.load(file)
    else:
        return []

# Hàm để lưu danh sách URL vào tệp
def save_urls(urls):
    with open(URL_FILE, 'w') as file:
        json.dump(urls, file, indent=4)

# Hàm để mở Chrome với profile được chọn
def open_chrome(profile):
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory])

# Hàm để mở trang đăng nhập Google trong Chrome
def login_google(profile):
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    login_url = 'https://myaccount.google.com/'
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory, login_url])

# Hàm để mở URL được chọn trong Chrome
def open_url(url):
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    subprocess.Popen([chrome_path, url])

# Hàm để đóng tất cả các tiến trình Chrome
def close_chrome():
    try:
        if os.name == 'nt':  # Windows
            os.system("taskkill /im chrome.exe /f")
        else:  # Unix-based
            os.system("pkill chrome")
    except Exception as e:
        print(f"Đã xảy ra lỗi khi đóng Chrome: {e}")

# Hàm để mở Chrome và thêm profile nếu chưa tồn tại, sau đó mở Chrome
def open_chrome_and_add_profile():
    selected_profile = profile_var.get()
    if selected_profile:
        if selected_profile not in profiles:
            profiles.append(selected_profile)
            profiles.sort()  # Sắp xếp theo thứ tự ABC
            save_profiles(profiles)
            profile_dropdown['values'] = profiles
            update_listbox()
        
        # Lưu đường dẫn Chrome vào danh sách và config
        new_chrome_path = chrome_var.get()
        if new_chrome_path and new_chrome_path not in chrome_paths:
            chrome_paths.append(new_chrome_path)
            chrome_paths.sort()  # Sắp xếp theo thứ tự ABC
            save_chrome_paths_to_config()
            chrome_dropdown['values'] = chrome_paths
        
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn hoặc nhập một profile")

# Hàm để lưu URL mới vào danh sách và `URL.json`, chỉ lưu khi URL là mới
def save_url_to_list_and_file(url):
    urls = read_urls()
    if url not in urls:
        urls.append(url)
        save_urls(urls)
        update_urls_listbox()
    else:
        print(f"URL '{url}' đã tồn tại trong danh sách.")

# Hàm để xóa danh sách URL và cập nhật giao diện
def clear_urls_list():
    save_urls([])
    update_urls_listbox()

# Hàm để cập nhật Listbox theo thứ tự ABC
def update_listbox():
    profiles_listbox.delete(0, tk.END)
    for profile in sorted(profiles):
        profiles_listbox.insert(tk.END, profile)

# Hàm để cập nhật Listbox URLs
def update_urls_listbox():
    urls_listbox.delete(0, tk.END)
    urls = read_urls()
    for url in urls:
        urls_listbox.insert(tk.END, url)

# Hàm để mở toàn bộ Chrome với các profile
def open_all_chrome_profiles():
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')    
    if not profiles:
        print("Không có profile nào để mở.")
        return
    
    for profile in profiles:
        profile_directory = f"--profile-directory=Profile {profile}"
        subprocess.Popen([chrome_path, profile_directory])

# Hàm để mở URL với toàn bộ Chrome profiles
def open_url_all_profiles():
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    if not profiles:
        print("Không có profile nào để mở.")
        return
    
    selected_url_index = urls_listbox.curselection()
    if selected_url_index:
        selected_url = urls_listbox.get(selected_url_index[0])
        for profile in profiles:
            profile_directory = f"--profile-directory=Profile {profile}"
            subprocess.Popen([chrome_path, profile_directory, selected_url])
    else:
        print("Vui lòng chọn một URL từ danh sách")

# Hàm để xử lý khi nhấn Enter trên Combobox để mở Chrome
def open_chrome_on_enter(event=None):
    if event and event.keysym == 'Return':
        open_chrome_and_add_profile()

# Hàm để xử lý khi nhấn phím Enter trên trường nhập URL
def handle_enter(event):
    if event.keysym == 'Return':
        open_and_save_url()

# Hàm để đăng nhập Google với profile từ Combobox
def login_google_from_combobox(event=None):
    selected_profile = profile_var.get()
    if selected_profile:
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile từ Combobox")

# Hàm để đăng nhập Google với profile từ Listbox
def login_google_from_listbox(event=None):
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile từ danh sách")

# Hàm để mở profile từ Listbox
def open_profile_from_listbox(event=None):
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn một profile từ danh sách")

# Hàm để lưu danh sách đường dẫn Chrome vào config
def save_chrome_paths_to_config():
    config = {'chrome_paths': chrome_paths}
    with open(CONFIG_FILE, 'w') as file:
        json.dump(config, file, indent=4)

# Đọc danh sách profiles từ tệp
profiles = read_profiles()

# Đường dẫn Chrome mặc định nếu không có trong config
default_chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'

# Đọc danh sách đường dẫn Chrome từ config
if os.path.exists(CONFIG_FILE):
    with open(CONFIG_FILE, 'r') as file:
        config = json.load(file)
        chrome_paths = config.get('chrome_paths', [default_chrome_path])
else:
    chrome_paths = [default_chrome_path]

# Hàm để lấy thư mục User Data
def get_user_data_dir():
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    
    if 'google' in chrome_path.lower():
        user_data_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Google', 'Chrome', 'User Data')
    elif 'centbrowser' in chrome_path.lower():
        chrome_folder_path = os.path.dirname(chrome_path)
        user_data_path = os.path.join(chrome_folder_path, 'User Data')
    else:
        raise ValueError("Không thể xác định thư mục User Data cho trình duyệt này.")
    
    return user_data_path

# Tạo cửa sổ chính
root = tk.Tk()
root.title("Profiles Google Chrome")

# Tạo frame chứa Combobox và Entry cho đường dẫn Chrome
chrome_frame = ttk.Frame(root)
chrome_frame.pack(pady=10, fill=tk.X)

# Label và Combobox cho đường dẫn Chrome
chrome_path_label = ttk.Label(chrome_frame, text="Chọn hoặc Nhập đường dẫn Chrome:")
chrome_path_label.pack(side=tk.LEFT, padx=5)

chrome_var = tk.StringVar()
chrome_var.set(read_chrome_path() or default_chrome_path)
chrome_dropdown = ttk.Combobox(chrome_frame, textvariable=chrome_var)
chrome_dropdown['values'] = chrome_paths
chrome_dropdown.pack(side=tk.LEFT, padx=5)

# Thêm nút để mở thư mục User Data
open_user_data_button = ttk.Button(chrome_frame, text="Mở User Data", command=open_user_data_folder)
open_user_data_button.pack(side=tk.LEFT, padx=5)

# Tạo nút để xóa đường dẫn Chrome đã chọn
delete_chrome_path_button = ttk.Button(chrome_frame, text="Xóa đường dẫn đã chọn", command=delete_selected_chrome_path)
delete_chrome_path_button.pack(side=tk.LEFT, padx=5)

# Tạo frame chứa Combobox và Entry cho Profile Chrome
profile_frame = ttk.Frame(root)
profile_frame.pack(pady=10, fill=tk.X)

# Label cho Combobox và Listbox
profile_label = ttk.Label(profile_frame, text="Chọn hoặc Nhập Profile:")
profile_label.pack(side=tk.LEFT, padx=5)

# Combobox để chọn hoặc nhập profile
profile_var = tk.StringVar()
profile_dropdown = ttk.Combobox(profile_frame, textvariable=profile_var)
profile_dropdown['values'] = profiles
profile_dropdown.pack(side=tk.LEFT, padx=5)

# Nút Mở Chrome và thêm đường dẫn nếu cần
open_button = ttk.Button(profile_frame, text="Mở Chrome", command=open_chrome_and_add_profile)
open_button.pack(side=tk.LEFT, padx=5)

# Nút Đăng Nhập Google cho Combobox
login_button_combobox = ttk.Button(profile_frame, text="Đăng Nhập Google", command=login_google_from_combobox)
login_button_combobox.pack(side=tk.LEFT, padx=5)

# Nút Đóng Chrome
close_button = ttk.Button(profile_frame, text="Đóng Chrome", command=close_chrome)
close_button.pack(side=tk.LEFT, padx=5)

# Frame chứa các nút và Listbox
listbox_frame = ttk.Frame(root)
listbox_frame.pack(pady=10, fill=tk.X)

# Label cho danh sách profiles
profiles_label = ttk.Label(listbox_frame, text="Danh sách Profiles:")
profiles_label.pack(side=tk.LEFT, padx=5)

# Listbox để hiển thị danh sách profiles
profiles_listbox = tk.Listbox(listbox_frame, selectmode=tk.SINGLE, height=5)
profiles_listbox.pack(side=tk.LEFT, padx=5)

# Thêm các profile vào Listbox
update_listbox()

# Nút Đăng Nhập Google cho Listbox
login_button_listbox = ttk.Button(listbox_frame, text="Đăng Nhập Google (Danh sách)", command=login_google_from_listbox)
login_button_listbox.pack(side=tk.LEFT, padx=5)

# Tạo nút để mở toàn bộ Chrome với các profile
open_all_chrome_button = ttk.Button(listbox_frame, text="Mở Toàn Bộ Chrome", command=open_all_chrome_profiles)
open_all_chrome_button.pack(side=tk.LEFT, padx=5)

# Xử lý sự kiện nhấp đúp vào một profile trong Listbox
profiles_listbox.bind('<Double-Button-1>', open_profile_from_listbox)

# Gắn sự kiện Enter cho Combobox
profile_dropdown.bind('<Return>', open_chrome_on_enter)

# Lưu trạng thái đường dẫn Chrome khi thoát
def on_close():
    save_chrome_path(chrome_var.get())
    root.destroy()

root.protocol("WM_DELETE_WINDOW", on_close)

# ----------------------------------
# Thêm phần URL vào giao diện
# ----------------------------------

# Hàm để lưu URL mới vào danh sách và cập nhật giao diện
def add_new_url():
    new_url = new_url_entry.get().strip()
    if new_url:
        save_url_to_list_and_file(new_url)
        new_url_entry.delete(0, tk.END)  # Xóa nội dung trong trường nhập sau khi lưu
    else:
        print("Vui lòng nhập một URL")

# Nút để mở URL từ khung nhập và lưu vào tệp URL
def open_and_save_url():
    new_url = new_url_entry.get().strip()
    if new_url:
        open_url(new_url)
        save_url_to_list_and_file(new_url)
    else:
        print("Vui lòng nhập một URL")

# Tạo frame mới cho khung nhập URL
url_input_frame = ttk.Frame(root)
url_input_frame.pack(pady=10)

# Label và Entry cho nhập URL mới
new_url_label = ttk.Label(url_input_frame, text="Nhập URL mới:")
new_url_label.pack(side=tk.LEFT, padx=5)

new_url_entry = ttk.Entry(url_input_frame, width=50)
new_url_entry.pack(side=tk.LEFT, padx=5)

# Nút để mở và lưu URL từ khung nhập
open_and_save_url_button = ttk.Button(url_input_frame, text="Mở và Lưu URL", command=open_and_save_url)
open_and_save_url_button.pack(side=tk.LEFT, padx=5)

# Nút để thêm URL mới
add_url_button = ttk.Button(url_input_frame, text="Thêm URL mới", command=add_new_url)
add_url_button.pack(side=tk.LEFT, padx=5)

# Tạo frame mới cho khung URL
url_buttons_frame = ttk.Frame(root)
url_buttons_frame.pack(pady=10, fill=tk.X)

# Gắn sự kiện nhấn phím Enter vào trường nhập URL
new_url_entry.bind('<Return>', handle_enter)

# Label cho danh sách URLs
urls_label = ttk.Label(url_buttons_frame, text="Danh sách URLs:")
urls_label.pack(side=tk.LEFT, padx=5, pady=10)

# Listbox để hiển thị danh sách URLs
urls_listbox = tk.Listbox(url_buttons_frame, selectmode=tk.SINGLE, height=5)
urls_listbox.pack(side=tk.LEFT, padx=5, pady=10)

# Thêm các URLs vào Listbox
update_urls_listbox()

# Hàm để mở URL từ Listbox với profile tương ứng
def open_url_from_listbox(event=None):
    index = urls_listbox.curselection()
    if index:
        selected_url = urls_listbox.get(index)
        selected_profile = profile_var.get()
        chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
        if 'chrome.exe' not in chrome_path.lower():
            chrome_path = os.path.join(chrome_path, 'chrome.exe')
        if selected_profile:
            profile_directory = f"--profile-directory=Profile {selected_profile}"
            subprocess.Popen([chrome_path, profile_directory, selected_url])
        else:
            open_url(selected_url)
    else:
        print("Vui lòng chọn một URL từ danh sách")

# Hàm để xóa các URLs đã chọn từ Listbox
def delete_selected_urls():
    selected_indices = urls_listbox.curselection()
    if selected_indices:
        selected_urls = [urls_listbox.get(idx) for idx in selected_indices]
        current_urls = read_urls()
        updated_urls = [url for url in current_urls if url not in selected_urls]
        save_urls(updated_urls)
        update_urls_listbox()
    else:
        print("Vui lòng chọn ít nhất một URL để xóa")

# Xử lý sự kiện nhấp đúp vào một URL trong Listbox để mở URL với profile tương ứng
urls_listbox.bind('<Double-Button-1>', lambda event: open_url_from_listbox(event))

# Frame để chứa hai nút "Mở URL được chọn" và "Xóa danh sách URLs"
open_delete_frame = ttk.Frame(url_buttons_frame)
open_delete_frame.pack(fill=tk.X)

# Nút để mở URL từ Listbox
open_url_button = ttk.Button(open_delete_frame, text="Mở URL được chọn", command=open_url_from_listbox)
open_url_button.pack(side=tk.LEFT, padx=5, pady=10)

# Nút để xóa URL từ Listbox
delete_url_button = ttk.Button(open_delete_frame, text="Xóa URL được chọn", command=delete_selected_urls)
delete_url_button.pack(side=tk.LEFT, padx=5, pady=10)

# Nút để mở URL với toàn bộ profile
open_all_profiles_button = ttk.Button(url_buttons_frame, text="Mở URL với Toàn Bộ Profiles", command=open_url_all_profiles)
open_all_profiles_button.pack(side=tk.LEFT, padx=5, pady=10)

# Nút để xóa danh sách URLs
delete_urls_button = ttk.Button(url_buttons_frame, text="Xóa danh sách URLs", command=clear_urls_list)
delete_urls_button.pack(side=tk.LEFT, padx=5, pady=10)

# ----------------------------------
# Nút để mở các tệp profiles.json, config.json và URL.json
# ----------------------------------

def open_profiles_file():
    subprocess.Popen(['notepad.exe', PROFILE_FILE])

def open_config_file():
    subprocess.Popen(['notepad.exe', CONFIG_FILE])

def open_url_file():
    subprocess.Popen(['notepad.exe', URL_FILE])

# Tạo frame mới để chứa hai nút "Mở config.json", "Mở profiles.json" và "Mở URL.json"
open_buttons_frame = ttk.Frame(root)
open_buttons_frame.pack(pady=10)

# Nút để mở config.json
open_config_button = ttk.Button(open_buttons_frame, text="Mở config.json", command=open_config_file)
open_config_button.pack(side=tk.LEFT, padx=5, pady=10)

# Nút để mở profiles.json
open_profiles_button = ttk.Button(open_buttons_frame, text="Mở profiles.json", command=open_profiles_file)
open_profiles_button.pack(side=tk.LEFT, padx=5, pady=10)

# Nút để mở URL.json
open_url_button = ttk.Button(open_buttons_frame, text="Mở URL.json", command=open_url_file)
open_url_button.pack(side=tk.LEFT, padx=5, pady=10)

# ----------------------------------
# -------------Selenium-------------
# ----------------------------------
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.chrome.options import Options as ChromeOptions
from webdriver_manager.chrome import ChromeDriverManager
import os
import time

# Hàm để đăng nhập vào Google với Selenium
def login_google_selenium(email, password, profile):
    chrome_options = ChromeOptions()
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')

    chrome_options.binary_location = chrome_path
    # chrome_options.add_argument('--start-maximized')  # Khởi động trình duyệt với cửa sổ lớn nhất

    # Thêm các tùy chọn khác nếu cần
    
    service = ChromeService(executable_path=ChromeDriverManager().install())

    driver = webdriver.Chrome(service=service, options=chrome_options)
    
    try:
        driver.get('https://accounts.google.com')

        # Tìm và nhập email
        email_field = driver.find_element(By.ID, 'identifierId')
        email_field.send_keys(email)
        email_field.send_keys(Keys.RETURN)
        time.sleep(2)

        # Tìm và nhập mật khẩu
        password_field = driver.find_element(By.NAME, 'password')
        password_field.send_keys(password)
        password_field.send_keys(Keys.RETURN)
        time.sleep(2)

        # Kiểm tra đăng nhập thành công
        if "myaccount.google.com" in driver.current_url:
            print("Đăng nhập thành công!")
        else:
            print("Đăng nhập thất bại.")
    except Exception as e:
        print(f"Đã xảy ra lỗi: {e}")
    finally:
        driver.quit()

# Định nghĩa biến selected_profile như một biến global hoặc trong phạm vi chương trình của bạn
selected_profile = tk.StringVar()

# Tạo frame mới cho Selenium và các phần liên quan
def create_selenium_frame():
    global selected_profile  # Đảm bảo biến global selected_profile được sử dụng

    selenium_frame = ttk.Frame(root)
    selenium_frame.pack(pady=10, fill=tk.X)

    # Label và Entry cho email và password
    email_label = ttk.Label(selenium_frame, text="Email:")
    email_label.pack(side=tk.LEFT, padx=5)
    email_entry = ttk.Entry(selenium_frame, width=30)
    email_entry.pack(side=tk.LEFT, padx=5)

    password_label = ttk.Label(selenium_frame, text="Password:")
    password_label.pack(side=tk.LEFT, padx=5)
    password_entry = ttk.Entry(selenium_frame, show="*", width=30)
    password_entry.pack(side=tk.LEFT, padx=5)

    # Nút Đăng nhập Google bằng Selenium
    login_selenium_button = ttk.Button(selenium_frame, text="Đăng Nhập Google (Selenium)",
                                       command=lambda: login_google_selenium(email_entry.get(), password_entry.get(), selected_profile.get()))
    login_selenium_button.pack(side=tk.LEFT, padx=5)

# Gọi hàm để tạo frame Selenium trong ứng dụng chính của bạn
create_selenium_frame()

# Chạy GUI
root.mainloop()
