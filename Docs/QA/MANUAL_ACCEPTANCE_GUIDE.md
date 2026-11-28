# Hướng dẫn nghiệm thu thủ công

## Chuẩn bị

1. Ghi nhánh, SHA, trình duyệt, viewport, ngày giờ và vai trò. Khởi động IIS Express trên bản Debug hoặc Release đã build theo README.
2. Xác minh server và database trong Web.config cùng `SELECT DB_NAME()`. Chỉ chạy trên `MedicalEquipmentDB_BetaTest`. Đăng nhập bằng tài khoản fixture theo vai trò. Dùng hồ sơ trình duyệt riêng hoặc cửa sổ riêng tư cho từng tài khoản.
3. Chuẩn bị thiết bị/phiếu fixture và ghi mã vào checklist. Mỗi thành viên dùng tiền tố riêng. Tạo tài khoản theo [hướng dẫn an toàn](TestData/README.md). Giữ mật khẩu, Cookie, token và hash ngoài ảnh và log.
4. Ghi PASS khi thực hiện đủ bước và khớp kết quả; FAIL khi đã phát hiện sai; BLOCKED khi thiếu điều kiện; NOT RUN khi chưa thực hiện. Ghi đường dẫn hoặc ảnh đã ẩn danh. Với lỗi tài khoản, ghi độ dài và loại ký tự của mật khẩu; dùng mô tả như «có dấu ngoặc góc».

**SQL A** là [đối chiếu nghiệp vụ](TestData/doi-chieu-nghiep-vu.sql) với mã fixture. **SQL D** là [đối chiếu Dashboard](TestData/doi-chieu-dashboard.sql) với bộ lọc thực tế.

## Tài khoản và giao diện

| Mã | Module · Vai trò | Điều kiện và dữ liệu mẫu | Các bước | Kết quả mong đợi và dấu hiệu lỗi | SQL |
|---|---|---|---|---|---|
| RC-005 | Quản trị · ADMIN | Tên fixture mới, STAFF, mật khẩu 12–256 ký tự có dấu ngoặc góc | Mở Thêm tài khoản, nhập hai mật khẩu khớp, lưu, mở cửa sổ riêng tư và đăng nhập | Tài khoản xuất hiện, có thông báo thành công, đăng nhập được. Lỗi nếu form biến mất, không có bản ghi hoặc được tạo sai role. | A |
| RCA-A01 | Quản trị · ADMIN | Tên đã có, tên khác dành cho các ca nhập sai | Thử tên trùng, bỏ role, mật khẩu ngắn, xác nhận sai lần lượt | Lỗi tiếng Việt đứng cạnh trường. Mật khẩu không được điền lại vào form. | A |
| RC-006 | Quản trị · ADMIN và STAFF | Tài khoản fixture, hai mật khẩu khác nhau, một phiên STAFF đang mở | Đặt lại, xác nhận hộp thoại, thử mật khẩu cũ và mới. Tải lại phía phiên cũ | Thành công, mật khẩu cũ bị từ chối, mật khẩu mới dùng được và phiên cũ hết hiệu lực. Lỗi nếu thiếu thông báo hoặc mật khẩu cũ còn dùng được. | A |
| RC-007 | Đổi mật khẩu · STAFF | Tài khoản fixture đang đăng nhập | Thử mật khẩu hiện tại sai, xác nhận sai, sau đó đổi hợp lệ. Đăng xuất và thử cả hai mật khẩu | Hai lỗi hiện đúng trường. Đổi thành công và chỉ mật khẩu mới đăng nhập được. | A |
| RCA-A02 | Quyền · MANAGER/ENGINEER/STAFF | Mỗi người có phiên riêng | Mở trực tiếp URL Quản lý tài khoản | Trang từ chối hoặc thông báo quyền. FAIL nếu tải được thao tác quản trị. | |
| RC-008 | Giao diện · ADMIN/MANAGER | Form tạo tài khoản và các bộ lọc tài sản | Mở từng select bằng chuột, Tab và phím mũi tên | Font tiếng Việt và mũi tên SVG nhất quán. Menu lựa chọn còn dùng được bằng phím. | |
| RC-009 | Điều hướng · Bốn vai trò | Viewport 1024, 1280, 1366, 1920 px | Dùng Tab và Enter mở từng nhóm có quyền, thử chuyển nhóm, nhấn Escape rồi nhấp ra ngoài | Các mục nằm ngang một hàng. Mỗi lần chỉ mở một nhóm, liên kết cuối ở trong màn hình. STAFF chỉ thấy liên kết đúng quyền. | |
| RC-010 | Responsive · ADMIN | Viewport 360, 390, 768, 1024, 1280, 1366, 1920 px | Mở Login, Dashboard, bảng tài sản, form Import, chi tiết và phiếu. Với màn hình hẹp, mở Menu, mở một nhóm rồi nhấn Escape hai lần | Bảng cuộn trong vùng riêng, ô nhập mobile ≥16px. Escape trả tiêu điểm về nhãn nhóm rồi về nút Menu. Toàn trang vừa viewport. | |
| RC-011 | Thông báo · ADMIN/MANAGER | Một tên trùng và một bản ghi fixture hợp lệ | Gửi form sai rồi đúng. Đọc banner sau redirect, tải lại. Thử hủy hộp thoại đặt lại mật khẩu | Có nhãn chữ, nội dung dễ đọc, thông báo chỉ xuất hiện một lần và thao tác hủy giữ trạng thái. | A |

## Dữ liệu, kho và API

Mỗi ca Excel dùng tài khoản **ADMIN/MANAGER**, một bản sao [tệp tương ứng](TestData/README.md), và một ModelCode đang hoạt động. Mở Thiết bị → Import, tải tệp, đọc kết quả từng dòng rồi tra mã bằng SQL A khi có ghi.

| Mã | Module · Vai trò | Dữ liệu | Thao tác | Kỳ vọng và dấu hiệu lỗi | SQL |
|---|---|---|---|---|---|
| RCA-X01 | Import · ADMIN/MANAGER | hop-le.xlsx | Tải tệp và mở tài sản mới | Lưu đúng một dòng, giá/Serial khớp. FAIL nếu thiếu hoặc sai dữ liệu. | A |
| RCA-X02 | Import · ADMIN/MANAGER | trung-ma-tai-san.xlsx | Tải lần đầu | Dòng trùng mã bị từ chối, chỉ một dòng với mã đó được ghi. | A |
| RCA-X03 | Import · ADMIN/MANAGER | trung-serial.xlsx | Tải lần đầu | Dòng trùng Serial bị từ chối, chỉ một tài sản có Serial đó. | A |
| RCA-X04 | Import · ADMIN/MANAGER | gia-am.xlsx | Tải tệp | Thông báo lỗi giá và không có bản ghi. | A |
| RCA-X05 | Import · ADMIN/MANAGER | sai-model-hoac-khoa.xlsx | Tải tệp | Hai dòng báo sai tham chiếu, không có tài sản mới. | A |
| RCA-X06 | Import · ADMIN/MANAGER | tep-hong.xlsx | Tải tệp | Báo không đọc được Excel và không ghi. | A |
| RCA-X07 | Import · ADMIN/MANAGER | qua-2mb.xlsx | Tải tệp | Báo vượt 2 MiB, không xử lý gói. | A |
| DEV-14 | Nhận diện · Các vai trò tra cứu | Thiết bị fixture và máy quét hỗ trợ Code 128 | Mở QR/Barcode từ chi tiết và quét | Chuỗi giải mã khớp Serial nguyên văn. Thiếu máy quét thì ghi NOT RUN. | A |
| RCA-C01 | Danh mục · ADMIN/MANAGER | Category, Supplier, Model mới; ADMIN dùng riêng Department | Tạo/đọc/sửa/xóa theo thứ tự, thử xóa danh mục khi vẫn có mẫu | CRUD thành công, dòng có tham chiếu được bảo toàn và có thông báo. | |
| RCA-D01 | Thiết bị · ADMIN/MANAGER | Tài sản fixture trạng thái hoạt động, khoa A và B hợp lệ | Tạo và lọc theo Serial, thử đổi khoa trực tiếp, lập điều chuyển sang B | Từ chối đổi khoa trực tiếp, phiếu đúng cập nhật thiết bị và một giao dịch kho. | A |
| RCA-W01 | WorkOrder · STAFF/MANAGER/ENGINEER | Thiết bị fixture và kỹ sư hoạt động | STAFF gửi, MANAGER phân công, ENGINEER bắt đầu và hoàn tất | Trạng thái chuyển đúng và lưu riêng tác nhân. STAFF không tự sửa hoặc đóng phiếu. | A |
| RCA-M01 | Kỹ thuật · ENGINEER | Thiết bị fixture có phiếu được giao | Ghi bảo trì và hiệu chuẩn; thử ngày kế tiếp trước ngày thực hiện | Ghi đúng khi hợp lệ, từ chối ngày sai, thiết bị hiệu chuẩn không đạt có trạng thái hỏng. | A |
| RCA-S01 | Dashboard · ADMIN/MANAGER | Có dữ liệu thiết bị và lịch kỹ thuật | Đổi khoa và năm, so số thiết bị/cảnh báo/chi phí | Tổng số khớp SQL và biểu đồ đổi theo lọc. | D |
| RCA-P01 | API · ADMIN, STAFF, khách | Collection và biến cục bộ, fixture đã chuẩn bị | GET, POST, PUT, DELETE; lặp thử ghi thiếu token và bằng STAFF | Mã 200/201/204 trên đường hợp lệ, 401 khách, 403 STAFF, 400 thiếu token, 404 sau xóa và 409 xung đột. | A |

## Chuyển kết quả cho nhóm

Gửi bảng mã ca, vai trò, SHA, kỳ vọng, thực tế và bằng chứng. Với ca lỗi, mô tả cách lặp lại tối thiểu, tần suất và trạng thái SQL phù hợp. Nhóm đối chiếu [biên bản nghiệm thu](USER_ACCEPTANCE_REPORT.md), cập nhật mã lỗi đã biết trước khi tạo lỗi mới và retest ca liên quan. Dữ liệu fixture có lịch sử được giữ riêng và bàn giao người phụ trách trước khi dọn.
