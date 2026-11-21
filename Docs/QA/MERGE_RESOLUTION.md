# Quyết định hợp nhất

| Bước | Commit | Kết quả |
|---|---|---|
| PR #5 | 1acf895d3c4b3b3b5134e87f6c182a90f17420fa | Merge tự động, rà stat và thay đổi kho/schema. |
| PR #6 | 6a911912da4154f4f415e896e0b98348acae018f | Merge tự động, chỉ bổ sung diff account/layout so với bước trước. |
| PR #7 | acb08f200f92ebb298cf47920bf9e01dceb27866 | Chín file xung đột, giữ cả hai parent. |

| File | Quyết định và tác động | Kiểm tra |
|---|---|---|
| Data/MedicalEquipmentContext.cs | Giữ DbSet đã có khóa của kho và các quy ước EF. Ghi chú về model chưa có khóa của nhánh cũ không còn áp dụng. | Build và các trang truy vấn EF trong log HTTP. |
| Models/Department.cs | Giữ thuộc tính phòng ban và khởi tạo collection đã ghép. Hunk xung đột ở dòng using chỉ khác BOM. | Build, Razor và trang Department. |
| Models/EquipmentAsset.cs | Kết hợp mapping, validation, soft-delete, giá và ngày mua với navigation CalibrationLogs và MaintenanceRecords. | Build, Razor và trang Equipment. |
| Models/EquipmentCategory.cs, EquipmentModel.cs, Supplier.cs | Dùng cột schema thật, gồm mã, trạng thái và khóa ngoại ManufacturerId. View kho đọc tên từ ManufacturerSupplier. | Build, Razor và các trang danh mục. |
| MedicalEquipmentManagement.csproj | Gộp Compile và Content của cả ba module. Giữ .NET Framework 4.8, đồng bộ Web.config và packages.config ở commit tiếp theo. | Debug và Release, Razor precompile. |
| Views/Equipment/Index.cshtml | Giữ view danh sách tài sản đúng kiểu trả về từ EquipmentController. Danh mục dòng thiết bị có EquipmentModelController và view riêng. | Build, Razor và log HTTP. |
| Views/Shared/_Navbar.cshtml | Giữ menu account và kho theo vai trò, bổ sung link danh mục và nhà cung cấp, bỏ link thiết bị trùng. | Razor. Kiểm tra toàn bộ link theo vai trò còn trong QA. |

Site.css, MaintenanceRecord và SQL được rà sau merge tự động. Bộ schema chính giữ bản có các cột bảo trì của Module 3, seed Module 2 là file riêng. Chạy `git diff --cached --check` trước khi chốt xung đột.
