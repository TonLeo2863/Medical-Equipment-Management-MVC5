# Rà soát PR trước khi tích hợp Beta

Kiểm tra trực tiếp GitHub ngày 21/11/2026 bằng API REST và `git fetch origin --prune`. Nhánh mặc định và đích của cả ba PR là `develop` tại `1aa1b8ea1052048637db50b21d0cbccc49ed3243`. Danh sách toàn bộ PR trả về #5, #6 và #7. Chưa có PR riêng cho Web API hoặc Dashboard.

| PR | Tác giả GitHub | Nhánh nguồn | SHA | File | Phạm vi và tình trạng đọc mã |
|---|---|---|---|---:|---|
| [#5](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/5) | Thien7677 | feature/module3-kho-baotri | d367764af2f55b7cda8ba6be1e1cd83ac425d466 | 43 | Open. Department, Inventory, Maintenance, WorkOrder, mapping EF và schema. Điều chuyển cần xác minh phòng nguồn và người thực hiện. |
| [#6](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/6) | chaukhaivinh | feature/module1_account_layout | f0ead93a0f2ee34b35440b554a7301902d896483 | 51 | Open. AccountController, filter, Navbar, Error và layout. Chứa cả hai commit #5. Mã nguồn dùng MD5 và fallback plaintext, view 500 sai tên. |
| [#7](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/7) | trgtien74 | feature/member2-equipment-catalog | aea606b7ac0d56421dc9d6ea07876567841f191f | 47 | Open. CRUD Category, Supplier, EquipmentModel và EquipmentAsset, import Excel, QR/Barcode, seed, .NET 4.8. Phân kỳ từ develop và thay đổi các model dùng chung. |

Các endpoint review, comment, check runs và commit status của các PR được truy vấn. Không có bằng chứng CI đạt để dùng thay kết quả build. `git ls-remote --heads origin beta-projects` trả về danh sách rỗng trước khi tạo nhánh. Các tệp rà soát chưa theo dõi trong thư mục gốc được giữ tại chỗ. Dùng worktree độc lập từ origin/develop và merge lần lượt #5, #6, #7. Web API có mã khung tại develop, cần bổ sung riêng trong Beta.
