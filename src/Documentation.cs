
// ReSharper disable UnusedType.Global
// ReSharper disable CommentTypo
#pragma warning disable CS1587, CS1591

/// <summary>
/// <para>
/// ===================================================================================================
/// </para>
/// <para>
///                              Pro5Chrome - Quản lý và Tự động hóa Profile Chrome
/// </para>
/// <para>
/// ===================================================================================================
/// </para>
/// 
/// <para>
/// Ứng dụng desktop được xây dựng bằng C# WinForms để quản lý, sắp xếp và tự động hóa
/// các profile Google Chrome (hoặc các trình duyệt nhân Chrome khác như Cent Browser).
/// </para>
/// 
/// <para>
/// <b>Tính năng nổi bật:</b>
/// </para>
/// <list type="bullet">
///     <item><description><b>Quản lý Profile:</b> Thêm, xóa, khám phá tự động các profile từ thư mục User Data.</description></item>
///     <item><description><b>Quản lý Cửa sổ:</b> Dễ dàng sắp xếp, phóng to, thu nhỏ, đóng tất cả các cửa sổ trình duyệt.</description></item>
///     <item><description><b>Tự động hóa:</b> Tích hợp Selenium để thực thi các kịch bản tự động trên các profile.</description></item>
///     <item><description><b>Lưu trữ Thông tin:</b> Lưu trữ email, mật khẩu, OTP secret cho từng profile để sử dụng cho các tác vụ tự động.</description></item>
///     <item><description><b>Quản lý URL:</b> Lưu lại danh sách các URL thường dùng.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// ===================================================================================================
/// </para>
/// <para>
///                 Hướng dẫn Cài đặt &amp; Chạy Chức năng Tự động hóa (Selenium)
/// </para>
/// <para>
/// ===================================================================================================
/// </para>
/// 
/// <para>
/// Để biên dịch và chạy đầy đủ các tính năng của dự án, đặc biệt là chức năng "Tự động Đăng nhập Google",
/// bạn cần thực hiện các bước cài đặt sau.
/// </para>
/// 
/// <para>
/// <b>Bước 1: Tải và Cài đặt .NET SDK</b>
/// </para>
/// <list type="bullet">
///     <item><description>
///     Đảm bảo bạn đã cài đặt <b>.NET 8 SDK</b> hoặc phiên bản mới hơn. 
///     Bạn có thể tải về từ <see href="https://dotnet.microsoft.com/download">trang web chính thức của Microsoft</see>.
///     </description></item>
/// </list>
/// 
/// <para>
/// <b>Bước 2: Thêm Thư viện Selenium vào Dự án</b>
/// </para>
/// <para>
/// Mở một cửa sổ Terminal hoặc Command Prompt trong thư mục gốc của dự án 
/// (thư mục chứa tệp <c>Pro5Chrome.csproj</c>) và chạy lệnh sau:
/// </para>
/// <code>
/// dotnet add package Selenium.WebDriver
/// </code>
/// <para>
/// Lệnh này sẽ tự động tải và tham chiếu thư viện Selenium vào dự án của bạn.
/// </para>
/// 
/// <para>
/// <b>Bước 3: Tải và Đặt ChromeDriver</b>
/// </para>
/// <para>
/// Đây là bước quan trọng để mã nguồn có thể điều khiển được trình duyệt.
/// </para>
/// <list type="number">
///     <item><description>
///     <b>Kiểm tra phiên bản Chrome của bạn:</b> Mở Chrome, đi đến <c>Cài đặt &gt; Giới thiệu về Chrome</c> và ghi lại phiên bản của bạn (ví dụ: <c>125.0.6422.113</c>).
///     </description></item>
///     <item><description>
///     <b>Tải về ChromeDriver tương thích:</b> Truy cập trang <see href="https://googlechromelabs.github.io/chrome-for-testing/">Google for Testing: ChromeDriver downloads</see>, 
///     tìm đến mục "Stable" và tải về phiên bản <c>chromedriver-win64.zip</c> khớp với phiên bản Chrome của bạn.
///     </description></item>
///     <item><description>
///     <b>Giải nén và Đặt vào đúng thư mục:</b> Giải nén tệp zip, sao chép tệp <c>chromedriver.exe</c> và dán nó vào thư mục biên dịch của dự án, 
///     thường là: <c>src/bin/Debug/net8.0-windows</c>.
///     </description></item>
/// </list>
/// 
/// <para>
/// ---------------------------------------------------------------------------------------------------
/// </para>
/// 
/// <para>
/// Sau khi hoàn thành 3 bước trên, bạn có thể mở dự án bằng Visual Studio và nhấn "Start" 
/// (hoặc chạy lệnh <c>dotnet run</c> từ thư mục <c>src</c>) để biên dịch và chạy ứng dụng với đầy đủ tính năng.
/// </para>
/// </remarks>
internal static class Documentation
{
}
