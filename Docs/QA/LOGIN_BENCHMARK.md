# Đo hiệu năng đăng nhập

## Phương pháp

Chạy IIS Express và SQL Express trên máy Windows hiện tại. Timer HTTP đo POST Login trước redirect. Request đầu tiên của tiến trình IIS là cold. Các request kế tiếp dùng cùng tiến trình. Header `Server-Timing` chỉ bật khi chạy từ loopback và thiết lập `MEDICAL_QA_TIMINGS=1`. Log không ghi mật khẩu, hash, Cookie hoặc token.

Đo PBKDF2 trực tiếp trong Windows PowerShell bằng một vector UTF-8 cố định. Triển khai .NET Framework cũ và Windows CNG đều dùng SHA256, 600.000 vòng, salt 16 byte và khóa 32 byte. Test xác nhận khóa khớp từng byte và mật khẩu sai bị từ chối. Đo SQL riêng bằng ADO.NET với kết nối đã mở và câu SELECT có tham số. Trường `ef_query` gồm cả LINQ, materialization và chi phí kết nối, nên tách khỏi phép đo SQL trực tiếp.

## Kết quả đã ghi nhận

| Phép đo | Beta | Nhánh phát hành |
|---|---:|---:|
| PBKDF2 trực tiếp | 12.389–15.337 ms, bản managed trong các lượt đối chiếu | 2.581–3.357 ms, Windows CNG |
| Login cold HTTP | 23.500 ms cho ADMIN trong QA Beta | 9.433–12.306 ms qua các lượt đã ghi |
| Login warm HTTP | 15.942–17.567 ms cho các vai trò còn lại trong QA Beta | Thường 2.575–4.360 ms, có một lượt ENGINEER 7.799 ms |
| SQL SELECT với kết nối mở sẵn | Chưa đo riêng trong Beta | 4 ms ở lượt sau tiếp tục công việc |
| Phân rã cold ở lượt sau tiếp tục | Chưa đo riêng trong Beta | EF init 2.366 ms, EF query 4.263 ms, PBKDF2 2.848 ms, tổng 9.779 ms |

Ở mốc `24a0244`, lần chạy hồi quy ghi cold 12.935 ms gồm EF init 3.313 ms, EF query 6.146 ms và PBKDF2 3.268 ms. Ba lượt warm MANAGER/ENGINEER/STAFF lần lượt là 3.290/3.761/3.428 ms, truy vấn SQL trên kết nối mở 3 ms. Một lượt chạy song song với các công cụ QA có cold 20.655 ms và PBKDF2 riêng trong HTTP 12.333 ms. Vector managed cũng dao động đến 32.015 ms khi máy có tải song song, còn CNG 2.713 ms ở phép đo trực tiếp cùng lượt. [Log IIS](Logs/final-rc-iis.log)

Các lần chạy trên cùng máy QA có tải nền khác nhau. Đây là mẫu đo thực tế, chưa phải tải đồng thời có kiểm soát. Chi phí cold còn tập trung ở khởi tạo và thực thi truy vấn EF lần đầu. [Bằng chứng Beta](BETA_BUG_REPORT.md)

Lượt hồi quy cuối đạt cold 11.105 ms. EF khởi tạo 2.844 ms, truy vấn đầu tiên 4.975 ms và PBKDF2 3.087 ms. Warm của ba vai trò tiếp theo là 3.189, 3.578 và 3.260 ms. SQL với kết nối mở sẵn mất 3 ms. Vector PBKDF2 managed mất 14.902 ms và CNG mất 2.950 ms, nhanh hơn khoảng 5 lần trong phép đo trực tiếp này. [Log hồi quy](Logs/final-integration.log)
