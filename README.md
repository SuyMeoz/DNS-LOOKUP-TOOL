# 🔎 DNS-LOOKUP-TOOL *(Made by Mướp The Lỏ)*

Một công cụ **tra cứu phân giải tên miền** đơn giản trên WinForms, hỗ trợ nhiều loại bản ghi DNS, đo thời gian phản hồi, lưu lịch sử và xuất báo cáo.  

---

## ✨ Tính năng nổi bật

- 🌐 **Tra cứu bản ghi DNS**
  - A / AAAA (Tên miền → IP)
  - PTR (IP → Hostname)
  - MX, CNAME, TXT, NS, SOA

- ⏱️ **Đo thời gian phản hồi**
  - Thực hiện nhiều lần truy vấn
  - Thống kê trung bình / min / max

- 📂 **Batch Processing**
  - Đọc danh sách query từ file (mỗi dòng một query)
  - Tự động xử lý và lưu kết quả

- 📝 **Quản lý lịch sử**
  - Lưu lịch sử truy vấn vào `history.json`
  - Xuất kết quả sang `.txt` / `.csv` và tùy chọn nén ZIP

- ⚙️ **Cấu hình nâng cao**
  - DNS server tùy chỉnh
  - Force TCP only
  - DNSSEC (stub)

- 📊 **Báo cáo**
  - Tạo báo cáo thống kê đơn giản (`report.txt`)

---

## ⚡ Cơ chế hoạt động

Ứng dụng sử dụng thư viện **DnsClient** để thực hiện truy vấn DNS:

- `ResolveDomain` → Query A records từ domain  
- `ReverseLookup` → Query PTR từ IP  
- `QueryMultipleRecords` → Query nhiều loại bản ghi song song  
- `MeasureResponseTime` → Đo thời gian phản hồi với `Stopwatch`  
- `BatchProcess` → Đọc file, chạy query cho từng dòng, lưu kết quả  
- Lịch sử truy vấn được ghi/đọc từ `history.json`  
- Cấu hình bảo mật lưu trong `security_settings.json`  
- Báo cáo thống kê được ghi vào `report.txt`

---

## 🛠️ Công nghệ & Thư viện

- **Ngôn ngữ:** C# (.NET 6.0+)
- **Framework:** WinForms
- **NuGet Packages:**
  - [DnsClient](https://www.nuget.org/packages/DnsClient/)
  - [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

---

## 🚀 Cài đặt & Chạy thử

1. Clone repository:
   ```bash
   git clone https://github.com/your-repo/DNSLookupTool.git
   cd DNSLookupTool
   ```

2. Cài đặt dependencies nếu thiếu :
   ```bash
   dotnet add package DnsClient
   dotnet add package Newtonsoft.Json
   ```

3. Build & chạy:
   ```bash
   dotnet build
   dotnet run
   ```

---

## 📸 Giao diện minh họa

*(Lười Chụp)*

---

## 📄 Giấy phép

Dự án phục vụ mục đích **học tập**. Bạn có thể tùy chỉnh và tái sử dụng theo nhu cầu cá nhân.


