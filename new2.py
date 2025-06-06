import tkinter as tk
from tkinter import ttk, Menu, messagebox
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.chrome.options import Options as ChromeOptions
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import subprocess
import json
import os
import time
import pygetwindow as gw
import pywinauto
import psutil
import webbrowser
import screeninfo
import shutil
import ctypes
import pyperclip  # Thư viện để thao tác với clipboard

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

# Khởi tạo các biến toàn cục
profile_window_map = {}
current_window_index = 0

# --------------------------------------------------------------
# Start Nút để mở các tệp profiles.json, config.json và URL.json
# --------------------------------------------------------------

def open_profiles_file():
    subprocess.Popen(['notepad.exe', PROFILE_FILE])

def open_config_file():
    subprocess.Popen(['notepad.exe', CONFIG_FILE])

def open_url_file():
    subprocess.Popen(['notepad.exe', URL_FILE])

# Tạo frame mới để chứa các nút "Mở config.json", "Mở profiles.json" và "Mở URL.json"
open_buttons_frame = ttk.Frame(root, borderwidth=2, relief="groove")
open_buttons_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Frame chứa các nút để căn giữa các nút trong open_buttons_frame
center_buttons_frame = ttk.Frame(open_buttons_frame)
center_buttons_frame.pack(anchor="center")

# Nút để mở config.json
open_config_button = ttk.Button(center_buttons_frame, text="Mở config.json", command=open_config_file)
open_config_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=5)

# Nút để mở profiles.json
open_profiles_button = ttk.Button(center_buttons_frame, text="Mở profiles.json", command=open_profiles_file)
open_profiles_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=5)

# Nút để mở URL.json
open_url_button = ttk.Button(center_buttons_frame, text="Mở URL.json", command=open_url_file)
open_url_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=5)

# ------------------------------------------------------------
# End Nút để mở các tệp profiles.json, config.json và URL.json
# ------------------------------------------------------------

# --------------------------
# Start Chrome configuration
# --------------------------

# Đường dẫn Chrome mặc định nếu không có trong config
default_chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'

# Cấu hình mặc định
DEFAULT_CONFIG = {
    "always_on_top": False,
    "chrome_paths": ["C:/Program Files/Google/Chrome/Application/chrome.exe"],
    "chrome_path": "C:/Program Files/Google/Chrome/Application/chrome.exe"
}

# Hàm chuẩn hóa đường dẫn
def normalize_paths(config):
    if 'chrome_paths' in config:
        config['chrome_paths'] = [
            path.replace("\\", "/") + ("/chrome.exe" if not path.endswith("/chrome.exe") else "")
            for path in config['chrome_paths']
        ]
    return config

# Hàm để đọc cấu hình từ tệp config.json
def read_config():
    global is_always_on_top, chrome_paths, default_chrome_path
    try:
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, 'r') as file:
                config = json.load(file)
                config = normalize_paths(config)
                is_always_on_top = config.get('always_on_top', False)
                chrome_paths = config.get('chrome_paths', [default_chrome_path])
                # default_chrome_path = chrome_paths[0] if chrome_paths else default_chrome_path
                chrome_path = config.get('chrome_path', default_chrome_path)
                # In thông báo về các giá trị cấu hình đã đọc
                print(f"is_always_on_top: {is_always_on_top}")
                print(f"use_chrome_path: {chrome_path}")
        else:
            print(f"Tệp {CONFIG_FILE} không tồn tại. Sẽ sử dụng cấu hình mặc định.")
            is_always_on_top = False
            chrome_paths = [default_chrome_path]
    except FileNotFoundError:
        print(f"Tệp {CONFIG_FILE} không tồn tại. Sẽ sử dụng cấu hình mặc định.")
        is_always_on_top = False
        chrome_paths = [default_chrome_path]
    except json.JSONDecodeError as e:
        print(f"Lỗi giải mã JSON trong tệp {CONFIG_FILE}: {e}. Sẽ sử dụng cấu hình mặc định.")
        is_always_on_top = False
        chrome_paths = [default_chrome_path]
    except Exception as e:
        print(f"Lỗi khi đọc tệp {CONFIG_FILE}: {e}. Sẽ sử dụng cấu hình mặc định.")
        is_always_on_top = False
        chrome_paths = [default_chrome_path]

# Hàm để lưu cấu hình
def save_config():
    global is_always_on_top, chrome_paths, default_chrome_path
    config = {
        'always_on_top': is_always_on_top,
        'chrome_paths': chrome_paths
    }
    try:
        config = normalize_paths(config)
        with open(CONFIG_FILE, 'w') as file:
            json.dump(config, file, indent=4)
    except Exception as e:
        print(f"Lỗi khi lưu cấu hình: {e}")

# Hàm xử lý lỗi JSON
def handle_json_error():
    print("Xử lý lỗi JSON...")
    try:
        os.remove(CONFIG_FILE)  # Xóa tệp config.json khi gặp lỗi JSON
        print(f"Đã xóa {CONFIG_FILE} do lỗi JSON.")
        
        # Tạo lại tệp config.json với dữ liệu mặc định
        default_config = {
            'always_on_top': is_always_on_top,
            'chrome_paths': [default_chrome_path]           
        }
        with open(CONFIG_FILE, 'w') as file:
            json.dump(default_config, file, indent=4)
            print(f"Đã tạo lại tệp {CONFIG_FILE} với dữ liệu mặc định.")
        
        # Trả về cấu hình mặc định
        return default_config

    except Exception as e:
        print(f"Lỗi khi xử lý lỗi JSON: {e}")
        return None

# Hàm để cập nhật trạng thái always on top
def toggle_always_on_top():
    global is_always_on_top
    is_always_on_top = not is_always_on_top
    root.attributes('-topmost', is_always_on_top)
    print(f"Ứng dụng luôn hiển thị trên cùng: {is_always_on_top}")
    save_config()  # Lưu trạng thái vào config.json
    always_on_top_var.set(is_always_on_top)  # Đồng bộ hóa checkbox với giá trị mới của is_always_on_top

# Hàm xử lý sự kiện khi checkbox thay đổi trạng thái
def on_checkbox_change():
    toggle_always_on_top()  # Đảm bảo rằng trạng thái always on top được cập nhật
    save_config()  # Lưu trạng thái vào config.json khi checkbox thay đổi
    always_on_top_var.set(is_always_on_top)  # Đồng bộ hóa checkbox với giá trị mới của is_always_on_top

# Biến để lưu trạng thái của checkbox
is_always_on_top = False

# Gọi hàm để đọc cấu hình khi khởi động ứng dụng
read_config()

# Tạo checkbox để điều khiển tính năng luôn hiển thị trên cùng
always_on_top_var = tk.BooleanVar()
always_on_top_var.set(is_always_on_top)  # Giá trị mặc định, có thể bị ghi đè sau khi đọc từ config.json
always_on_top_checkbox = ttk.Checkbutton(center_buttons_frame, text="Luôn hiển thị trên cùng", variable=always_on_top_var, command=toggle_always_on_top)
always_on_top_checkbox.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10)

# Hàm để kiểm tra taskbar có ẩn hay không
def is_taskbar_hidden():
    SPI_GETWORKAREA = 0x0030
    rect = ctypes.wintypes.RECT()
    ctypes.windll.user32.SystemParametersInfoW(SPI_GETWORKAREA, 0, ctypes.byref(rect), 0)
    screen_height = ctypes.windll.user32.GetSystemMetrics(1)
    return rect.bottom == screen_height  # Taskbar ẩn nếu vùng làm việc bằng kích thước màn hình

# Hàm để cập nhật trạng thái của checkbox
def update_taskbar_status():
    current_status = is_taskbar_hidden()
    if hide_taskbar_var.get() != current_status:
        hide_taskbar_var.set(current_status)
        hide_taskbar_checkbox.config(text="Đã ẩn thanh tác vụ" if current_status else "Không ẩn thanh tác vụ")
    root.after(1000, update_taskbar_status)  # Lặp lại kiểm tra sau 1 giây

# Khởi tạo checkbox và trạng thái ban đầu
hide_taskbar_var = tk.BooleanVar(value=is_taskbar_hidden())
hide_taskbar_checkbox = ttk.Checkbutton(center_buttons_frame, text="Đã ẩn thanh tác vụ" if hide_taskbar_var.get() else "Không ẩn thanh tác vụ", variable=hide_taskbar_var)
hide_taskbar_checkbox.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10)

# Bắt đầu cập nhật trạng thái taskbar định kỳ
update_taskbar_status()

# Hàm để đọc đường dẫn Chrome từ config
def read_chrome_path():
    if os.path.exists(CONFIG_FILE):
        try:
            with open(CONFIG_FILE, 'r') as file:
                config = json.load(file)
                return config.get('chrome_path', '')  # Trả về đường dẫn Chrome từ config nếu có
        except json.JSONDecodeError as e:
            print(f"Lỗi khi đọc file cấu hình: {e}")
            return ''
        except Exception as e:
            print(f"Lỗi không xác định khi đọc file cấu hình: {e}")
            return ''
    else:
        return ''

# Đọc danh sách đường dẫn Chrome từ config
def read_chrome_paths():
    if os.path.exists(CONFIG_FILE):
        try:
            with open(CONFIG_FILE, 'r') as file:
                config = json.load(file)
                return config.get('chrome_paths', [default_chrome_path])
        except json.JSONDecodeError as e:
            print(f"Lỗi khi đọc file cấu hình: {e}")
            return [default_chrome_path]
        except Exception as e:
            print(f"Lỗi không xác định khi đọc file cấu hình: {e}")
            return [default_chrome_path]
    else:
        return [default_chrome_path]

chrome_paths = read_chrome_paths()

# Hàm để lưu đường dẫn Chrome vào config
def save_chrome_path(chrome_path):
    # Đọc cấu hình hiện tại từ file
    config = {}
    try:
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

    except json.JSONDecodeError as e:
        print(f"Lỗi khi đọc hoặc ghi file cấu hình: {e}")
    except PermissionError as e:
        print(f"Không có quyền truy cập để ghi vào {CONFIG_FILE}: {e}")
    except Exception as e:
        print(f"Lỗi khi lưu đường dẫn Chrome: {e}")

# Hàm để mở thư mục User Data
def open_user_data_folder():
    use_chrome_path = chrome_var.get() or read_chrome_path()
    
    # print(f"Đường dẫn Chrome đã sử dụng: {use_chrome_path.replace('\\', '/')}")
    use_chrome_path_display = use_chrome_path.replace('\\', '/')
    print(f"Đường dẫn Chrome đã sử dụng: {use_chrome_path_display}")
    
    user_data_path = None
    
    if 'google' in use_chrome_path.lower():
        user_data_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Google', 'Chrome', 'User Data')
    elif 'centbrowser' in use_chrome_path.lower():
        if 'chrome' in use_chrome_path.lower():
            chrome_folder_path = os.path.dirname(use_chrome_path)
            user_data_path = os.path.join(chrome_folder_path, 'User Data')  # Đường dẫn đến thư mục User Data của Cent Browser
            user_data_path_display = user_data_path.replace('\\', '/')
            print(f"Cent Browser User Data path: {user_data_path_display}")
            # Kiểm tra xem thư mục có tồn tại không
            if not os.path.exists(user_data_path):
                print(f"Thư mục User Data Cent Browser không tồn tại: {user_data_path}")
                return
    elif 'chromedriver' in use_chrome_path.lower() or 'chrome-win64' in use_chrome_path.lower():  # Check for Chrome for Testing
        if os.path.exists(os.path.join(os.path.dirname(use_chrome_path), 'chromedriver.exe')):  # Verify chromedriver exists
            user_data_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Google', 'Chrome for Testing', 'User Data')  # Đường dẫn đến Chrome for Testing's User Data
            print(f"Chrome for Testing User Data path: {user_data_path}")
        else:
            print(f"Không tìm thấy tệp chromedriver.exe tại: {os.path.dirname(use_chrome_path)}")
    else:
        print("Không thể mở thư mục User Data cho đường dẫn này.")
        return
    
    if user_data_path:
        user_data_path = os.path.abspath(user_data_path)
        subprocess.Popen(['explorer', user_data_path])

# Hàm để xóa đường dẫn Chrome đã chọn
def delete_selected_chrome_path():
    selected_path = chrome_var.get()
    if selected_path in chrome_paths:
        chrome_paths.remove(selected_path)
        save_config()
        chrome_dropdown['values'] = chrome_paths
        chrome_var.set(chrome_paths[0] if chrome_paths else default_chrome_path)

# Tạo frame chứa Combobox và Entry cho đường dẫn Chrome
chrome_frame = ttk.Frame(root, borderwidth=2, relief="groove")
chrome_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Label và Combobox cho đường dẫn Chrome
chrome_path_label = ttk.Label(chrome_frame, text="Chọn hoặc Nhập đường dẫn Chrome:")
chrome_path_label.pack(side=tk.LEFT, padx=5)

chrome_var = tk.StringVar()
chrome_var.set(read_chrome_path())
chrome_dropdown = ttk.Combobox(chrome_frame, textvariable=chrome_var)
chrome_dropdown['values'] = chrome_paths
chrome_dropdown.pack(side=tk.LEFT, padx=5)

# Thêm nút để mở thư mục User Data
open_user_data_button = ttk.Button(chrome_frame, text="Mở User Data", command=open_user_data_folder)
open_user_data_button.pack(side=tk.LEFT, padx=5)

# Tạo nút để xóa đường dẫn Chrome đã chọn
delete_chrome_path_button = ttk.Button(chrome_frame, text="Xóa đường dẫn đã chọn", command=delete_selected_chrome_path)
delete_chrome_path_button.pack(side=tk.LEFT, padx=5)

# Gọi hàm để cập nhật giao diện khi khởi động
always_on_top_var.set(is_always_on_top)  # Đồng bộ hóa always_on_top_var với giá trị từ config.json
root.attributes('-topmost', is_always_on_top)  # Đảm bảo rằng trạng thái always on top được thiết lập chính xác khi khởi động

# ------------------------
# End Chrome configuration
# ------------------------

# ----------------------------------
# Start Chrome profiles configuration
# ----------------------------------

def update_profile_listbox():
    global open_profile_listbox, close_profile_listbox, profile_window_map
    main_window_title = root.title()  # Lấy tiêu đề của cửa sổ chính của chương trình

    # Tìm tất cả các cửa sổ Chrome hoặc CentBrowser
    chrome_windows = gw.getWindowsWithTitle("Google Chrome") + gw.getWindowsWithTitle("Cent Browser")
    
    # Loại bỏ cửa sổ chính của chương trình khỏi danh sách
    chrome_windows = [win for win in chrome_windows if win.title != main_window_title]

    # Xóa danh sách cũ
    open_profile_listbox.delete(0, tk.END)
    close_profile_listbox.delete(0, tk.END)
    profile_window_map = {}

    # Thêm các cửa sổ Chrome vào ListBox tương ứng
    for win in chrome_windows:
        profile_window_map[win.title] = win
        if win.isActive:
            open_profile_listbox.insert(tk.END, win.title)
        close_profile_listbox.insert(tk.END, win.title)
        profile_window_map[win.title] = win

    # Lập lịch cập nhật lại sau 5 giây
    root.after(5000, update_profile_listbox)

def update_listbox_decorator(func):
    def wrapper(*args, **kwargs):
        result = func(*args, **kwargs)
        update_profile_listbox()
        return result
    return wrapper

# Hàm để đọc danh sách profiles từ tệp
def read_profiles():
    if os.path.exists(PROFILE_FILE):
        with open(PROFILE_FILE, 'r') as file:
            return json.load(file)
    else:
        return []

# Đọc danh sách profiles từ tệp
profiles = read_profiles()
profile_count = len(profiles)
print(f"Số lượng profiles từ tệp: {profile_count}")

# Hàm để lưu danh sách profiles vào tệp
def save_profiles(profiles):
    with open(PROFILE_FILE, 'w') as file:
        json.dump(profiles, file, indent=4)

# Hàm để mở Chrome và thêm profile nếu chưa tồn tại, sau đó mở Chrome
@update_listbox_decorator
def open_chrome_and_add_profile():
    selected_profile = profile_var.get()
    if selected_profile:
        if selected_profile not in profiles:
            profiles.append(selected_profile)
            profiles.sort()  # Sắp xếp theo thứ tự ABC
            save_profiles(profiles)
            profile_dropdown.set_completion_list(profiles)  # Cập nhật danh sách cho AutocompleteCombobox
            update_listbox()
            update_profile_count()
        
        # Lưu đường dẫn Chrome vào danh sách vào config
        new_chrome_path = chrome_var.get()
        if new_chrome_path and new_chrome_path not in chrome_paths:
            chrome_paths.append(new_chrome_path)
            chrome_paths.sort()  # Sắp xếp theo thứ tự ABC
            save_config()
            chrome_dropdown['values'] = chrome_paths
        
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn hoặc nhập một profile")

def get_chrome_arguments(profile):
    return [
        # f"--profile-directory=Profile {profile}",
        f"--user-data-dir=Profile {profile}",
        "--lang=vi",
        "--password-store=basic",
        "--gpm-disable-machine-id",
        # "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36",
        "--no-default-browser-check",
        # "--uniform2f-noise=-1",  # Đưa uniform2f_noise vào danh sách
        # f"--load-extension={{}}",  # Đặt dấu ngoặc nhọn để thay thế sau này
        # "--max-vertex-uniform=4232",
        # "--max-fragment-uniform=992",
        # "--webgl-renderer=\"ANGLE (Intel, Intel(R) UHD Graphics 630 (0x00009BC5) Direct3D11 vs_5_0 ps_5_0, D3D11)\"",
        "--no-first-run",
        "--hide-crash-restore-bubble"
    ]

# Hàm để mở Chrome với profile được chọn
@update_listbox_decorator
def open_chrome(profile):
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
    chrome_arguments = get_chrome_arguments(profile)
    subprocess.Popen([use_chrome_path] + chrome_arguments)

# Hàm để mở trang đăng nhập Google trong Chrome
@update_listbox_decorator
def login_google(profile):
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
    login_url = 'https://accounts.google.com/'
    chrome_arguments = get_chrome_arguments(profile)
    subprocess.Popen([use_chrome_path] + chrome_arguments + [login_url])

# Hàm để đăng nhập Google với profile từ Combobox
@update_listbox_decorator
def login_google_from_combobox(event=None):
    selected_profile = profile_var.get()
    if selected_profile:
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile từ Combobox")

# Hàm để đóng tất cả các tiến trình Chrome
@update_listbox_decorator
def close_chrome():
    try:
        if os.name == 'nt':  # Windows
            os.system("taskkill /im chrome.exe /f")
        else:  # Unix-based
            os.system("pkill chrome")
    except Exception as e:
        print(f"Đã xảy ra lỗi khi đóng Chrome: {e}")

# Hàm để xử lý khi nhấn Enter trên Combobox để mở Chrome
@update_listbox_decorator
def open_chrome_on_enter(event=None):
    if event and event.keysym == 'Return':
        open_chrome_and_add_profile()

class AutocompleteCombobox(ttk.Combobox):
    def set_completion_list(self, completion_list):
        self._completion_list = sorted(completion_list)
        self._hits = []
        self._hit_index = 0
        self.position = 0
        self.bind('<KeyRelease>', self.handle_keyrelease)
        self.bind('<Delete>', self.handle_delete)
        self['values'] = self._completion_list

    def autocomplete(self, delta=0):
        if delta:
            self.delete(self.position, tk.END)
        else:
            self.position = len(self.get())

        _hits = [item for item in self._completion_list if item.lower().startswith(self.get().lower())]

        if _hits != self._hits:
            self._hit_index = 0
            self._hits = _hits

        if _hits:
            self._hit_index = (self._hit_index + delta) % len(_hits)
            self.delete(0, tk.END)
            self.insert(0, _hits[self._hit_index])
            self.select_range(self.position, tk.END)

    def handle_keyrelease(self, event):
        if event.keysym in ('BackSpace', 'Left', 'Right', 'Up', 'Down'):
            return
        if event.keysym == 'Delete':
            self.handle_delete(event)
        else:
            self.autocomplete()

    def handle_delete(self, event=None):
        self.delete(0, tk.END)  # Xóa tất cả văn bản trong trường nhập
        return 'break'

# Tạo frame chứa Combobox và Entry cho Profile Chrome
configs_frame = ttk.Frame(root, borderwidth=2, relief="groove")
configs_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Label cho Combobox và Listbox
profile_label = ttk.Label(configs_frame, text="Chọn hoặc Nhập Profile:")
profile_label.pack(side=tk.LEFT, padx=5)

# Combobox để chọn hoặc nhập profile
profile_var = tk.StringVar()
profile_dropdown = AutocompleteCombobox(configs_frame, textvariable=profile_var)
profile_dropdown.set_completion_list(profiles)
profile_dropdown.pack(side=tk.LEFT, padx=5)

# Nút Mở Chrome và thêm đường dẫn nếu cần
open_button = ttk.Button(configs_frame, text="Mở Chrome", command=open_chrome_and_add_profile)
open_button.pack(side=tk.LEFT, padx=5)

# Gắn sự kiện Enter cho Combobox
def handle_enter(event):
    open_chrome_on_enter(event)
    update_profile_listbox()

profile_dropdown.bind('<Return>', handle_enter)

# Nút Đăng Nhập Google cho Combobox
login_button_combobox = ttk.Button(configs_frame, text="Đăng Nhập Google", command=login_google_from_combobox)
login_button_combobox.pack(side=tk.LEFT, padx=5)

# Nút Đóng Chrome
close_button = ttk.Button(configs_frame, text="Đóng Chrome", command=close_chrome)
close_button.pack(side=tk.LEFT, padx=5)

# -----------------
# Profile Listboxes
# -----------------

# Hàm để mở profile từ Listbox
@update_listbox_decorator
def open_profile_from_listbox(event=None):
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        open_chrome(selected_profile)
    else:
        print("Vui lòng chọn một profile từ danh sách")

# Hàm để đăng nhập Google với profile từ Listbox
@update_listbox_decorator
def login_google_from_listbox(event=None):
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        login_google(selected_profile)
    else:
        print("Vui lòng chọn một profile từ danh sách")

# Hàm để cập nhật Listbox theo thứ tự ABC
def update_listbox():
    profiles_listbox.delete(0, tk.END)
    for profile in sorted(profiles):
        profiles_listbox.insert(tk.END, profile)

def update_profile_count():
    # Đọc lại danh sách profiles
    profiles = read_profiles()
    profile_count = len(profiles)

    # Cập nhật lại nội dung của Label
    profiles_label.config(text=f"Danh sách Profiles (Tổng: {profile_count}):")

# Frame chứa các nút và Listbox
profiles_frame = ttk.Frame(root, borderwidth=2, relief="groove")
profiles_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Frame chứa Listbox
show_listbox_frame = ttk.Frame(profiles_frame, borderwidth=2, relief="groove")
show_listbox_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=10, pady=10)

# Hiển thị số lượng profiles trong label
profiles_label = ttk.Label(show_listbox_frame, text=f"Danh sách Profiles (Tổng: {profile_count}):", font=("Helvetica", 12, "bold"))
profiles_label.pack(side=tk.TOP, padx=5, pady=5)

# Listbox để hiển thị danh sách profiles
profiles_listbox = tk.Listbox(show_listbox_frame, selectmode=tk.SINGLE, height=5, font=("Helvetica", 10))
profiles_listbox.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

# Hàm để chọn profile khi click chuột trái vào Listbox
def on_left_click(event):
    # Xác định vị trí của con trỏ chuột
    listbox_index = profiles_listbox.nearest(event.y)
    # Đưa profile Combo box	
    profile_var.set(profiles_listbox.get(listbox_index))

# Thêm sự kiện chuột trái vào Listbox
profiles_listbox.bind('<Button-1>', on_left_click)

# Xử lý sự kiện nhấp đúp vào một profile trong Listbox
def handle_double_click(event):
    open_profile_from_listbox(event)
    update_profile_listbox()

profiles_listbox.bind('<Double-Button-1>', handle_double_click)

# --------------------------------
# End Chrome profile configuration
# --------------------------------

# -----------------
# Start Right Click
# -----------------

# Hàm để xử lý sự kiện chuột phải vào Listbox
def on_right_click(event):
    # Xác định vị trí của con trỏ chuột
    listbox_index = profiles_listbox.nearest(event.y)

    # Chọn profile tương ứng với mục được click chuột phải
    profiles_listbox.selection_clear(0, tk.END)
    profiles_listbox.selection_set(listbox_index)
    profiles_listbox.activate(listbox_index)
 
    # Hiển thị menu ngữ cảnh tại vị trí con trỏ chuột
    context_menu.post(event.x_root, event.y_root)

# Hàm để sao chép tên profile được chọn vào clipboard
def copy_selected_profile():
    selected_index = profiles_listbox.curselection()
    if selected_index:
        selected_profile = profiles_listbox.get(selected_index)
        pyperclip.copy(selected_profile)

# Hàm để xóa profile được chọn trong Listbox và cập nhật giao diện
def delete_selected_profile():
    selected_index = profiles_listbox.curselection()
    if selected_index:
        selected_profile = profiles_listbox.get(selected_index)

        # Hỏi xác nhận người dùng có muốn xóa không
        confirm = messagebox.askyesno("Xác nhận xóa", f"Bạn có chắc chắn muốn xóa thư mục User Data 'Profile {selected_profile}' không?")
        if confirm:
            # Xóa profile trong danh sách
            profiles_listbox.delete(selected_index)
            profiles.remove(selected_profile)
            save_profiles(profiles)

            # Xác định đường dẫn User Data để xóa
            use_chrome_path = chrome_var.get() or read_chrome_path()
            if 'google' in use_chrome_path.lower():
                user_data_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Google', 'Chrome', 'User Data')
            elif 'centbrowser' in use_chrome_path.lower():
                chrome_folder_path = os.path.dirname(use_chrome_path)
                user_data_path = os.path.join(chrome_folder_path, 'User Data')
            else:
                print("Không thể xác định đường dẫn User Data cho đường dẫn này.")
                return

            # Định nghĩa tên thư mục User Data cho profile đang chọn
            selected_user_data_folder_name = f"Profile {selected_profile}"  # Ví dụ: Profile a
            selected_user_data_folder_path = os.path.join(user_data_path, selected_user_data_folder_name)

            # Chuẩn hóa đường dẫn bằng os.path.normpath
            selected_user_data_folder_path = os.path.normpath(selected_user_data_folder_path)

            # Kiểm tra xem thư mục có tồn tại không trước khi xóa
            if os.path.exists(selected_user_data_folder_path):
                try:
                    # Xóa thư mục User Data liên quan
                    shutil.rmtree(selected_user_data_folder_path)
                    print(f"Đã xóa thư mục User Data: {selected_user_data_folder_path}")
                    update_listbox()
                    update_profile_count()
                except PermissionError as e:
                    close_chrome_window()
                    print(f"Không thể xóa thư mục: {e}. Có thể một tệp đang được sử dụng.")
                    time.sleep(1)
                    try:
                        close_chrome_window()
                        shutil.rmtree(selected_user_data_folder_path)  # Thử lại lần nữa
                        print(f"Đã xóa thư mục User Data: {selected_user_data_folder_path}")
                    except Exception as e:
                        close_chrome_window()
                        print(f"Vẫn không thể xóa: {e}")
            else:
                print(f"Thư mục User Data không tồn tại: {selected_user_data_folder_path}")
    else:
        print("Vui lòng chọn một profile từ danh sách")

# Tạo menu ngữ cảnh
context_menu = Menu(root, tearoff=0)
context_menu.add_command(label="Copy tên profile", command=lambda: copy_selected_profile())
context_menu.add_command(label="Xóa profile", command=delete_selected_profile)

# Gán sự kiện chuột phải vào Listbox
profiles_listbox.bind("<Button-3>", on_right_click)

# ---------------
# End Right Click
# ---------------

# ---------------------------
# Start Hàm tương tác profile
# ---------------------------

# Define function to arrange windows evenly on the screen
def arrange_chrome_windows():
    main_window_title = root.title()  # Đảm bảo biến main_window_title đã được định nghĩa

    # Giá trị mặc định
    margin = 0  # Giá trị giãn cách mặc định là 0

    # Lấy kích thước màn hình
    screen = screeninfo.get_monitors()[0]  # Lấy màn hình đầu tiên
    screen_width = screen.width
    screen_height = screen.height

    # Kiểm tra trạng thái của checkbox
    if hide_taskbar_var.get():
        taskbar_height = 0  # Không trừ gì nếu ẩn thanh tác vụ
    else:
        taskbar_height = 40  # Trừ 40 pixels nếu thanh tác vụ hiện

    # Tính chiều cao hiệu dụng của màn hình
    effective_height = screen_height - taskbar_height

    # Tìm tất cả các cửa sổ Chrome hoặc CentBrowser
    chrome_windows = gw.getWindowsWithTitle("Google Chrome") + gw.getWindowsWithTitle("Cent Browser")

    if chrome_windows:
        # Loại bỏ cửa sổ chính của chương trình khỏi danh sách
        chrome_windows = [win for win in chrome_windows if win.title != main_window_title]
        
        # Khôi phục tất cả các cửa sổ phóng to về chế độ bình thường
        for win in chrome_windows:
            if win.isMaximized:
                win.restore()

        # Sắp xếp các cửa sổ theo thứ tự đảo ngược của thứ tự chúng được mở
        chrome_windows.sort(key=lambda x: x._hWnd, reverse=True)

        # Lấy số cột từ ô nhập liệu
        try:
            num_columns = int(columns_entry.get())
            if num_columns < 1:
                num_columns = 1  # Đảm bảo số cột không nhỏ hơn 1
        except ValueError:
            num_columns = 2  # Nếu không có giá trị hợp lệ, mặc định là 2

        # Tính số hàng dựa trên số cột
        num_rows = (len(chrome_windows) + num_columns - 1) // num_columns  # Tính số hàng cần thiết
        
        # Kiểm tra xem num_rows có bằng 0 không
        if num_rows == 0:
            print("Không có cửa sổ để sắp xếp.")
            return  # Dừng hàm nếu không có cửa sổ

        # Tính kích thước mới cho cửa sổ
        window_width = (screen_width - (num_columns - 1) * margin) // num_columns
        window_height = (effective_height - (num_rows - 1) * margin) // num_rows # Sử dụng chiều cao có thể sử dụng

        # Giới hạn số lượng cửa sổ theo số hàng và cột
        max_windows = num_columns * num_rows
        chrome_windows = chrome_windows[:max_windows]

        # Sắp xếp và di chuyển các cửa sổ
        for index, win in enumerate(chrome_windows):
            row = index // num_columns
            col = index % num_columns

            # Tính toán vị trí của cửa sổ
            x = col * (window_width + margin)
            y = row * (window_height + margin)

            # Di chuyển và thay đổi kích thước cửa sổ
            try:
                win.moveTo(x, y)
                win.resizeTo(window_width, window_height)
                print(f"Cửa sổ {win.title} đã được sắp xếp tại ({x}, {y}) với kích thước ({window_width}, {window_height})")
            except Exception as e:
                print(f"Lỗi khi di chuyển hoặc thay đổi kích thước cửa sổ {win.title}: {e}")

        print("Đã sắp xếp các cửa sổ Chrome thành công.")
    else:
        print("Không tìm thấy cửa sổ Chrome hoặc CentBrowser nào.")

# Tạo hàm tìm kiếm và xử lý cửa sổ Chrome theo profile
def find_chrome_window_by_profile(profile):
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
    profile_directory = f"--profile-directory=Profile {profile}"
    
    for proc in psutil.process_iter(attrs=['pid', 'name', 'cmdline']):
        if proc.info['name'] == 'chrome.exe' or proc.info['name'] == 'CentBrowser.exe':
            try:
                if any(profile_directory in arg for arg in proc.info['cmdline']):
                    print(f"Found process with profile: {proc.info['cmdline']}")
                    windows = gw.getWindowsWithTitle(proc.info['name'])
                    for win in windows:
                        if proc.pid == win._hWnd:
                            return win
            except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess) as e:
                print(f"Error accessing process info: {e}")
                continue
    return None

# Hàm để mở toàn bộ Chrome với các profile
@update_listbox_decorator
def open_all_chrome_profiles():
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
    if not profiles:
        print("Không có profile nào để mở.")
        return
    
    for profile in profiles:
        chrome_arguments = get_chrome_arguments(profile)
        subprocess.Popen([use_chrome_path] + chrome_arguments)

# Hàm tìm cửa sổ Chrome theo profile
def find_chrome_window(profile_name):
    main_window_title = root.title()  # Lấy title của cửa sổ chính của chương trình
    all_windows = gw.getAllTitles()
    
    # Tạo danh sách cửa sổ không chứa cửa sổ chính của chương trình
    filtered_windows = []
    for window_title in all_windows:
        if profile_name in window_title or "- Google Chrome" in window_title or "- Cent Browser" in window_title:
            windows = gw.getWindowsWithTitle(window_title)
            filtered_windows.extend(windows)
    
    # Loại bỏ cửa sổ chính của chương trình khỏi danh sách
    filtered_windows = [win for win in filtered_windows if win.title != main_window_title]
    
    if filtered_windows:
        # Sắp xếp các cửa sổ theo thứ tự đảo ngược của thứ tự chúng được mở
        filtered_windows.sort(key=lambda x: x._hWnd, reverse=True)
        return filtered_windows[0]  # Trả về cửa sổ đầu tiên trong danh sách đã loại bỏ cửa sổ chính
    else:
        return None

@update_listbox_decorator
def maximize_selected_chrome():
    index = profiles_listbox.curselection()
    print(f"Current selection index: {index}")  # Debug: Xem chỉ mục lựa chọn hiện tại
    if index:
        selected_profile = profiles_listbox.get(index[0])  # Lấy giá trị từ chỉ mục đầu tiên
        print(f"Selected profile: {selected_profile}")  # Debug: Xem giá trị profile được chọn
        chrome_window = find_chrome_window(selected_profile)
        if chrome_window:
            try:
                chrome_window.maximize()  # Tối đa hóa cửa sổ
                print(f"Đã phóng to cửa sổ cho hồ sơ '{selected_profile}'")
            except Exception as e:
                print(f"Lỗi khi phóng to cửa sổ: {e}")
        else:
            print(f"Không tìm thấy cửa sổ cho hồ sơ '{selected_profile}'")
    else:
        print("Vui lòng chọn một hồ sơ để phóng to.")

@update_listbox_decorator
def minimize_selected_chrome():
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        
        # Tìm cửa sổ Chrome hoặc CentBrowser
        chrome_window = find_chrome_window(selected_profile)
        if chrome_window:
            chrome_window.minimize()
        else:
            print(f"Không tìm thấy cửa sổ cho hồ sơ '{selected_profile}'")
    else:
        print("Vui lòng chọn một hồ sơ để thu nhỏ.")

@update_listbox_decorator
def restore_selected_chrome():
    index = profiles_listbox.curselection()
    if index:
        selected_profile = profiles_listbox.get(index)
        
        # Tìm cửa sổ Chrome hoặc CentBrowser
        chrome_window = find_chrome_window(selected_profile)
        if chrome_window:
            # Kiểm tra xem cửa sổ đang minimized hay không active
            if chrome_window.isMinimized:
                chrome_window.restore()
                chrome_window.activate()
                print(f"Đã khôi phục và kích hoạt cửa sổ cho hồ sơ '{selected_profile}'")
            elif not chrome_window.isActive:
                chrome_window.restore()
                chrome_window.activate()
                print(f"Đã khôi phục và kích hoạt cửa sổ cho hồ sơ gần nhất")
            else:
                print(f"Cửa sổ cho hồ sơ gần nhất đã hoạt động trước đó.")
        else:
            print(f"Không tìm thấy cửa sổ cho hồ sơ gần nhất")
    else:
        print("Vui lòng chọn một hồ sơ để khôi phục.")

# Hàm để đóng cửa sổ Chrome hoặc Cent Browser
@update_listbox_decorator
def close_chrome_window():
    # Lấy title của cửa sổ chính của chương trình
    main_window_title = root.title()  

    # Tìm tất cả các cửa sổ của Chrome và CentBrowser
    chrome_windows = gw.getWindowsWithTitle("Google Chrome") + gw.getWindowsWithTitle("Cent Browser")

    if chrome_windows:
        # Sắp xếp các cửa sổ theo thứ tự đảo ngược của thứ tự chúng được mở
        chrome_windows.sort(key=lambda x: x._hWnd, reverse=True)

        # Lặp qua các cửa sổ tìm được
        for win in chrome_windows:
            # Đảm bảo cửa sổ không phải là cửa sổ chính của chương trình
            if win.title != main_window_title:
                try:
                    # Kích hoạt cửa sổ
                    win.activate()
                    print(f"Đã chuyển đến và kích hoạt cửa sổ: {win.title}")

                    # Đóng cửa sổ
                    win.close()
                    print(f"Đã đóng cửa sổ: {win.title}")

                    return  # Kết thúc sau khi đóng thành công cửa sổ
                except Exception as e:
                    print(f"Lỗi khi đóng cửa sổ: {e}")

        # Nếu không tìm thấy cửa sổ phù hợp để đóng
        print("Không tìm thấy cửa sổ phù hợp để đóng.")
    else:
        print("Không tìm thấy cửa sổ Chrome hoặc Cent Browser nào để đóng.")

@update_listbox_decorator
def switch_tab_chrome():
    global current_window_index
    main_window_title = root.title()  # Đảm bảo biến main_window_title đã được định nghĩa

    # Tìm tất cả các cửa sổ Chrome hoặc CentBrowser
    chrome_windows = gw.getWindowsWithTitle("Google Chrome") + gw.getWindowsWithTitle("Cent Browser")

    if chrome_windows:
        # Loại bỏ cửa sổ chính của chương trình khỏi danh sách
        chrome_windows = [win for win in chrome_windows if win.title != main_window_title]
        
        # Sắp xếp các cửa sổ theo thứ tự đảo ngược của thứ tự chúng được mở
        chrome_windows.sort(key=lambda x: x._hWnd, reverse=True)

        # Kiểm tra và cập nhật chỉ số cửa sổ hiện tại để không vượt quá số lượng cửa sổ
        if current_window_index >= len(chrome_windows):
            current_window_index = 0

        if chrome_windows:
            # Lấy cửa sổ kế tiếp dựa trên chỉ số hiện tại
            chrome_window = chrome_windows[current_window_index]

            try:
                # Kích hoạt cửa sổ mà không di chuyển chuột
                chrome_window.activate()
                print(f"Đã chuyển đến và kích hoạt cửa sổ: {chrome_window.title}")

                # Tăng chỉ số cửa sổ hiện tại để chuyển sang cửa sổ kế tiếp trong lần nhấn nút tiếp theo
                current_window_index += 1

            except Exception as e:
                print(f"Lỗi khi chuyển tab: {e}")
        else:
            print("Không tìm thấy cửa sổ Chrome hoặc CentBrowser nào sau khi loại bỏ cửa sổ chính.")
    else:
        print("Không tìm thấy cửa sổ Chrome hoặc CentBrowser nào.")

# Frame chứa tất cả các thành phần điều khiển
container_frame = ttk.Frame(profiles_frame, borderwidth=2, relief="solid")  # Tạo frame có khung
container_frame.pack(side=tk.LEFT, padx=10, pady=10, fill=tk.BOTH, expand=True, anchor='w')  # Đóng khung và giãn cách

# Tạo frame chứa các nút điều khiển
control_frame = ttk.Frame(container_frame)
control_frame.pack(side=tk.LEFT, pady=5, anchor='w')

# Tạo frame cho hàng đầu tiên
row1_control_frame = ttk.Frame(control_frame)
row1_control_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Tạo frame cho hàng thứ hai
row2_control_frame = ttk.Frame(control_frame)
row2_control_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Frame con để chứa các ô nhập liệu
entry_frame = ttk.Frame(row2_control_frame)  # Đặt entry_frame bên trong khung mới
entry_frame.pack(side=tk.RIGHT)

# Tạo frame cho hàng thứ ba
row3_control_frame = ttk.Frame(control_frame)
row3_control_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Nút Đăng Nhập Google cho Listbox
login_button_listbox = ttk.Button(row1_control_frame, text="Đăng Nhập Google (Danh sách)", command=login_google_from_listbox)
login_button_listbox.pack(side=tk.LEFT, padx=5)

# Tạo nút để mở toàn bộ Chrome với các profile
open_all_chrome_button = ttk.Button(row1_control_frame, text="Mở Toàn Bộ Chrome", command=open_all_chrome_profiles)
open_all_chrome_button.pack(side=tk.LEFT, padx=5)

# Gắn nút "Phóng to" với hàm maximize_selected_chrome
maximize_button = ttk.Button(row2_control_frame, text="Phóng to", command=maximize_selected_chrome)
maximize_button.pack(side=tk.LEFT, padx=5, anchor='w')

# Gắn nút "Chuyển Tab" với hàm switch_tab_chrome
switch_tab_button = ttk.Button(row2_control_frame, text="Chuyển Tab", command=switch_tab_chrome)
switch_tab_button.pack(side=tk.LEFT, padx=5, anchor='w')

# Gắn nút "Sắp xếp" với hàm arrange_chrome_windows
arrange_button = ttk.Button(row2_control_frame, text="Sắp xếp", command=arrange_chrome_windows)
arrange_button.pack(side=tk.LEFT, padx=5, pady=5, anchor='center')

# Nhập liệu cho "Số Cột"
columns_frame = ttk.Frame(entry_frame)
columns_frame.pack(side=tk.TOP, fill=tk.X, padx=5, pady=2)
ttk.Label(columns_frame, text="Số Cột:").pack(side=tk.LEFT, padx=5)
columns_entry = ttk.Entry(columns_frame, width=5)
columns_entry.pack(side=tk.RIGHT, padx=5)

# Đặt giá trị mặc định cho ô nhập liệu
columns_entry.insert(0, "2")  # Giá trị mặc định là 2

# Gắn nút "Thu nhỏ" với hàm minimize_selected_chrome
minimize_button = ttk.Button(row3_control_frame, text="Thu nhỏ", command=minimize_selected_chrome)
minimize_button.pack(side=tk.LEFT, padx=5, anchor='w')

# Gắn nút "Khôi Phục" với hàm restore_selected_chrome
restore_button = ttk.Button(row3_control_frame, text="Khôi Phục", command=restore_selected_chrome)
restore_button.pack(side=tk.LEFT, padx=5, anchor='w')

# Gắn nút "Đóng" với hàm close_chrome_window
close_button = ttk.Button(row3_control_frame, text="Đóng", command=close_chrome_window)
close_button.pack(side=tk.LEFT, padx=5, anchor='w')

# Frame for displaying Profile đang mở
open_profile_frame = ttk.Frame(profiles_frame, borderwidth=2, relief="groove")
open_profile_frame.pack(side=tk.TOP, fill=tk.BOTH, expand=True, padx=10, pady=10)
open_profile_label = ttk.Label(open_profile_frame, text="Profile đang mở")
open_profile_label.pack(anchor="nw")

open_profile_listbox = tk.Listbox(open_profile_frame,  height=1)
open_profile_listbox.pack(fill=tk.BOTH, expand=True)

# Frame for displaying Profile chuẩn bị đóng
close_profile_frame = ttk.Frame(profiles_frame, borderwidth=2, relief="groove")
close_profile_frame.pack(side=tk.BOTTOM, fill=tk.BOTH, expand=True, padx=10, pady=10)
close_profile_label = ttk.Label(close_profile_frame, text="Profile chuẩn bị đóng")
close_profile_label.pack(anchor="nw")

close_profile_listbox = tk.Listbox(close_profile_frame,  height=2)
close_profile_listbox.pack(fill=tk.BOTH, expand=True)

# -------------------------
# End tương tác với Profile
# -------------------------

# ---------
# Start URL
# ---------

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

# Hàm để lưu URL mới vào danh sách và `URL.json`, chỉ lưu khi URL là mới
def save_url_to_list_and_file(url):
    urls = read_urls()
    if url not in urls:
        urls.append(url)
        save_urls(urls)
        update_urls_listbox()
    else:
        print(f"URL '{url}' đã tồn tại trong danh sách.")

# Hàm để cập nhật Listbox URLs
def update_urls_listbox():
    urls_listbox.delete(0, tk.END)
    urls = read_urls()
    for url in urls:
        urls_listbox.insert(tk.END, url)

# Nút để mở URL từ khung nhập và lưu vào tệp URL
def open_and_save_url():
    new_url = new_url_entry.get().strip()
    if new_url:
        open_url(new_url)
        save_url_to_list_and_file(new_url)
        new_url_entry.delete(0, tk.END)  # Xóa nội dung trong trường nhập sau khi lưu
    else:
        print("Vui lòng nhập một URL")

# Hàm để lưu URL mới vào danh sách và cập nhật giao diện
def add_new_url():
    new_url = new_url_entry.get().strip()
    if new_url:
        save_url_to_list_and_file(new_url)
        new_url_entry.delete(0, tk.END)  # Xóa nội dung trong trường nhập sau khi lưu
    else:
        print("Vui lòng nhập một URL")

# Tạo frame mới cho khung nhập URL
url_input_frame = ttk.Frame(root, borderwidth=2, relief="groove")
url_input_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

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

# Hàm để xử lý khi nhấn phím Enter trên trường nhập URL
def handle_enter(event):
    if event.keysym == 'Return':
        open_and_save_url()

# Gắn sự kiện nhấn phím Enter vào trường nhập URL
new_url_entry.bind('<Return>', handle_enter)

# Tạo frame mới cho khung URL
url_frame = ttk.Frame(root, borderwidth=2, relief="groove")
url_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Frame chứa Listbox cho danh sách URLs
urls_listbox_frame = ttk.Frame(url_frame, borderwidth=2, relief="groove")
urls_listbox_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=10, pady=10)

# Label cho danh sách URLs
urls_label = ttk.Label(urls_listbox_frame, text="Danh sách URLs:", font=("Helvetica", 12, "bold"))
urls_label.pack(side=tk.TOP, padx=5, pady=5)

# Listbox để hiển thị danh sách URLs
urls_listbox = tk.Listbox(urls_listbox_frame, selectmode=tk.SINGLE, height=5, font=("Helvetica", 10))
urls_listbox.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

# Thêm các URLs vào Listbox
update_urls_listbox()

# Hàm để mở URL từ Listbox với profile tương ứng
def open_url_from_listbox(event=None):
    index = urls_listbox.curselection()
    if index:
        selected_url = urls_listbox.get(index)
        selected_profile = profile_var.get()
        use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
        if 'chrome.exe' not in use_chrome_path.lower():
            use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
        if selected_profile:
            chrome_arguments = get_chrome_arguments(profile)
            subprocess.Popen([use_chrome_path] + chrome_arguments + [selected_url])
        else:
            open_url(selected_url)
    else:
        print("Vui lòng chọn một URL từ danh sách")

# Hàm để mở URL được chọn trong Chrome
def open_url(url):
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path  # Lấy đường dẫn Chrome từ Combobox, nếu không có thì dùng đường dẫn mặc định
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')

    chrome_arguments = get_chrome_arguments(profile)
    subprocess.Popen([use_chrome_path] + chrome_arguments + [url])    

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

# Hàm để kiểm tra các profile Chrome đang chạy
def get_running_profiles():
    running_profiles = []
    for process in psutil.process_iter(['name', 'cmdline']):
        if process.info['name'] == 'chrome.exe' and '--profile-directory=' in ' '.join(process.info['cmdline']):
            for cmd in process.info['cmdline']:
                if '--profile-directory=' in cmd:
                    profile = cmd.split('=')[-1]
                    running_profiles.append(profile)
    return list(set(running_profiles))  # Loại bỏ trùng lặp nếu có

# Hàm mở URL với các profiles đang chạy
def open_url_with_running_profiles():
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')

    running_profiles = get_running_profiles()
    if not running_profiles:
        print("Không có profile nào đang chạy.")
        return
    else:
        print(f"Profiles đang chạy: {running_profiles}")

    selected_url_index = urls_listbox.curselection()
    if selected_url_index:
        selected_url = urls_listbox.get(selected_url_index[0])
        for profile in running_profiles:
            chrome_arguments = get_chrome_arguments(profile)
            subprocess.Popen([use_chrome_path] + chrome_arguments + [selected_url])
    else:
        print("Vui lòng chọn một URL từ danh sách")

# Hàm để mở URL với toàn bộ Chrome profiles
def open_url_all_profiles():
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    if 'chrome.exe' not in use_chrome_path.lower():
        use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')
    if not profiles:
        print("Không có profile nào để mở.")
        return
    
    selected_url_index = urls_listbox.curselection()
    if selected_url_index:
        selected_url = urls_listbox.get(selected_url_index[0])
        for profile in profiles:
            chrome_arguments = get_chrome_arguments(profile)
            subprocess.Popen([use_chrome_path] + chrome_arguments + [selected_url])
    else:
        print("Vui lòng chọn một URL từ danh sách")

# Hàm để xóa danh sách URL và cập nhật giao diện
def clear_urls_list():
    save_urls([])
    update_urls_listbox()

# Xử lý sự kiện nhấp đúp vào một URL trong Listbox để mở URL với profile tương ứng
urls_listbox.bind('<Double-Button-1>', lambda event: open_url_from_listbox(event))

# Frame chứa các nút tương tác URL trong Listbox
url_control_frame  = ttk.Frame(url_frame)
url_control_frame .pack(side=tk.LEFT, padx=5, anchor='w')

# Tạo frame cho hàng đầu tiên
row1_url_frame = ttk.Frame(url_control_frame )
row1_url_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Tạo frame cho hàng thứ hai
row2_url_frame = ttk.Frame(url_control_frame )
row2_url_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Tạo frame cho hàng thứ ba
row3_url_frame = ttk.Frame(url_control_frame )
row3_url_frame.pack(side=tk.TOP, pady=5, anchor='w')

# Nút để mở URL từ Listbox
open_url_button = ttk.Button(row1_url_frame, text="Mở URL được chọn", command=open_url_from_listbox)
open_url_button.pack(side=tk.LEFT, padx=5)

# Nút để xóa URL từ Listbox
delete_url_button = ttk.Button(row1_url_frame, text="Xóa URL được chọn", command=delete_selected_urls)
delete_url_button.pack(side=tk.LEFT, padx=5)

# Nút để mở URL với các profiles đang chạy
open_running_profiles_button = ttk.Button(row2_url_frame, text="Mở URL với Profiles Đang Chạy", command=open_url_with_running_profiles)
open_running_profiles_button.pack(side=tk.LEFT, padx=5)

# Nút để mở URL với toàn bộ profile
open_all_profiles_button = ttk.Button(row3_url_frame, text="Mở URL với Toàn Bộ Profiles", command=open_url_all_profiles)
open_all_profiles_button.pack(side=tk.LEFT, padx=5)

# Nút để xóa danh sách URLs
delete_urls_button = ttk.Button(row3_url_frame, text="Xóa danh sách URLs", command=clear_urls_list)
delete_urls_button.pack(side=tk.LEFT, padx=5)

# -------
# End URL
# -------

# ------------
# Đồng bộ chuột
# ------------
import pyautogui
import threading

# Các biến toàn cục để điều khiển trạng thái đồng bộ
is_syncing = False
pause_sync = False

# Hàm đồng bộ chuột giữa các cửa sổ
def sync_mouse_to_windows():
    global is_syncing, pause_sync
    while is_syncing:
        if not pause_sync:
            x, y = pyautogui.position()  # Lấy vị trí chuột
            pyautogui.moveTo(x, y)  # Di chuyển chuột đến vị trí tương tự trên các cửa sổ khác
        time.sleep(0.01)

# Hàm bắt đầu đồng bộ
def start_sync():
    global is_syncing
    if not is_syncing:  # Đảm bảo không khởi động nhiều lần
        is_syncing = True
        sync_thread = threading.Thread(target=sync_mouse_to_windows)
        sync_thread.start()

# Hàm tạm dừng đồng bộ
def pause_syncing():
    global pause_sync
    pause_sync = True

# Hàm tiếp tục đồng bộ sau khi tạm dừng
def resume_syncing():
    global pause_sync
    pause_sync = False

# Hàm dừng đồng bộ
def stop_sync():
    global is_syncing
    is_syncing = False

# Giao diện Tkinter cho đồng bộ
def create_sync_control_panel():
    sync_window = tk.Toplevel(root)  # Tạo một cửa sổ con mới
    sync_window.title("Control Panel Đồng bộ")

    # Nút để bắt đầu đồng bộ
    start_button = tk.Button(sync_window, text="Bắt đầu đồng bộ", command=start_sync, width=25, height=2)
    start_button.pack(pady=10)

    # Nút để tạm dừng đồng bộ
    pause_button = tk.Button(sync_window, text="Tạm dừng", command=pause_syncing, width=25, height=2)
    pause_button.pack(pady=10)

    # Nút để tiếp tục đồng bộ
    resume_button = tk.Button(sync_window, text="Tiếp tục đồng bộ", command=resume_syncing, width=25, height=2)
    resume_button.pack(pady=10)

    # Nút để dừng đồng bộ
    stop_button = tk.Button(sync_window, text="Dừng đồng bộ", command=stop_sync, width=25, height=2)
    stop_button.pack(pady=10)

# Thêm nút vào giao diện chính để mở Control Panel đồng bộ
sync_button = tk.Button(root, text="Đồng bộ chuột", command=create_sync_control_panel, width=25, height=2)
sync_button.pack(pady=10)

# ------------
# Đồng bộ chuột
# ------------

# ----------------------------------
# -------------Selenium-------------
# ----------------------------------

# Định nghĩa biến global cho driver và selected_profile
driver = None
selected_profile = tk.StringVar()

# Hàm để đăng nhập vào Google với Selenium
def login_google_selenium(email, password, profile):
    global driver
    chrome_options = ChromeOptions()
    
    # Sử dụng đường dẫn chrome đã được cung cấp hoặc tìm kiếm mặc định
    use_chrome_path = chrome_var.get() or read_chrome_path() or default_chrome_path
    
    # Đường dẫn đến thư mục chứa chrome.exe và chromedriver.exe
    program_directory = os.path.dirname(os.path.abspath(__file__))
    chrome_folder = os.path.join(program_directory, 'chrome-win64')
    chromedriver_path = os.path.join(chrome_folder, 'chromedriver.exe')
    chrome_exe_path = os.path.join(chrome_folder, 'chrome.exe')

    # Kiểm tra sự tồn tại của chromedriver.exe trong thư mục chrome-win64 trước
    if os.path.isfile(chromedriver_path):
        # Nếu tồn tại chromedriver.exe, sử dụng chrome.exe từ thư mục này
        if os.path.isfile(chrome_exe_path):
            use_chrome_path = chrome_exe_path
        else:
            print("Không tìm thấy tệp chrome.exe trong thư mục chrome-win64")
            return
    else:
        # Nếu không tìm thấy chromedriver.exe, sử dụng đường dẫn đã cung cấp
        if not use_chrome_path or 'chrome.exe' not in use_chrome_path.lower():
            use_chrome_path = os.path.join(use_chrome_path, 'chrome.exe')

    # Kiểm tra lại sự tồn tại của tệp chrome.exe
    if os.path.isfile(use_chrome_path):
        chrome_options.binary_location = use_chrome_path
    else:
        print("Không tìm thấy tệp chrome.exe trong đường dẫn đã cung cấp")
        return

    try:
        # Tạo dịch vụ Chrome với chromedriver
        service = ChromeService(executable_path=chromedriver_path if os.path.isfile(chromedriver_path) else ChromeDriverManager().install())
        # Khởi động trình duyệt Chrome với các tùy chọn đã thiết lập
        driver = webdriver.Chrome(service=service, options=chrome_options)
        
        driver.get('https://accounts.google.com')
        
        WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.ID, 'identifierId')))
        
        # Tìm và nhập email
        email_field = driver.find_element(By.ID, 'identifierId')
        email_field.send_keys(email)
        email_field.send_keys(Keys.RETURN)
        
        WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.NAME, 'password')))
        
        # Tìm và nhập mật khẩu
        password_field = driver.find_element(By.NAME, 'password')
        password_field.send_keys(password)
        password_field.send_keys(Keys.RETURN)
        
        # Kiểm tra đăng nhập thành công
        if "myaccount.google.com" in driver.current_url:
            print("Đăng nhập thành công!")
        else:
            print("Đăng nhập thất bại.")
    except Exception as e:
        print(f"Đã xảy ra lỗi trong quá trình đăng nhập: {e}")
        if driver:
            driver.quit()
    finally:
        if driver:
            driver.quit()

# Tạo frame mới cho Selenium và các phần liên quan
def create_selenium_frame():
    global driver
    global selected_profile

    selenium_frame = ttk.Frame(root)
    selenium_frame.pack(pady=10, fill=tk.X)

    # Label và Entry cho email và password
    email_label = ttk.Label(selenium_frame, text="Email:")
    email_label.pack(side=tk.LEFT, padx=5)
    email_entry = ttk.Entry(selenium_frame, width=30)
    email_entry.pack(side=tk.LEFT, padx=5)

    # Thêm hàm cho sự kiện khi nhấn Enter trong password_entry
    def on_enter(event):
        login_google_selenium(email_entry.get(), password_entry.get(), selected_profile.get())

    # Bắt sự kiện Enter khi ở trong password_entry
    password_label = ttk.Label(selenium_frame, text="Password:")
    password_label.pack(side=tk.LEFT, padx=5)
    password_entry = ttk.Entry(selenium_frame, show="*", width=30)
    password_entry.pack(side=tk.LEFT, padx=5)
    password_entry.bind("<Return>", on_enter)

    # Nút Đăng nhập Google bằng Selenium
    login_selenium_button = ttk.Button(selenium_frame, text="Đăng Nhập Google (Selenium)",
                                       command=lambda: login_google_selenium(email_entry.get(), password_entry.get(), selected_profile.get()))
    login_selenium_button.pack(side=tk.LEFT, padx=5)

# Gọi hàm để tạo frame Selenium trong ứng dụng chính của bạn
create_selenium_frame()

# Lưu trạng thái đường dẫn Chrome khi thoát
def on_close():
    save_config()  # Lưu trạng thái always_on_top vào config.json
    save_chrome_path(chrome_var.get())
    root.destroy()

# Gắn sự kiện khi đóng cửa sổ
root.protocol("WM_DELETE_WINDOW", on_close)

# Hàm để cập nhật danh sách profile khi khởi động
update_profile_listbox()
update_listbox()
update_profile_count()

# Chạy GUI
root.mainloop()