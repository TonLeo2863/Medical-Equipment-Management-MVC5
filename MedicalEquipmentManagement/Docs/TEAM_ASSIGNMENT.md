# PHÂN CÔNG 4 THÀNH VIÊN

## Member 1 – Core/Auth + khung MVC
Phụ trách phần dùng chung và tài khoản. Tránh sửa các Controller nghiệp vụ của Member 2/3/4.

1. Chuẩn hoá project MVC 5, RouteConfig, BundleConfig, FilterConfig.
2. Thiết kế `_Layout.cshtml`.
3. Tạo các Partial View: menu, user menu, notification.
4. Xây dựng Model `User` và `Role` ở mức cần thiết.
5. Đăng nhập.
6. Đăng xuất.
7. Lưu người dùng đăng nhập vào Session.
8. Cookie “Remember me” ở mức cơ bản nếu nhóm chọn làm.
9. DataAnnotations cho form tài khoản.
10. Phân quyền Manager/Staff bằng filter hoặc attribute.
11. Trang dashboard khung.
12. Thông báo lỗi/thành công dùng TempData.
13. Validation phía server.
14. Tìm hiểu và ghi chú vòng đời Request-Response MVC.
15. Cấu hình route chuẩn `/Controller/Action/{id}`.
16. Không đặt truy vấn DB trực tiếp trong View.
17. Chỉ dùng ViewModel cho form cần nhiều dữ liệu.
18. Tạo `AccountController`.
19. Tạo `Authorize`/custom authorization phù hợp.
20. Test login sai/đúng/đăng xuất.
21. Test Session hết hạn.
22. Test quyền Staff/Manager.
23. Viết README phần chạy tài khoản mẫu.
24. Không sửa DB schema sau khi đã chốt nếu chưa trao đổi.
25. Hỗ trợ merge conflict của các file `App_Start` và `Views/Shared`.
26. Review tối thiểu 1 PR/người khác.
27. Tạo seed dữ liệu tài khoản demo.
28. Kiểm tra chống null cơ bản trên form.
29. Ghi screenshot chức năng cho báo cáo.
30. Bàn giao API hợp đồng tên Role/User cho Member 4 nếu cần.

## Member 2 – Quản lý danh mục + thiết bị
Phụ trách `Category`, `Equipment`, `Supplier` và giao diện MVC nghiệp vụ.

1. Thiết kế Model `EquipmentCategory`.
2. Thiết kế Model `Equipment`.
3. Thiết kế Model `Supplier`.
4. Khoá chính, khoá ngoại rõ ràng.
5. DataAnnotations cho mã thiết bị, tên, giá trị, ngày mua.
6. CRUD danh mục thiết bị.
7. CRUD thiết bị.
8. CRUD nhà cung cấp.
9. Trang danh sách thiết bị.
10. Tìm kiếm gần đúng theo tên/mã.
11. Lọc theo danh mục.
12. Lọc theo trạng thái.
13. Sắp xếp theo tên/giá/ngày mua.
14. Pagination đơn giản nếu cần.
15. Trang chi tiết thiết bị.
16. Upload/hiển thị ảnh thiết bị nếu nhóm chọn.
17. Hiển thị quan hệ Category/Supplier.
18. Dùng LINQ cho truy vấn tìm kiếm/lọc.
19. Dùng strongly typed ViewModel ở form phức tạp.
20. Tạo `EquipmentController`.
21. Tạo views Index/Details/Create/Edit/Delete.
22. Không sửa `AccountController`.
23. Không sửa `InventoryController`.
24. Test CRUD đủ 4 trạng thái.
25. Test mã thiết bị trùng.
26. Test dữ liệu bắt buộc.
27. Test tìm kiếm không có kết quả.
28. Test filter kết hợp.
29. Chuẩn bị dữ liệu demo tối thiểu 20 thiết bị.
30. Viết mô tả chức năng cho báo cáo.

## Member 3 – Kho, phòng ban + bảo trì
Phụ trách nghiệp vụ vận hành, tồn kho và bảo trì.

1. Thiết kế Model `Department`.
2. Thiết kế Model `InventoryTransaction`.
3. Thiết kế Model `MaintenanceRecord`.
4. CRUD phòng ban.
5. Nhập kho thiết bị.
6. Xuất/chuyển thiết bị.
7. Cập nhật trạng thái thiết bị.
8. Theo dõi số lượng/tình trạng.
9. Ghi nhận ngày nhập.
10. Ghi nhận vị trí/phòng ban sử dụng.
11. Tạo lịch sử bảo trì.
12. Tạo form thêm bảo trì.
13. Trang lịch sử bảo trì theo thiết bị.
14. Lọc thiết bị sắp đến hạn bảo trì.
15. LINQ thống kê thiết bị theo trạng thái.
16. LINQ thống kê theo phòng ban.
17. Kiểm tra không cho xuất vượt tồn.
18. Kiểm tra không cho xoá thiết bị đang có giao dịch nếu nhóm quyết định ràng buộc.
19. Tạo `InventoryController`.
20. Tạo `MaintenanceController`.
21. Tạo views danh sách/chi tiết/thêm/sửa.
22. Dùng TempData cho thông báo giao dịch.
23. Test nhập kho.
24. Test xuất kho.
25. Test tồn kho âm.
26. Test chuyển phòng ban.
27. Test thêm bảo trì.
28. Test lọc bảo trì đến hạn.
29. Chuẩn bị dữ liệu giao dịch demo.
30. Viết mô tả nghiệp vụ và ảnh màn hình báo cáo.

## Member 4 – Web API + thống kê + tích hợp
Phụ trách Chapter 5 của đề cương và các trang thống kê.

1. Cấu hình `WebApiConfig`.
2. Xây dựng API Controller.
3. API GET danh sách thiết bị.
4. API GET thiết bị theo id.
5. API POST thiết bị.
6. API PUT thiết bị.
7. API DELETE thiết bị.
8. Trả JSON.
9. HTTP status hợp lý.
10. Routing API rõ ràng.
11. DTO cho API nếu cần.
12. Validate dữ liệu đầu vào.
13. Test API bằng Postman.
14. Lưu collection Postman trong `Docs/Postman`.
15. API tìm kiếm thiết bị.
16. API lọc theo category.
17. API thống kê thiết bị theo trạng thái.
18. Trang dashboard số liệu.
19. Thống kê tổng số thiết bị.
20. Thống kê đang sử dụng/đang bảo trì/hỏng.
21. Thống kê theo phòng ban.
22. Dùng LINQ cho thống kê.
23. Không viết lại business logic đã có của Member 2/3 nếu có thể tái sử dụng.
24. Chỉ sửa `Api/`, `Docs/Postman/` và module report trừ khi được đồng ý.
25. Test API 200/400/404.
26. Test POST dữ liệu thiếu.
27. Test PUT id không tồn tại.
28. Test DELETE.
29. Ghi endpoint và request/response mẫu.
30. Chuẩn bị phần demo Chapter 5 cho thuyết trình.

## Quy ước tích hợp
- `main`: ổn định.
- `develop`: nhánh tích hợp.
- Mỗi người làm feature branch từ `develop`.
- PR chỉ merge vào `develop`.
- Chỉ trưởng nhóm/maintainer merge `develop -> main` sau khi test.
