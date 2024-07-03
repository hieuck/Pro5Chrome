import tkinter as tk
from tkinter import ttk
import subprocess
import json
import os

# Đường dẫn tệp profiles.json và config.json trong cùng thư mục với file .py
PROFILE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'profiles.json')
CONFIG_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'config.json')

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

# Hàm để đọc danh sách đường dẫn Chrome từ config
def read_chrome_paths():
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, 'r') as file:
            config = json.load(file)
            return config.get('chrome_paths', {})  # Trả về danh sách đường dẫn Chrome từ config nếu có
    else:
        return {}

# Hàm để lưu danh sách đường dẫn Chrome vào config
def save_chrome_paths(chrome_paths):
    config = {'chrome_paths': chrome_paths}
    with open(CONFIG_FILE, 'w') as file:
        json.dump(config, file, indent=4)

# Hàm để mở Chrome với profile được chọn và đường dẫn Chrome đã chọn
def open_chrome(profile):
    chrome_paths = chrome_var.get() or default_chrome_paths  # Lấy danh sách đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    chrome_path = chrome_paths.get(profile, default_chrome_path)  # Lấy đường dẫn Chrome từ danh sách đã chọn, nếu không có thì dùng đường dẫn mặc định
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory])

# Hàm để mở trang đăng nhập Google trong Chrome
def login_google(profile):
    chrome_paths = chrome_var.get() or default_chrome_paths  # Lấy danh sách đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    chrome_path = chrome_paths.get(profile, default_chrome_path)  # Lấy đường dẫn Chrome từ danh sách đã chọn, nếu không có thì dùng đường dẫn mặc định
    login_url = 'https://myaccount.google.com/'
    profile_directory = f"--profile-directory=Profile {profile}"
    subprocess.Popen([chrome_path, profile_directory, login_url])

# Hàm để đóng tất cả các tiến trình Chrome
def close_chrome():
    try:
        if os.name == 'nt':  # Windows
            os.system("taskkill /im chrome.exe /f")
        else:  # Unix-based
            os.system("pkill chrome")
    except Exception as e:
        print(f"Đã xảy ra lỗi khi đóng Chrome: {e}")

# Hàm để mở Chrome và thêm profile nếu chưa tồn tại
def open_chrome_and_add_profile():
    selected_profile = profile_var.get()
    if selected_profile:
        if selected_profile not in profiles:
            profiles.append(selected_profile)
            profiles.sort()  # Sắp xếp theo thứ tự ABC
            save_profiles(profiles)
            profile_dropdown['values'] = profiles
            update_listbox()
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn hoặc nhập một profile")

# Hàm để cập nhật Listbox theo thứ tự ABC
def update_listbox():
    profiles_listbox.delete(0, tk.END)
    for profile in sorted(profiles):
        profiles_listbox.insert(tk.END, profile)

# Hàm để xử lý khi nhấn Enter trên Combobox để mở Chrome
def open_chrome_on_enter(event=None):
    if event and event.keysym == 'Return':
        open_chrome_and_add_profile()

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

# Hàm để cập nhật danh sách đường dẫn Chrome khi Combobox thay đổi
def update_chrome_paths(event=None):
    selected_profile = profile_var.get()
    selected_chrome_path = chrome_var.get()
    if selected_profile and selected_chrome_path:
        chrome_paths[selected_profile] = selected_chrome_path
    save_chrome_paths(chrome_paths)

# Đọc danh sách profiles từ tệp
profiles = read_profiles()

# Đọc danh sách đường dẫn Chrome từ tệp config
default_chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'


# Tạo cửa sổ chính
root = tk.Tk()
root.title("Profiles Google Chrome")

# Label cho Combobox và Listbox
profile_label = ttk.Label(root, text="Chọn hoặc Nhập Profile:")
profile_label.pack(pady=10)

# Combobox để chọn hoặc nhập profile
profile_var = tk.StringVar()
profile_dropdown = ttk.Combobox(root, textvariable=profile_var)
profile_dropdown['values'] = profiles
profile_dropdown.pack(pady=10)

# Label và Combobox cho danh sách đường dẫn Chrome
chrome_path_label = ttk.Label(root, text="Chọn hoặc Nhập đường dẫn Chrome:")
chrome_path_label.pack(pady=10)

chrome_var = tk.StringVar()
chrome_paths = read_chrome_paths() or default_chrome_paths
chrome_var.set(chrome_paths.get(profile_var.get(), default_chrome_path))  # Thiết lập đường dẫn Chrome cho Combobox
chrome_dropdown = ttk.Combobox(root, textvariable=chrome_var)
chrome_dropdown['values'] = list(chrome_paths.values())
chrome_dropdown.pack(pady=10)

# Frame chứa các nút và Listbox
frame = ttk.Frame(root)
frame.pack(pady=20)

# Nút Mở Chrome
open_button = ttk.Button(frame, text="Mở Chrome", command=open_chrome_and_add_profile)
open_button.pack(side=tk.LEFT, padx=5)

# Nút Đóng Chrome
close_button = ttk.Button(frame, text="Đóng Chrome", command=close_chrome)
close_button.pack(side=tk.LEFT, padx=5)

# Nút Đăng Nhập Google cho Combobox
login_button_combobox = ttk.Button(frame, text="Đăng Nhập Google", command=login_google_from_combobox)
login_button_combobox.pack(side=tk.LEFT, padx=5)

# Label cho danh sách profiles
profiles_label = ttk.Label(root, text="Danh sách Profiles:")
profiles_label.pack()

# Listbox để hiển thị danh sách profiles
profiles_listbox = tk.Listbox(root, selectmode=tk.SINGLE, height=5)
profiles_listbox.pack(pady=10)

# Thêm các profile vào Listbox
update_listbox()

# Nút Đăng Nhập Google cho Listbox
login_button_listbox = ttk.Button(root, text="Đăng Nhập Google (Danh sách)", command=login_google_from_listbox)
login_button_listbox.pack(pady=10)

# Xử lý sự kiện nhấp đúp vào một profile trong Listbox
profiles_listbox.bind('<Double-Button-1>', open_profile_from_listbox)

# Gắn sự kiện Enter cho Combobox
profile_dropdown.bind('<Return>', open_chrome_on_enter)

# Cập nhật danh sách đường dẫn Chrome khi Combobox thay đổi
profile_dropdown.bind('<<ComboboxSelected>>', update_chrome_paths)

# Lưu trạng thái danh sách đường dẫn Chrome khi thoát
def on_close():
    save_chrome_paths(chrome_paths)
    root.destroy()

root.protocol("WM_DELETE_WINDOW", on_close)

# Chạy GUI
root.mainloop()
