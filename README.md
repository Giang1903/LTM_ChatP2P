# LTM_ChatP2P
Đề tài:  Lập trình ứng dụng P2P Chat (GUI) (Ứng dụng chat, thao tác bằng GUI nhưng không có server)
Các tính năng chính
1. Giao diện người dùng (GUI)
Giao diện đơn giản bằng WPF.
Có các thành phần:
Ô nhập IP và port để kết nối.
Khung hiển thị tin nhắn.
Ô nhập tin nhắn.
Nút "Kết nối", "Gửi".
Danh sách người dùng đã kết nối.
2. Kết nối P2P qua TCP Socket
Cho phép nhập IP và port để kết nối trực tiếp giữa hai máy.
Hiển thị trạng thái kết nối.
Trò chuyện đồng thời : Ứng dụng hỗ trợ nhiều cuộc trò chuyện cùng lúc.
Tự động đóng socket khi ngắt kết nối.
4. Gửi và nhận tin nhắn
Gửi và nhận yêu cầu trò chuyện : Sau khi thiết lập, người dùng có thể gửi yêu cầu trò chuyện cho người khác khi biết IP và cổng của họ, đồng thời cũng có thể nhận các yêu cầu đến.
Tin nhắn kèm thời gian gửi/nhận.
Tính năng kết nối lại : Khi cuộc trò chuyện không còn hoạt động, nút kết nối lại sẽ hiển thị.
5. Lưu lịch sử chat
Lưu trữ hội thoại cục bộ : Tất cả các cuộc trò chuyện đều được lưu trữ cục bộ ở định dạng JSON
8. Thông báo trạng thái
Hiển thị thông báo khi có tin nhắn mới.
## Ngôn ngữ lập trình: C#
## Môi trường phát triển: Visual Studio
## Mô hình triển khai: P2P
## Mô hình thiết kế: MVVM
