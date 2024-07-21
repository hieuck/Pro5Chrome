import tkinter as tk
import webbrowser

def open_shop_titans():
    # Đường dẫn tới ứng dụng Shop Titans trong Steam
    shop_titans_url = "steam://rungameid/1258080"
    
    # Mở URL bằng trình duyệt mặc định của hệ điều hành
    webbrowser.open(shop_titans_url)

def login():
    username = entry_username.get()
    password = entry_password.get()
    # Thực hiện xử lý đăng nhập ở đây (chẳng hạn kiểm tra đăng nhập và xử lý logic)

# Tạo cửa sổ gốc
root = tk.Tk()
root.title("Shop Tians Auto")

# Tạo frame cho phần mở Game
open_frame = tk.Frame(root)
open_frame.pack()

# Tạo nút mở Shop Titans
button_open = tk.Button(open_frame, text="Mở Shop Titans", command=open_shop_titans)
button_open.pack(padx=10, pady=10)

# Tạo frame cho phần đăng nhập
login_frame = tk.Frame(root, padx=20, pady=20)
login_frame.pack(side=tk.LEFT, padx=50, pady=50)

# Tạo label và entry cho tên đăng nhập
label_username = tk.Label(login_frame, text="Tên đăng nhập:")
label_username.grid(row=0, column=0, padx=5, pady=5)
entry_username = tk.Entry(login_frame)
entry_username.grid(row=0, column=1, padx=5, pady=5)

# Tạo label và entry cho mật khẩu
label_password = tk.Label(login_frame, text="Mật khẩu:")
label_password.grid(row=1, column=0, padx=5, pady=5)
entry_password = tk.Entry(login_frame, show="*")
entry_password.grid(row=1, column=1, padx=5, pady=5)

# Tạo nút đăng nhập
button_login = tk.Button(login_frame, text="Đăng nhập", command=login)
button_login.grid(row=2, columnspan=2, padx=5, pady=10)

# Chạy vòng lặp chính của tkinter
root.mainloop()
