import tkinter as tk
from tkinter import ttk
import subprocess
import json
import os
import webbrowser

# Đường dẫn tệp profiles.json trong cùng thư mục với file .py
PROFILE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'profiles.json')

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

# Hàm để mở Chrome với profile được chọn hoặc tạo mới nếu không tồn tại
def open_chrome(profile):
    # Đường dẫn đến tệp thực thi của Chrome
    chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory])

# Hàm để mở trang đăng nhập Google trong Chrome
def login_google(profile):
    login_url = 'https://myaccount.google.com/'
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen(['C:/Program Files/Google/Chrome/Application/chrome.exe', profile_directory, login_url])

# Hàm để đóng tất cả các tiến trình Chrome
def close_chrome():
    try:
        if os.name == 'nt':  # Windows
            os.system("taskkill /im chrome.exe /f")
        else:  # Unix-based
            os.system("pkill chrome")
    except Exception as e:
        print(f"Đã xảy ra lỗi khi đóng Chrome: {e}")

# Hàm xử lý khi nhấn nút "Mở Chrome"
def open_chrome_action():
    selected_profile = profile_var.get()
    if selected_profile:
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn một profile")

# Hàm xử lý khi nhấn nút "Đăng Nhập"
def login_google_action():
    selected_profile = profile_var.get()
    if selected_profile:
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile")

# Hàm xử lý khi nhấn nút "Đóng Chrome"
def close_chrome_action():
    close_chrome()

# Đọc danh sách profiles từ tệp
profiles = read_profiles()

# Tạo cửa sổ chính
root = tk.Tk()
root.title("Profiles Google Chrome")

# Label và combobox để chọn profile
profile_label = ttk.Label(root, text="Chọn hoặc Nhập Profile:")
profile_label.pack(pady=10)

profile_var = tk.StringVar()
profile_dropdown = ttk.Combobox(root, textvariable=profile_var)
profile_dropdown['values'] = profiles
profile_dropdown.pack(pady=10)

# Tạo khung cho các nút
button_frame = ttk.Frame(root)
button_frame.pack(pady=20)

# Nút Mở Chrome
open_button = ttk.Button(button_frame, text="Mở Chrome", command=open_chrome_action)
open_button.pack(side=tk.LEFT, padx=5)

# Nút Đóng Chrome
close_button = ttk.Button(button_frame, text="Đóng Chrome", command=close_chrome_action)
close_button.pack(side=tk.LEFT, padx=5)

# Nút Đăng Nhập
login_button = ttk.Button(button_frame, text="Đăng Nhập Google", command=login_google_action)
login_button.pack(side=tk.LEFT, padx=5)

# Gắn sự kiện Enter cho hàm open_chrome_action
root.bind('<Return>', lambda event: open_chrome_action())

# Chạy GUI
root.mainloop()
