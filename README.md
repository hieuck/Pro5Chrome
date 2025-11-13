# Pro5Chrome - Quản lý và Tự động hóa Profile Chrome

Ứng dụng desktop được xây dựng bằng C# WinForms để quản lý, sắp xếp và tự động hóa các profile Google Chrome (hoặc các trình duyệt nhân Chrome khác như Cent Browser).

## Tính năng nổi bật

- **Quản lý Profile:** Thêm, xóa, khám phá tự động các profile từ thư mục User Data.
- **Quản lý Cửa sổ:** Dễ dàng sắp xếp, phóng to, thu nhỏ, đóng tất cả các cửa sổ trình duyệt.
- **Tự động hóa:** Tích hợp Selenium để thực thi các kịch bản tự động trên các profile.
- **Lưu trữ Thông tin:** Lưu trữ email, mật khẩu, OTP secret cho từng profile để sử dụng cho các tác vụ tự động.
- **Quản lý URL:** Lưu lại danh sách các URL thường dùng.

---

## Hướng dẫn Cài đặt & Chạy Chức năng Tự động hóa (Selenium)

Để biên dịch và chạy đầy đủ các tính năng của dự án, đặc biệt là chức năng "Tự động Đăng nhập Google", bạn cần thực hiện các bước cài đặt sau.

### Bước 1: Tải và Cài đặt .NET SDK

- Đảm bảo bạn đã cài đặt **.NET 8 SDK** hoặc phiên bản mới hơn. Bạn có thể tải về từ [trang web chính thức của Microsoft](https://dotnet.microsoft.com/download).

### Bước 2: Thêm Thư viện Selenium vào Dự án

Mở một cửa sổ Terminal hoặc Command Prompt trong thư mục gốc của dự án (thư mục chứa tệp `Pro5Chrome.csproj`) và chạy lệnh sau:

```bash
dotnet add package Selenium.WebDriver
```

Lệnh này sẽ tự động tải và tham chiếu thư viện Selenium vào dự án của bạn.

### Bước 3: Tải và Đặt ChromeDriver

Đây là bước quan trọng để mã nguồn có thể điều khiển được trình duyệt.

1.  **Kiểm tra phiên bản Chrome của bạn:**
    - Mở trình duyệt Chrome.
    - Đi đến `Cài đặt` > `Giới thiệu về Chrome`.
    - Ghi lại phiên bản của bạn (ví dụ: `125.0.6422.113`).

2.  **Tải về ChromeDriver tương thích:**
    - Truy cập trang web sau: **[Google for Testing: ChromeDriver downloads](https://googlechromelabs.github.io/chrome-for-testing/)**
    - Tìm đến mục `Stable`.
    - Dưới phiên bản Chrome của bạn, sao chép URL cho `chromedriver-win64.zip`.

3.  **Giải nén và Đặt vào đúng thư mục:**
    - Giải nén tệp zip vừa tải về. Bạn sẽ thấy một tệp có tên `chromedriver.exe`.
    - Sao chép tệp `chromedriver.exe` này.
    - Dán nó vào thư mục biên dịch của dự án. Đường dẫn mặc định thường là:
      `src/bin/Debug/net8.0-windows`
    - Khi bạn chạy ứng dụng từ Visual Studio hoặc qua `dotnet run`, nó sẽ tự động tìm thấy `chromedriver.exe` tại vị trí này.

---

Sau khi hoàn thành 3 bước trên, bạn có thể mở dự án bằng Visual Studio và nhấn "Start" (hoặc chạy lệnh `dotnet run` từ thư mục `src`) để biên dịch và chạy ứng dụng với đầy đủ tính năng.
