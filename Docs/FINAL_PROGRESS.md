# Tiến độ phát hành

## Checkpoint hiện tại · Navbar cổ điển · 28/11/2026

- Nhánh làm việc `fix/rc-classic-ui`, tạo từ HEAD sạch `dde840fb6e45e6af799a27b4a316375a6f99d1cc` đã đối chiếu trên GitHub. Mốc giao diện tham khảo `793e38690c5467df5cb67b2e3ec13443103a1952`. Commit giao diện mới `23ef0d9`, commit bổ sung nhánh cho CI `3fa9ece`. Lấy SHA bàn giao gồm cả báo cáo bằng `git rev-parse HEAD`.
- So sánh hai mốc cho thấy khung trang, Navbar, màu và minh họa Login là phần trình bày. Form mật khẩu, model binding, CSRF, điều kiện theo role, TempData và JavaScript xử lý gửi form thuộc bản sửa nghiệm thu. Trang chủ, View Dashboard và script Chart.js đã giống nhau ở hai mốc.
- Mã ứng dụng sửa trong `Views/Shared/_Layout.cshtml`, `Views/Shared/_Navbar.cshtml`, `Views/Account/Login.cshtml`, `Content/Site.css`, `Scripts/site.js`. CI sửa bộ lọc nhánh trong `.github/workflows/build.yml`. QA sửa `Run-FinalSmoke.ps1`, `Verify-Responsive.cs`, hướng dẫn nghiệm thu, báo cáo, log và ảnh. Cập nhật README, CHANGELOG, kiến trúc, hướng dẫn mã nguồn, người dùng và ghi chú phát hành. Danh sách tệp đầy đủ bằng `git diff --name-status dde840f..HEAD`.
- Mốc QA `e69efae` lưu bộ kiểm thử và ảnh. Restore 24 gói, build Debug/Release, Razor và IIS/Edge/SQL đạt. Lượt cuối 249 PASS, 0 FAIL, kiểm tra 23 URL nghiệp vụ và hai trang công khai tại bảy chiều rộng. Chú thích điều hướng, responsive và ca kiểm thử mới viết bằng tiếng Việt. [Log](QA/Logs/classic-navbar.log) · [Báo cáo](QA/FINAL_TEST_REPORT.md) · [Ảnh](QA/Screenshots/README.md)
- BetaTest giữ schema, seed và cấu hình. Đã dọn fixture riêng, số dòng và checksum tổng hợp khớp trên 16 bảng trước/sau, không còn tài khoản/tài sản fixture, transaction người dùng khác hoặc IIS Express của lượt chạy.
- Khi tiếp tục, đọc mục này, kiểm tra `git status --short --branch`, đối chiếu SHA với `git ls-remote --heads origin refs/heads/fix/rc-classic-ui` và lượt CI cùng SHA. Việc nghiệm thu còn lại là nhận phản hồi về giao diện trên thiết bị thật. Biên bản thủ công gần nhất vẫn là 34 PASS, 7 FAIL và 50 NOT RUN. [Checklist](QA/MANUAL_ACCEPTANCE_GUIDE.md)

## Nhánh và mốc Git của Final Candidate trước đây

- Nhánh `release/final-candidate`, tạo từ HEAD `origin/beta-projects` đã fetch tại `c9ce363d4aae4ec690054c366f798cfa63ee9d24`.
- Mốc mã nguồn `d94c4eb6d8541ee8ce78dcde654cf6652535f3a6` gồm ứng dụng, migration, chú thích và tài nguyên Chart.js. Commit tài liệu chứa chính checkpoint này đứng sau mốc mã nguồn. Dùng `git rev-parse HEAD` để lấy SHA của commit tài liệu.
- Worktree ban đầu sạch. Fetch xác nhận ba nhánh tính năng là tổ tiên của Beta. PR #5–#7 vẫn mở về `develop`. Không thay đổi nhánh nguồn hoặc PR.

## Phần đã triển khai

| Phạm vi | Thay đổi |
|---|---|
| Tài khoản | PBKDF2-SHA256 qua Windows CNG với nguyên định dạng, 600.000 vòng, salt 16 byte và khóa 32 byte. Tái xác nhận tài khoản/role từ SQL, POST logout có chống CSRF, giao diện quản trị và đổi mật khẩu. |
| Danh mục/tài sản | Quyền ghi MVC và API, validation chung, khoa đã gán đổi qua điều chuyển, xóa mềm và chặn xóa khi có bảo trì hoặc phiếu đang mở. Hạn file Excel, kết quả từng dòng, kiểm tra ký tự Code 128-B. Trang chi tiết có lịch sử kỹ thuật và điều chuyển. |
| Kho và kỹ thuật | Khóa dòng và transaction khi điều chuyển. Tách người báo, kỹ sư được giao và người kết thúc phiếu. STAFF chỉ xem phiếu tự báo, ENGINEER xử lý phiếu được giao. Có bảo trì liên kết phiếu, hiệu chuẩn, cảnh báo 15/30 ngày và lưu lý do hủy phiếu chờ chưa phân công. |
| Dashboard và API | Tổng hợp EF6/LINQ, phạm vi phiếu theo vai trò, thống kê chi phí, Chart.js cục bộ, API dùng JSON, Session và token cho thao tác ghi. |
| Giao diện và tài liệu | Menu nhóm theo role, trạng thái gửi biểu mẫu, bố cục responsive, thông báo tiếng Việt. Có kế hoạch tổng thể, bảng đối chiếu Issues, hướng dẫn mã nguồn, schema, người dùng, Postman, changelog và kế hoạch nhánh. |

## Database và dữ liệu

- Chỉ làm việc với `DESKTOP-AUTD3A3\SQLEXPRESS`, `MedicalEquipmentDB_BetaTest`. Connection string đang dùng Windows Authentication. Không chạy script tạo lại hay seed database gốc.
- Backup `MedicalEquipmentDB_BetaTest_final_candidate_20261128103152.bak` dùng COPY_ONLY và CHECKSUM, đã qua `RESTORE VERIFYONLY` có CHECKSUM; chưa thử restore vật lý vào DB riêng.
- Đã chạy hai lượt `03_FinalCandidate_Workflow.sql` với exit 0; cả bốn khóa ngoại mới được tin cậy. Dữ liệu cũ giữ nguyên mã, tài sản và giá trị ReportedBy, đánh dấu người báo chưa xác minh và chuẩn hóa `Đã hoàn thành` thành `Đã xong`. Thêm người phân công, người kết thúc, kết quả sửa chữa, kỹ thuật viên bảo trì và khóa liên kết phiếu bảo trì.
- Tái hiện SQL 515 khi cố ghi `ReportedBy=NULL` trong transaction đã rollback. Việc tái hiện Beta bằng HTTP/EF với InnerException chưa thực hiện. Các bài kiểm thử dùng fixture tạm riêng để bảo toàn tài khoản QA đã có.

## Kiểm thử và vấn đề đang theo dõi

- PASS NuGet Restore theo phiên bản `packages.config` sau khi chạy lại với mạng. PASS MSBuild Debug/Release ở mốc mã nguồn. Hai lượt Razor trước mốc đều exit 0.
- PASS lượt IIS hồi quy trước các chỉnh sửa cuối với tài khoản tạm cho bốn vai trò. Kiểm tra đăng nhập, từ chối tài khoản bị khóa, mật khẩu sai, API 200/201/204/400/401/403/404/409, điều chuyển đúng vị trí và giao dịch, STAFF tạo phiếu với trường giả mạo bị bỏ qua, phân công/hoàn thành, bảo trì, hiệu chuẩn không đạt, Dashboard và logout POST. GET logout đã được sửa thành 405, retest đạt, giữ Session.
- BETA-004 đã cải thiện. Hai lượt đo cho PBKDF2 managed 13.793–15.337 ms và CNG 2.702–3.357 ms. HTTP cold 9.433–12.306 ms. Lần warm đầu cho bốn role khoảng 2.585–2.641 ms khi máy rảnh hơn và dao động 3.694–7.799 ms ở lượt sau. Cold vẫn có chi phí khởi tạo/truy vấn EF. Chưa chốt mục tiêu thời gian.
- BETA-005 đã qua luồng STAFF và xác nhận các cột bằng SQL trên fixture. Chưa nhận định trạng thái phát hành chung.

## Việc đang làm và thứ tự tiếp tục

1. Đọc checkpoint và chạy `git status --short --branch`, `git log -3 --oneline`. Đối chiếu HEAD với remote trước khi ghi thêm.
2. Xác minh đúng `DB_NAME()` và các cột/khóa ngoại ghi ở trên. Migration đã chạy hai lượt. Chỉ chạy thêm khi phát hiện sai khác schema cần sửa; sao lưu và khóa database đích trước khi thay đổi.
3. Từ kết quả lượt QA cuối dưới đây, tiếp tục đúng ca dừng hoặc thất bại. Lệnh chạy script là `powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File Docs/QA/Run-FinalSmoke.ps1`. Dùng biến môi trường `MEDICAL_QA_BARCODE_DECODER` trỏ tới ZXing.Net 0.16.11 bản net48 nếu muốn giải mã hình, chỉ là công cụ QA ngoài repository.
4. Bổ sung hồi quy các mặt còn thiếu bằng fixture. Bao gồm CRUD Category/Supplier/EquipmentModel/Department, phân trang và thứ tự, HTTP rollback và điều chuyển cạnh tranh, ADMIN cuối cùng và đổi/đặt lại mật khẩu, hết hạn theo thời gian, tệp vượt dung lượng, đối chiếu toàn bộ chỉ số Dashboard, kiểm tra browser ở desktop/tablet/mobile. Xác nhận các tài khoản QA đã có khi có thông tin đăng nhập qua kênh riêng. Seed/schema trên DB trống đang NOT RUN.
5. Hoàn thiện `Docs/QA/FINAL_TEST_REPORT.md`, `FINAL_BUG_REPORT.md`, `FINAL_BUG_REPORT.csv`, `LOGIN_BENCHMARK.md` và `Docs/RELEASE_NOTES.md` bằng kết quả thực thi. Chỉ chốt Final Candidate khi đạt toàn bộ tiêu chí đầu bài. Remember me, quên mật khẩu, quan hệ Users–Departments và API ghi WorkOrder từ bên thứ ba chưa triển khai.

## Quy chuẩn phải giữ khi tiếp tục

- Giữ ASP.NET MVC 5, .NET Framework 4.8, EF6, Razor, LINQ, SQL Server và Web API 2. Giữ tên định danh tiếng Anh và tránh sửa logic chỉ để dịch chú thích.
- Rà hết code do nhóm viết bằng UTF-8. Dùng tiếng Việt có dấu trong chú thích C#, Razor, JavaScript, CSS, SQL. XML Documentation của lớp/hàm trọng yếu giải thích đầu vào, đầu ra, điều kiện, quan hệ module và ngoại lệ. Loại bỏ lời chú thích cũ thừa hoặc sai. Giữ nguyên nội dung thư viện, NuGet và file tự sinh. Rà toàn bộ phần còn sót trước khi build/QA lại.
- Commit theo Conventional Commits với nội dung tiếng Việt. Tài liệu, PR và các báo cáo dùng tiếng Việt, giữ thuật ngữ môn học. Cập nhật tiến độ sau từng mốc và ghi PASS, FAIL, BLOCKED, NOT RUN theo bằng chứng.
- Giữ cấu hình cục bộ và tài khoản QA. Chỉ dùng BetaTest, backup trước khi đổi schema hoặc thao tác có rủi ro mất dữ liệu. Dùng fixture cô lập, ghi nguyên tử, kiểm tra quyền phía server và không lưu bí mật vào Git. Không reset cưỡng chế, xóa nhánh hay merge vào main/develop trước QA.

## Tệp đã thay đổi

- Commit mã nguồn ghi đầy đủ danh sách tệp qua `git show --name-status --format=short d94c4eb`. Vị trí trọng tâm gồm `Controllers/`, `Api/`, `Filters/`, `Helpers/`, `Data/`, `Models/`, `ViewModels/`, `Views/`, `Content/Site.css`, `Scripts/`, `Database/` và `MedicalEquipmentManagement.csproj`.
- Commit checkpoint chứa `README.md`, `CHANGELOG.md`, `Docs/FINAL_PROGRESS.md`, `Docs/PLANNING/FINAL_MASTER_PLAN.md`, `Docs/PLANNING/REQUIREMENTS_TRACEABILITY.md`, `Docs/SOURCE_CODE_GUIDE.md`, `Docs/USER_GUIDE.md`, `Docs/DATABASE/SCHEMA_CHANGES.md`, `Docs/BRANCH_CLEANUP_PLAN.md`, `Docs/QA/Run-FinalSmoke.ps1` và `Docs/QA/Equipment.postman_collection.json`.

## Trạng thái đồng bộ

Hai mốc được giữ trên nhánh phát triển. Dùng `git status --short --branch` để xem thay đổi chưa commit hoặc chênh lệch upstream, và `git ls-remote --heads origin release/final-candidate` để xác nhận SHA đã push. Trạng thái kiểm tra cuối ghi cùng kết quả lượt QA bên dưới.

## Tiếp tục từ checkpoint đã lưu

- Checkpoint tài liệu `6da93b6` đã push lên `origin/release/final-candidate`. Giữ commit mã nguồn `d94c4eb` làm mốc đối chiếu.
- Xác minh lại schema BetaTest bằng truy vấn chỉ đọc. `ReporterVerified` và `AssignedToUserId` có mặt, transaction người dùng đang mở bằng 0 tại thời điểm kiểm tra. Hoàn tất dọn đúng 6 tài khoản và 2 tài sản fixture từ tiến trình QA bị ngắt, đối chiếu ID theo log và không có lịch sử liên quan. Các bộ dữ liệu nghiệp vụ và tài khoản QA cũ được giữ nguyên.
- Bổ sung Cookie gắn với lần đăng nhập, yêu cầu đồng thời Cookie phiên và Cookie HttpOnly khi xác nhận tài khoản, cảnh báo gửi biểu mẫu và rà 113 tệp first-party ở chế độ giải mã UTF-8 nghiêm ngặt. Các regex kiểm tra phổ biến không tìm thấy dấu thay thế hoặc chuỗi mojibake đã biết.
- Mở rộng `Run-FinalSmoke.ps1` để kiểm tra thứ tự trang, Excel hợp lệ/sai/trùng/quá lớn, API giá âm, điều chuyển đồng thời và rollback, kết quả Dashboard, quyền ghi và CRUD danh mục. Thêm kiểm tra Edge WebView2 trong `Docs/QA/Verify-Responsive.cs`.
- Tái hiện đầy đủ BETA-005 trên bản Beta `c9ce363` tách riêng trong thư mục tạm. POST WorkOrder/Create bằng STAFF trả HTTP 500 và chuỗi DbUpdateException → UpdateException → SqlException, cột ReportedBy bắt buộc. Đã dừng IIS Beta và thu hồi fixture. [Log](QA/Logs/final-beta005-reproduction.log)
- Mốc ứng dụng tiếp theo là `24a0244`. Lượt IIS đã có 140 dòng PASS trước khi harness dừng ở phép so tên cột ModelName. Đã sửa harness đối chiếu cột Name và tách lượt danh mục để tiếp tục ca còn thiếu. [Log](QA/Logs/final-rc-iis.log)
- PASS lượt CRUD danh mục, phòng ban và tài sản MVC. PASS Edge WebView2 với Dashboard/Thiết bị ở 390, 768 và 1280 px, gồm điều hướng di động và biểu đồ có dữ liệu. [Log danh mục](QA/Logs/final-catalog.log) · [Log trình duyệt](QA/Logs/final-responsive.log)
- Lượt hồi quy tích hợp cuối trên ứng dụng `24a0244` exit 0, 207 PASS. Kiểm tra Session timeout của bản sao tạm cũng đạt cả trước hạn và sau 75 giây. Không cần chạy lại migration. [Hồi quy](QA/Logs/final-integration.log) · [Session](QA/Logs/final-session-expiry.log)
- Đối soát cuối xác nhận đúng SQL Express/BetaTest, 0 transaction người dùng, 0 tài khoản và tài sản fixture còn sót, 0 tiến trình QA/IIS do phiên này khởi chạy. Bốn khóa ngoại workflow vẫn hoạt động và được tin cậy. Số lượng tài khoản hiện hữu, số tài khoản hoạt động và số hash PBKDF2 theo vai trò khớp số liệu khảo sát.
- Mốc chốt tài liệu bổ sung báo cáo lỗi Markdown/CSV, báo cáo kiểm thử, benchmark và ghi chú phát hành. Sau khi mở lại, kiểm tra `git status --short --branch` và đối chiếu `git ls-remote --heads origin release/final-candidate`. Bước nghiệm thu tiếp theo là xác minh tài khoản QA có sẵn qua kênh riêng, ca ADMIN duy nhất trên fixture được cho phép và các ca NOT RUN trong báo cáo. [Báo cáo QA](QA/FINAL_TEST_REPORT.md)

## Đợt sửa nghiệm thu RC-005–011

- Nhánh `fix/rc-acceptance-ui` tạo từ `release/final-candidate` SHA `793e38690c5467df5cb67b2e3ec13443103a1952`. Worktree ban đầu sạch.
- Edge WebView2 tái hiện ba lỗi ở mật khẩu có dấu ngoặc góc trên bản RC chạy tách riêng. Sửa binding trong `AccountInputs` và `AccountController`, thêm mật khẩu xác nhận khi tạo trong `UserAdminController`. Các view tài khoản hiển thị lỗi từng trường và nút hiện/ẩn.
- Shared Layout/Navbar, Site.css và site.js chuyển sang Sidebar, ngăn di động và thông báo TempData tập trung. View Import sửa nhóm nút và bảng tra cứu ở 360 px.
- Tạo bộ fixture Excel, hai SQL chỉ đọc và hướng dẫn nghiệm thu. Thêm quy chuẩn repo, CODEOWNERS, mẫu PR và workflow Windows. Thay đổi ứng dụng không yêu cầu chỉnh schema hoặc chạy lại migration. Các ca ghi dùng fixture riêng.
- Nghiệm thu thủ công từ người dùng giữ nguyên 34 PASS, 7 FAIL, 50 NOT RUN; DEV-14 thiếu công cụ. Tiếp nhận kết quả mới qua [biên bản](QA/USER_ACCEPTANCE_REPORT.md).

### Checkpoint bàn giao nhánh sửa nghiệm thu

- Restore, Debug, Release và Razor đạt cục bộ. Hồi quy tích hợp 237 PASS, 0 FAIL. Edge WebView2 đã xác minh ba luồng mật khẩu, quyền theo menu, thao tác bàn phím và 23 URL ở sáu viewport. Lượt chỉnh màu/căn checkbox tiếp theo 16 PASS, 0 FAIL. [Báo cáo](QA/FINAL_TEST_REPORT.md) · [Ảnh](QA/Screenshots/README.md)
- Không đổi schema. Đã dọn fixture theo khóa riêng. Xác minh 0 fixture tài khoản/tài sản, 0 transaction người dùng và 0 tiến trình QA/IIS do lượt này tạo.
- Nhóm file ứng dụng gồm AccountController, UserAdminController, AccountInputs, Site.css, site.js và các view. Nhóm kiểm thử gồm Run-FinalSmoke, Verify-Responsive, TestData, log và ảnh. Nhóm quy chuẩn gồm AGENTS/CLAUDE/GEMINI, CONTRIBUTING, tài liệu Docs và .github. Lấy danh sách đầy đủ bằng `git diff --name-status 793e386..HEAD` và các commit bằng `git log --oneline 793e386..HEAD`.
- Khi tiếp tục, kiểm tra SHA local/remote và trạng thái workflow. Thử lại RC-005–011 bằng [hướng dẫn nghiệm thu](QA/MANUAL_ACCEPTANCE_GUIDE.md). Nhóm giữ 34 PASS, 7 FAIL, 50 NOT RUN cho lần nhận phản hồi hiện có. Điểm 8/10 và kết quả checklist bổ sung chờ người dùng đánh giá. Chưa mở PR, merge hoặc tạo Release.
