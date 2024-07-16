#-----------------------
# Hàm tương tác profile
#-----------------------

def maximize_selected_chrome():

def minimize_selected_chrome():

def close_selected_chrome():

# Tạo frame chứa các nút Phóng to, Thu nhỏ, Đóng
resize_frame = ttk.Frame(listbox_frame)
resize_frame.pack(side=tk.LEFT, padx=5)

# Gắn nút "Phóng to" với hàm maximize_selected_chrome
maximize_button = ttk.Button(resize_frame, text="Phóng to", command=maximize_selected_chrome)
maximize_button.pack(side=tk.LEFT, padx=5)

# Gắn nút "Thu nhỏ" với hàm minimize_selected_chrome
minimize_button = ttk.Button(resize_frame, text="Thu nhỏ", command=minimize_selected_chrome)
minimize_button.pack(side=tk.LEFT, padx=5)

# Gắn nút "Đóng" với hàm close_selected_chrome
close_button = ttk.Button(resize_frame, text="Đóng", command=close_selected_chrome)
close_button.pack(side=tk.LEFT, padx=5)

#-----------------------
# Hàm tương tác profile
#-----------------------
