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
  - Xuất kết quả sang `.txt` / `.csv` / `.json` / `.html`

- ⚙️ **Cấu hình nâng cao**
  - DNS server tùy chỉnh
  - Force TCP only
  - DNSSEC (stub)

- 📊 **Báo cáo**
  - Tạo báo cáo thống kê HTML
  - Xuất dữ liệu lịch sử dưới nhiều định dạng

- 🛠️ **Công cụ mạng**
  - **Ping**: Kiểm tra khả năng kết nối và đo thời gian phản hồi
  - **Traceroute**: Theo dõi đường đi gói tin đến đích

- 🔎 **WHOIS Lookup**
  - Tra cứu thông tin sở hữu tên miền / IP
  - Lưu kết quả vào lịch sử

- ℹ️ **Giao diện hiện đại**
  - Giao diện WinForms với theme tối
  - 9 tab chức năng riêng biệt
  - Trạng thái DNS server hiển thị trên header

---

## ⚡ Cơ chế hoạt động

### DNS Lookup
Ứng dụng sử dụng thư viện **DnsClient** để thực hiện truy vấn DNS:
- `ResolveDomain` → Query A/AAAA records từ domain  
- `ReverseLookup` → Query PTR từ IP  
- `QueryMultipleRecords` → Query nhiều loại bản ghi song song (MX, CNAME, TXT, NS, SOA)
- Hỗ trợ DNS server tùy chỉnh với cấu hình retry và timeout

### Network Tools
- **Ping**: Sử dụng `System.Net.NetworkInformation.Ping` để gửi ICMP echo request
- **Traceroute**: Gọi lệnh `tracert` của hệ thống và hiển thị kết quả real-time

### WHOIS Lookup
- Tra cứu thông tin WHOIS thông qua socket TCP kết nối đến WHOIS server
- Hỗ trợ truy vấn tên miền và địa chỉ IP

### Quản lý dữ liệu
- **Lịch sử**: Ghi/đọc từ `history.json` với thông tin type, query, timestamp, thời gian phản hồi
- **Cấu hình bảo mật**: Lưu trong `security_settings.json` (TCP-only, DNSSEC, v.v.)
- **Báo cáo**: Tạo file HTML, CSV, JSON hoặc Text từ dữ liệu lịch sử
- **Batch Processing**: Đọc file text, xử lý từng dòng và tổng hợp kết quả

---

## 📋 Các tab chức năng

| # | Tab | Chức năng |
|---|-----|----------|
| 1 | A/AAAA Lookup | Tra cứu địa chỉ IPv4/IPv6 từ tên miền |
| 2 | PTR Lookup | Tra cứu tên miền từ địa chỉ IP (reverse DNS) |
| 3 | DNS Records | Truy vấn các loại bản ghi DNS khác nhau (MX, CNAME, TXT, NS, SOA) |
| 4 | Batch Process | Xử lý hàng loạt truy vấn từ file text |
| 5 | History | Xem lịch sử tất cả các truy vấn đã thực hiện |
| 6 | Settings & Export | Cấu hình bảo mật, reset settings, xuất báo cáo |
| 7 | Network Tools | Ping và Traceroute để kiểm tra kết nối mạng |
| 8 | WHOIS Lookup | Tra cứu thông tin sở hữu tên miền/IP |
| 9 | About | Thông tin về ứng dụng |

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


