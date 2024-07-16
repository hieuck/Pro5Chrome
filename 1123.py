import tkinter as tk
from tkinter import ttk
import subprocess
import json
import os
import pygetwindow as gw

# -----------------------------------------------------------------
# --------------------Copyright (c) 2024 hieuck--------------------
# -----------------------------------------------------------------

# Tạo cửa sổ chính
root = tk.Tk()
root.title("Profiles Google Chrome")

# Đường dẫn tệp profiles.json, config.json và URL.json trong cùng thư mục với file .py
PROFILE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'profiles.json')
CONFIG_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'config.json')
URL_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'URL.json')

# --------------------------
# Start Chrome configuration
# --------------------------

# Đường dẫn Chrome mặc định nếu không có trong config
default_chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'

# Hàm để đọc đường dẫn Chrome từ config
def read_chrome_path():
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, 'r') as file:
            config = json.load(file)
            return config.get('chrome_path', '')  # Trả về đường dẫn Chrome từ config nếu có
    else:
        return ''

# Đọc danh sách đường dẫn Chrome từ config
if os.path.exists(CONFIG_FILE):
    with open(CONFIG_FILE, 'r') as file:
        config = json.load(file)
        chrome_paths = config.get('chrome_paths', [default_chrome_path])
else:
    chrome_paths = [default_chrome_path]

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

# Hàm để lưu danh sách đường dẫn Chrome vào config
def save_chrome_paths_to_config():
    config = {'chrome_paths': chrome_paths}
    with open(CONFIG_FILE, 'w') as file:
        json.dump(config, file, indent=4)

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

# ------------------------
# End Chrome configuration
# ------------------------

# ----------------------------------
# Start Chrome profiles configuration
# ----------------------------------

# Hàm để đọc danh sách profiles từ tệp
def read_profiles():
    if os.path.exists(PROFILE_FILE):
        with open(PROFILE_FILE, 'r') as file:
            return json.load(file)
    else:
        return []

# Đọc danh sách profiles từ tệp
profiles = read_profiles()

# Hàm để lưu danh sách profiles vào tệp
def save_profiles(profiles):
    with open(PROFILE_FILE, 'w') as file:
        json.dump(profiles, file, indent=4)

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
        
        # Lưu đường dẫn Chrome vào danh sách vào config
        new_chrome_path = chrome_var.get()
        if new_chrome_path and new_chrome_path not in chrome_paths:
            chrome_paths.append(new_chrome_path)
            chrome_paths.sort()  # Sắp xếp theo thứ tự ABC
            save_chrome_paths_to_config()
            chrome_dropdown['values'] = chrome_paths
        
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn hoặc nhập một profile")

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

# Hàm để đăng nhập Google với profile từ Combobox
def login_google_from_combobox(event=None):
    selected_profile = profile_var.get()
    if selected_profile:
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile từ Combobox")

# Hàm để đóng tất cả các tiến trình Chrome
def close_chrome():
    try:
        if os.name == 'nt':  # Windows
            os.system("taskkill /im chrome.exe /f")
        else:  # Unix-based
            os.system("pkill chrome")
    except Exception as e:
        print(f"Đã xảy ra lỗi khi đóng Chrome: {e}")

# Hàm để xử lý khi nhấn Enter trên Combobox để mở Chrome
def open_chrome_on_enter(event=None):
    if event and event.keysym == 'Return':
        open_chrome_and_add_profile()

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

# Nút Đăng nhập Google
login_google_button = ttk.Button(profile_frame, text="Đăng nhập Google", command=lambda: login_google(profile_var.get()))
login_google_button.pack(side=tk.LEFT, padx=5)

# Nút Đóng Chrome
close_button = ttk.Button(profile_frame, text="Đóng Chrome", command=close_chrome)
close_button.pack(side=tk.LEFT, padx=5)

# Listbox để hiển thị các profile
profile_listbox = tk.Listbox(profile_frame)
profile_listbox.pack(side=tk.LEFT, padx=5)

def update_listbox():
    profile_listbox.delete(0, tk.END)
    for profile in profiles:
        profile_listbox.insert(tk.END, profile)

update_listbox()

# Gắn sự kiện Enter cho Combobox
profile_dropdown.bind('<Return>', open_chrome_on_enter)

# ----------------------------
# End Chrome profiles configuration
# ----------------------------

# ---------------------------------
# Start URL configuration functions
# ---------------------------------

# Hàm để đọc danh sách URL từ tệp URL.json
def read_urls():
    if os.path.exists(URL_FILE):
        with open(URL_FILE, 'r') as file:
            return json.load(file)
    else:
        return []

# Đọc danh sách URL từ tệp URL.json
urls = read_urls()

# Hàm để lưu danh sách URL vào tệp URL.json
def save_urls(urls):
    with open(URL_FILE, 'w') as file:
        json.dump(urls, file, indent=4)

# Hàm để mở URL trong Chrome
def open_url():
    selected_profile = profile_var.get()
    selected_url = url_var.get()
    if selected_profile and selected_url:
        open_chrome_url(selected_profile, selected_url)
        if selected_url not in urls:
            urls.append(selected_url)
            save_urls(urls)
            update_url_listbox()
    else:
        print("Vui lòng chọn profile và nhập URL")

# Hàm để mở URL trong Chrome với profile đã chọn
def open_chrome_url(profile, url):
    chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in chrome_path.lower():
        chrome_path = os.path.join(chrome_path, 'chrome.exe')
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory, url])

# Hàm để xóa URL đã chọn
def delete_selected_url():
    selected_url = url_var.get()
    if selected_url in urls:
        urls.remove(selected_url)
        save_urls(urls)
        update_url_listbox()

# Tạo frame chứa Combobox và Entry cho URL
url_frame = ttk.Frame(root)
url_frame.pack(pady=10, fill=tk.X)

# Label cho URL
url_label = ttk.Label(url_frame, text="Nhập URL:")
url_label.pack(side=tk.LEFT, padx=5)

# Entry cho URL
url_var = tk.StringVar()
url_entry = ttk.Entry(url_frame, textvariable=url_var, width=50)
url_entry.pack(side=tk.LEFT, padx=5)

# Nút Mở URL
open_url_button = ttk.Button(url_frame, text="Mở URL", command=open_url)
open_url_button.pack(side=tk.LEFT, padx=5)

# Nút Xóa URL đã chọn
delete_url_button = ttk.Button(url_frame, text="Xóa URL đã chọn", command=delete_selected_url)
delete_url_button.pack(side=tk.LEFT, padx=5)

# Listbox để hiển thị các URL
url_listbox = tk.Listbox(url_frame, listvariable=tk.StringVar(value=urls))
url_listbox.pack(side=tk.LEFT, padx=5)

def update_url_listbox():
    url_listbox.delete(0, tk.END)
    for url in urls:
        url_listbox.insert(tk.END, url)

update_url_listbox()

# -------------------------------
# End URL configuration functions
# -------------------------------

# Bắt đầu vòng lặp chính của tkinter
root.mainloop()
