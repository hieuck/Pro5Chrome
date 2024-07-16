# Import thêm các thư viện cần thiết
from tkinter import Tk, Frame, Button, Label

# Hàm xử lý khi nhấn nút phóng to
def on_maximize(profile_name):
    # Tìm cửa sổ chứa profile tương ứng và phóng to nó
    for profile_frame in profile_frames:
        if profile_frame.profile_name == profile_name:
            profile_frame.pack_forget()
            profile_frame.pack(fill="both", expand=True)
            break

# Hàm xử lý khi nhấn nút thu nhỏ
def on_minimize(profile_name):
    # Tìm cửa sổ chứa profile tương ứng và thu nhỏ nó
    for profile_frame in profile_frames:
        if profile_frame.profile_name == profile_name:
            profile_frame.pack_forget()
            profile_frame.pack(fill="x")  # Điều chỉnh fill theo ý muốn
            break

# Tạo cửa sổ gốc và frame chứa profile
root = Tk()

# Giả sử bạn có danh sách profile
profiles = ["Profile 1", "Profile 2", "Profile 3"]

# List để lưu trữ các frame của từng profile
profile_frames = []

# Tạo frame cho từng profile và các nút phóng to, thu nhỏ
for profile_name in profiles:
    profile_frame = Frame(root, borderwidth=2, relief="groove")
    profile_frame.profile_name = profile_name  # Lưu trữ tên profile để xử lý sau này

    # Tạo label hiển thị tên profile
    Label(profile_frame, text=profile_name).pack()

    # Tạo nút phóng to
    Button(profile_frame, text="Phóng to", command=lambda name=profile_name: on_maximize(name)).pack()

    # Tạo nút thu nhỏ
    Button(profile_frame, text="Thu nhỏ", command=lambda name=profile_name: on_minimize(name)).pack()

    # Thêm frame vào danh sách các frame của profile
    profile_frames.append(profile_frame)

    # Pack frame của profile vào root
    profile_frame.pack(fill="x", padx=10, pady=5)

# Chạy mainloop
root.mainloop()
