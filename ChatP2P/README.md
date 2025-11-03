#  Đề tài: Lập trình Ứng dụng P2P Chat (GUI)

Ứng dụng chat với giao diện đồ họa (GUI), hoạt động theo mô hình Peer-to-Peer (P2P), **không sử dụng máy chủ trung gian**.

---

##  MÔ HÌNH TRIỂN KHAI
- **Kiểu mạng:** Peer-to-Peer (P2P) – các máy ngang hàng kết nối trực tiếp với nhau.

##  MÔ HÌNH KIẾN TRÚC
- **Mô hình:** MVVM (Model – View – ViewModel)

---

##  CÁC CHỨC NĂNG CHÍNH

### 1. Lõi Mạng P2P (Không Máy chủ)

**Thiết lập Endpoint (Điểm cuối):**
- Tại cửa sổ đăng nhập, người dùng nhập:
  - Tên hiển thị (Display Name)
  - Địa chỉ IP
  - Port để lắng nghe kết nối

**Lắng nghe & Kết nối:**
- Ứng dụng sử dụng **TcpListener** để mở port và chờ kết nối đến (giống như một server mini).
- Khi người dùng muốn chat với người khác, sử dụng **TcpClient** để kết nối trực tiếp đến IP/Port của người đó.

---

### 2. Quản lý Kết nối & Hội thoại

**Gửi Yêu cầu Chat:**
- Người dùng nhập IP/Port của người khác qua giao diện thanh bên và gửi yêu cầu kết nối.

**Xử lý Yêu cầu Chat:**
- Khi có kết nối đến, hệ thống hiển thị thông báo:
  > “Người dùng X muốn kết nối. Chấp nhận hay Từ chối?”

**Quản lý Đa Hội thoại (Multiple Conversations):**
- Một lớp quản lý trung tâm (**Singleton**) theo dõi toàn bộ các cuộc chat.
- Phân loại thành 2 nhóm:
  | Loại | Mô tả |
  |------|-------|
  | **Active Conversations** | Các phiên chat đang hoạt động, vẫn còn kết nối TCP |
  | **Inactive Conversations** | Lịch sử các phiên chat đã kết thúc |

---

### 3. Tương tác & Gửi Tin nhắn

**Gửi Tin nhắn Văn bản:**
- Hỗ trợ gửi/nhận tin nhắn text giữa hai thiết bị qua TCP.

**Gửi “Buzz”:**
- Cho phép gửi hiệu ứng “rung cửa sổ” (buzz) để gây chú ý người nhận.

**Giao thức Dữ liệu:**
- Mọi dữ liệu truyền đi đều được:
  - **Đóng gói thành JSON**
  - **Giải mã JSON khi nhận**
- Đảm bảo đúng cấu trúc và toàn vẹn thông tin.

---

### 4.Lưu trữ & Trạng thái

**Lưu lịch sử chat:**
- Tất cả cuộc hội thoại được lưu tự động dưới dạng **file JSON cục bộ**.
- Có lớp **Serializer** chuyên đọc/ghi dữ liệu lịch sử → không bị mất khi tắt ứng dụng.

**Hệ thống Thông báo:**
- Hiển thị thông tin trạng thái/lỗi cho người dùng, ví dụ:
  - “Kết nối thất bại”
  - “Bạn không thể tự kết nối với chính mình”

**Chức năng Kết nối lại (Reconnect):**
- Với các cuộc chat mất kết nối, ứng dụng hiển thị nút **Reconnect** để thử kết nối TCP lại.

---

