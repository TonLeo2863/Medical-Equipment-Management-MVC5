# Báo cáo kiểm thử Beta

**Đủ bằng chứng cho demo có hướng dẫn. Chưa đủ điều kiện chốt phát hành.** Bộ mã kiểm thử `d50ee96c6c76e17f1e917c5442d41cc40e0ce0d9`. Windows 10 build 19044, Visual Studio 2022 MSBuild 17.14.60, .NET Framework 4.8, IIS Express, SQL Express 16.0.1000.6. Mọi thao tác dữ liệu QA thực hiện trên `MedicalEquipmentDB_BetaTest`. Hai phiên HTTP xác minh tài khoản ứng dụng có `sysadmin=0`.

## Kết quả thực thi

| Lượt kiểm tra | PASS | FAIL | BLOCKED | Bằng chứng |
|---|---:|---:|---:|---|
| IIS smoke, session và vai trò trước khi chỉnh handler | 79 | 4 | 0 | [HTTP](Logs/http-retest.log) · [IIS](Logs/iis-retest.log) |
| IIS MVC và API sau sửa, lượt cuối với mã tài sản chuẩn hóa | 90 | 0 | 0 | [HTTP](Logs/http-mvc-final.log) |
| Seed trên DB đã có dữ liệu demo, hai lần | 2 | 0 | 0 | [Lượt 1](Logs/seed-retest-1.log) · [Lượt 2](Logs/seed-retest-2.log) |
| Đối chiếu trạng thái sau nghiệp vụ | 1 | 0 | 0 | [SQL](Logs/sql-consistency-stage2.log) |
| MSBuild Debug và Release, Razor precompile | 3 | 0 | 0 | [Debug](Logs/build-debug-stage2.log) · [Release](Logs/build-release-stage2.log) · [Razor](Logs/razor-stage2.log) |
| **Tổng lượt kiểm tra được báo cáo** | **175** | **4** | **0** | Bốn ca FAIL thuộc cùng BETA-003 và đều có ca tương đương đạt sau sửa. |

Đếm từng dòng kiểm tra HTTP, gồm các request mở form và xác minh dữ liệu. Không cộng lại lượt kiểm thử cũ. Lượt MVC trung gian sau sửa đã xác nhận PUT/DELETE, nhưng script QA so mã tài sản phân biệt hoa thường nên bỏ sót tài sản MVC và báo sai số lượng import trùng. Lượt cuối dùng chữ hoa theo chuẩn của ứng dụng, tìm được tài sản MVC, xác nhận đúng một asset import và chạy các ca nghiệp vụ tiếp theo. [Log trung gian](Logs/http-mvc-retest.log).

Lệnh build dùng `MSBuild.exe MedicalEquipmentManagement.sln /p:Configuration=Debug|Release /p:Platform="Any CPU"` với mỗi cấu hình chạy riêng. Dùng các gói đã restore của Beta và `aspnet_compiler -v / -p <thu-muc-ung-dung>`. IIS thử riêng trên loopback.

## Tài khoản và phân quyền

| Vai trò | POST Login | Thời gian | GET đã kiểm tra | POST API với payload rỗng |
|---|---|---:|---|---|
| ADMIN | 302 về / | 23.500 ms | Department 200 bằng session, các danh sách danh mục/kho/bảo trì/WorkOrder 200 | 400 khi thiếu token, 201 khi gửi payload và token hợp lệ |
| MANAGER | 302 về / | 17.432 ms | Danh mục, kho, Equipment, Maintenance, WorkOrder 200. Department 302 AccessDenied | 400 khi thiếu token |
| ENGINEER | 302 về / | 15.942 ms | Equipment, Maintenance, WorkOrder 200. Department, kho và danh mục 302 AccessDenied | 403 |
| STAFF | 302 về / | 17.567 ms | Equipment, WorkOrder 200. Department, kho, danh mục và Maintenance 302 AccessDenied | 403 |
| Guest | GET Login 200 | 11.197 ms ở lần khởi động đầu | Equipment 302 Login. API 401 | 401 |

Cookie token và ASP.NET_SessionId có HttpOnly. Department trả 200 với đúng tên người dùng được hiển thị. Sau logout, cookie tên cũ còn trong cookie jar, nhưng Department chuyển về Login và API trả 401. BETA-002 đã được xác nhận lại. Độ trễ còn theo dõi ở BETA-004. [Log phiên và mã phản hồi](Logs/http-retest.log).

## Đối chiếu phạm vi đồ án

| Chức năng | Đã xác nhận bằng chạy thực tế | Phần còn mở |
|---|---|---|
| SQL schema và seed | Hai lượt seed exit 0, snapshot 15 bảng giống nhau, tồn kho 49, SA-002 giữ phòng ban 1. | Chưa chạy schema từ DB trống trong đợt này. |
| Category, Supplier, EquipmentModel | Tạo, đọc, sửa, xóa bằng MVC. Mã Category trùng bị validation chặn. Xóa Category khi còn Model tham chiếu giữ lại Category. | Cần mở rộng các tổ hợp FK và đầu vào lỗi. |
| EquipmentAsset | Tạo, xem, sửa qua MVC. Giá âm bị chặn. Tìm theo mã và lọc phòng ban kiểm tra bằng nội dung, xóa asset có lịch sử bị chặn. API xóa mềm và GET sau xóa trả 404. | Đã mở URL sắp xếp theo giá và trang 2, cần xác nhận thứ tự, ranh giới trang và xóa thành công từ MVC. |
| Điều chuyển, kho | Sai phòng nguồn và nguồn bằng đích bị chặn. Điều chuyển đúng cập nhật phòng ban, lịch sử, người yêu cầu và giao dịch kho. | Chưa chủ động gây lỗi giao dịch để xác minh rollback. |
| Bảo trì và WorkOrder | Tạo bản ghi bảo trì và phiếu sửa chữa. Xác nhận lịch sử và trạng thái tài sản chuyển sang đang bảo trì, đối chiếu SQL. | Chưa kiểm hết chuyển trạng thái phiếu, ngày/chi phí biên và các hành vi đóng phiếu. |
| Web API | GET collection/by id, POST 201, PUT 204, DELETE 204. JSON, 400, 401, 403, 404 và 409 đã chạy. Giá PUT lưu được, GET sau soft delete trả 404. | Cần mở rộng test cạnh tranh, FK và ma trận theo từng phương thức/role. |
| Import Excel | Mẫu XLSX tải được, file hợp lệ tạo asset, gửi lại cùng file không nhân đôi mã. Gửi workbook mã model sai, file hỏng và đuôi sai nhận lại HTTP 200. | Chưa đối chiếu đầy đủ thông báo và số dòng bị loại cho từng file lỗi, chưa thử vượt giới hạn dung lượng. |
| QR / Barcode | HTTP 200, MIME image/png và chữ ký PNG cho cả hai. | Chưa quét ảnh để đối chiếu nội dung mã. |
| Razor, tiếng Việt và menu | Razor precompile exit 0. Home, About, trang danh sách chính và các form đã tải. About có chuỗi tiếng Việt UTF-8 và thẻ viewport. | Chưa kiểm tra breakpoint bằng mắt và chưa bấm hết mọi mục menu của từng vai trò. |
| Đăng nhập, phân quyền | Bốn vai trò đăng nhập, GET trực tiếp/một số POST API, logout và session đã kiểm tra. | Cần rà quyền ghi MVC theo từng action/vai trò. Login chậm 16–24 giây trong QA. |

## NOT RUN và cách kiểm thử thủ công

1. **NOT RUN · Seed DB trống.** Chỉ dùng môi trường QA có DB đúng tên `MedicalEquipmentDB_BetaTest`. Kiểm tra `DB_NAME()`, bỏ CREATE DATABASE và USE gốc trong bản sao, dùng user giới hạn `beta_schema_qa`. Với DB đã có dữ liệu, dùng hai lượt như log hiện tại. Khi có môi trường BetaTest trống, đối chiếu thêm seed Module 3 và Module 2.
2. **NOT RUN · Quyền ghi MVC đầy đủ.** Đăng nhập từng vai trò theo bảng, mở form bằng URL trực tiếp và gửi token hợp lệ trên dữ liệu demo. Kiểm tra status, redirect, trạng thái DB.
3. **NOT RUN · Sắp xếp/phân trang và FK mở rộng.** Thêm đủ dòng demo để vượt kích thước trang. Đối chiếu giá/tên giữa hai trang, thử FK hợp lệ và không tồn tại, rồi kiểm tra bản ghi sau mỗi thao tác.
4. **NOT RUN · Rollback và vòng đời WorkOrder.** Dùng fixture cô lập, khiến ghi lịch sử điều chuyển thất bại theo điều kiện có kiểm soát. So sánh vị trí, lịch sử và giao dịch kho trước và sau. Chạy tiếp vòng đời phiếu và trạng thái bảo trì.
5. **NOT RUN · Kiểm tra giao diện và đầu vào biên.** Dùng browser desktop và mobile để mở menu mỗi vai trò, quét QR/Barcode, kiểm tra nội dung tiếng Việt. Thử Excel kích thước vượt giới hạn, đọc lỗi mỗi dòng và so số bản ghi.

## An toàn SQL

Đã dùng bản sao UTF-8 và khóa đích SQL bằng `IF DB_NAME() <> N'MedicalEquipmentDB_BetaTest' THROW ...`. Thực thi seed qua `sqlcmd -b` và impersonate user chỉ có quyền trong DB QA. Giá trị tồn kho và phòng hiện tại không bị ghi lại trong seed mới. Tài khoản đăng nhập và tài sản QA là fixture tổng hợp. Tài khoản SQL tạm được thu hồi sau IIS và cấu hình tạm được khôi phục.
