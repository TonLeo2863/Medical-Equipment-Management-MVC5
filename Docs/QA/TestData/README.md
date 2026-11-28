# Dữ liệu kiểm thử nghiệm thu

| Tệp | Phạm vi |
|---|---|
| `hop-le.xlsx` | Một dòng hợp lệ, giá 1.200.000 và khoa để trống |
| `trung-ma-tai-san.xlsx` | Hai dòng cùng AssetCode, khác Serial |
| `trung-serial.xlsx` | Hai dòng khác AssetCode, cùng Serial |
| `gia-am.xlsx` | Giá mua âm |
| `sai-model-hoac-khoa.xlsx` | Một dòng sai ModelCode, một dòng sai DepartmentCode |
| `tep-hong.xlsx` | Nội dung văn bản giả dạng XLSX để thử kiểm tra cấu trúc |
| `qua-2mb.xlsx` | Gói kiểm thử vượt 2 MiB để thử giới hạn trước khi đọc workbook |
| `doi-chieu-dashboard.sql` | Đối chiếu thiết bị, phiếu, chi phí và lịch kỹ thuật ở quyền ADMIN/MANAGER |
| `doi-chieu-nghiep-vu.sql` | Đối chiếu tài khoản fixture, vị trí, giao dịch kho, phiếu và lịch kỹ thuật |

Với tệp Excel dùng mẫu hợp lệ, kiểm tra mã `GE-LOGIQ-E10` đang hoạt động. Nếu dữ liệu máy khác, thay mã đó bằng một ModelCode hoạt động do nhóm cấp. Khi cùng dùng một BetaTest, mỗi thành viên tạo bản sao workbook và thay tiền tố RCUI của AssetCode/Serial bằng tiền tố riêng. Giữ nguyên hai giá trị trùng trong từng ca trùng và giữ mã tham chiếu cố ý sai.

Chỉ truy vấn BetaTest. Tùy chọn thực thi `sqlcmd -S 'localhost\SQLEXPRESS' -E -d MedicalEquipmentDB_BetaTest -b -f 65001 -i <tệp>`. Kiểm tra mã fixture và các tham số ở đầu script trước khi chạy.

## Tạo tài khoản nghiệm thu

ADMIN mở Quản lý tài khoản, tạo tên có tiền tố riêng rồi chọn đúng vai trò. Tạo mật khẩu ngẫu nhiên từ 12–256 ký tự và gửi qua kênh riêng. Đăng nhập bằng cửa sổ riêng tư để giữ phiên ADMIN. Chỉ đặt lại hoặc đổi mật khẩu của tài khoản fixture. Vô hiệu hóa fixture đã hoàn tất theo thống nhất của nhóm.

## Quét QR và Barcode

Từ chi tiết thiết bị, mở ảnh QR hoặc Barcode. Dùng máy quét có hỗ trợ QR và Code 128 hoặc ứng dụng quét trên điện thoại. So chuỗi giải mã với Serial trên trang chi tiết, gồm chữ hoa/thường và dấu gạch nối. Nếu thiếu công cụ thì ghi DEV-14 là NOT RUN. Công cụ QA tự động có thể dùng ZXing.Net net48 với `MEDICAL_QA_BARCODE_DECODER`.

## Postman

Import [collection](../Equipment.postman_collection.json). Điền biến môi trường cục bộ, để Cookie jar hoạt động trên cùng hostname. Lấy trang Login và token, đăng nhập, lấy token từ form tạo thiết bị rồi gửi `RequestVerificationToken` khi ghi. Chọn ModelId và AssetId của fixture. Không xuất environment chứa mật khẩu hoặc token.
