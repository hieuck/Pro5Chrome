# Pro5Chrome - Python Edition

![image](https://github.com/user-attachments/assets/cb6d695a-a7d1-42e6-b58e-97641609331e)

## Quản lý và Tự động hóa Profile Chrome bằng Python

Đây là phiên bản Python của công cụ Pro5Chrome, sử dụng các thư viện mạnh mẽ để cung cấp chức năng quản lý và tự động hóa trình duyệt Google Chrome.

---

## Cài đặt

Để chạy được ứng dụng, bạn cần cài đặt các thư viện phụ thuộc được liệt kê trong tệp `requirements.txt`.

Mở terminal hoặc command prompt trong thư mục gốc của dự án và chạy lệnh sau:

```bash
pip install -r requirements.txt
```

## Thiết lập ChromeDriver

Giống như phiên bản C#, phiên bản Python cũng cần `chromedriver.exe` để có thể điều khiển trình duyệt.

1.  **Kiểm tra phiên bản Chrome** của bạn (`Cài đặt > Giới thiệu về Chrome`).
2.  **Tải về ChromeDriver** tương thích từ trang [Google for Testing](https://googlechromelabs.github.io/chrome-for-testing/).
3.  **Đặt `chromedriver.exe`** vào cùng thư mục với script Python chính của bạn, hoặc một thư mục có trong biến môi trường PATH của hệ thống để script có thể tìm thấy.

## Cách chạy

Sau khi cài đặt xong, bạn có thể chạy ứng dụng bằng lệnh:

```bash
python main.py
```

*(Giả sử tệp script chính của bạn tên là `main.py`)*
