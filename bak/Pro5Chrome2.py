import tkinter as tk
from tkinter import ttk
import subprocess
import json
import os

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

# Hàm để mở Chrome với profile được chọn
def open_chrome(profile):
    # Đường dẫn đến tệp thực thi của Chrome
    chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory])

# Hàm xử lý khi nhấn nút Submit
def submit():
    selected_profile = profile_var.get()
    if selected_profile:
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn một profile")

# Hàm xử lý khi nhấn nút Add Profile
def add_profile():
    new_profile = new_profile_var.get()
    if new_profile and new_profile not in profiles:
        profiles.append(new_profile)
        profiles.sort()  # Sắp xếp theo thứ tự ABC
        save_profiles(profiles)
        profile_dropdown['values'] = profiles
        new_profile_var.set('')

# Đọc danh sách profiles từ tệp
profiles = read_profiles()

# Tạo cửa sổ chính
root = tk.Tk()
root.title("Profile Chrome")

# Label và combobox để chọn profile
profile_label = ttk.Label(root, text="Chọn Profile:")
profile_label.pack(pady=10)

profile_var = tk.StringVar()
profile_dropdown = ttk.Combobox(root, textvariable=profile_var)
profile_dropdown['values'] = profiles
profile_dropdown.pack(pady=10)

# Nút Đăng Nhập
submit_button = ttk.Button(root, text="Đăng Nhập", command=submit)
submit_button.pack(pady=20)

# Label và entry để nhập profile mới
new_profile_label = ttk.Label(root, text="Thêm Profile Mới:")
new_profile_label.pack(pady=10)

new_profile_var = tk.StringVar()
new_profile_entry = ttk.Entry(root, textvariable=new_profile_var)
new_profile_entry.pack(pady=10)

# Nút Thêm Profile
add_profile_button = ttk.Button(root, text="Thêm Profile", command=add_profile)
add_profile_button.pack(pady=10)

# Chạy GUI
root.mainloop()
