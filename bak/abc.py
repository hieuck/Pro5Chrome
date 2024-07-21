import tkinter as tk
from tkinter import ttk
import subprocess

# Constants for file paths
PROFILE_FILE = "profiles.json"
CONFIG_FILE = "config.json"
URL_FILE = "URL.json"

# Hàm để mở các tệp
def open_profiles_file():
    subprocess.Popen(['notepad.exe', PROFILE_FILE])

def open_config_file():
    subprocess.Popen(['notepad.exe', CONFIG_FILE])

def open_url_file():
    subprocess.Popen(['notepad.exe', URL_FILE])

# Initialize the main application window
root = tk.Tk()
root.title("Chrome Profile Manager")
root.geometry("1000x800")

# Tạo frame mới để chứa các nút "Mở config.json", "Mở profiles.json" và "Mở URL.json"
open_buttons_frame = ttk.Frame(root, borderwidth=2, relief="groove")
open_buttons_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# Nút để mở config.json
open_config_button = ttk.Button(open_buttons_frame, text="Mở config.json", command=open_config_file)
open_config_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10, expand=True)

# Nút để mở profiles.json
open_profiles_button = ttk.Button(open_buttons_frame, text="Mở profiles.json", command=open_profiles_file)
open_profiles_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10, expand=True)

# Nút để mở URL.json
open_url_button = ttk.Button(open_buttons_frame, text="Mở URL.json", command=open_url_file)
open_url_button.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10, expand=True)

# Hàm để cập nhật trạng thái always on top
def toggle_always_on_top():
    global is_always_on_top
    is_always_on_top = not is_always_on_top
    root.attributes('-topmost', is_always_on_top)
    print(f"Ứng dụng luôn hiển thị trên cùng: {is_always_on_top}")

# Hàm xử lý sự kiện khi checkbox thay đổi trạng thái
def on_checkbox_change():
    set_always_on_top()

# Tạo checkbox để điều khiển tính năng luôn hiển thị trên cùng
always_on_top_var = tk.BooleanVar()
always_on_top_checkbox = ttk.Checkbutton(open_buttons_frame, text="Luôn hiển thị trên cùng", variable=always_on_top_var, command=toggle_always_on_top)
always_on_top_checkbox.pack(side=tk.LEFT, fill=tk.BOTH, padx=5, pady=10, expand=True)

# Biến để lưu trạng thái của checkbox
is_always_on_top = False

# Example function to add profiles and URLs (for demonstration)
def add_demo_data():
    profiles = ["Profile 1", "Profile 2", "Profile 3"]
    urls = ["https://example.com", "https://example2.com", "https://example3.com"]

    for profile in profiles:
        profiles_listbox.insert(tk.END, profile)

    for url in urls:
        urls_listbox.insert(tk.END, url)

add_demo_data()

root.mainloop()
