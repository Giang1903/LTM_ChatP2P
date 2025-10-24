# LTM_ChatP2P
Đề tài:  Lập trình ứng dụng P2P Chat (GUI) (Ứng dụng chat, thao tác bằng GUI nhưng không có server)

CÁC CHỨC NĂNG CHÍNH
1. Giao diện người dùng (GUI)
Giao diện đơn giản bằng Windows Forms (hoặc WPF nếu bạn thích).
Có các thành phần:
Ô nhập IP và port để kết nối.
Khung hiển thị tin nhắn.
Ô nhập tin nhắn.
Nút "Kết nối", "Gửi", "Ngắt kết nối".
Nút gửi file.
Danh sách người dùng đã phát hiện trong mạng LAN.
2. Kết nối P2P qua TCP Socket
( Dùng thư viện System.Net.Sockets.)
Cho phép nhập IP và port để kết nối trực tiếp giữa hai máy.
Hiển thị trạng thái kết nối.
Tự động đóng socket khi ngắt kết nối.
3. Gửi và nhận tin nhắn
Gửi tin nhắn văn bản qua TCP.
Nhận tin nhắn và hiển thị ngay trên giao diện.
Tin nhắn kèm thời gian gửi/nhận.
4. Lưu lịch sử chat
Lưu tin nhắn vào file .txt,database hoặc .json.
Cho phép xem lại lịch sử chat theo ngày hoặc theo người dùng.
5. Gửi file nhỏ (ảnh, tài liệu)
Cho phép chọn file và gửi qua TCP.
File nhận được lưu vào thư mục chỉ định.
Giới hạn kích thước file (ví dụ: < 5MB).
6. Tự động phát hiện máy trong mạng LAN (UDP Broadcast)
Khi mở ứng dụng, gửi gói tin UDP broadcast để thông báo sự hiện diện.
Nhận gói tin từ các máy khác và hiển thị danh sách IP/port có thể kết nối.
7. Thông báo trạng thái
Hiển thị thông báo khi có tin nhắn mới.
Thông báo nếu mất kết nối.
8. Đăng nhập đơn giản ( không cần mật khẩu)
Cho phép người dùng nhập tên hiển thị.
Hiển thị tên người gửi trong khung chat.
