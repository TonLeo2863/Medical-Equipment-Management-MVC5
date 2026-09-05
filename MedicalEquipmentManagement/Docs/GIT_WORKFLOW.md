# Git workflow

## Branches
- `main`: release/demo.
- `develop`: integration.
- `feature/member1-*`: Auth/Core.
- `feature/member2-*`: Equipment/Catalog.
- `feature/member3-*`: Inventory/Maintenance.
- `feature/member4-*`: API/Report.

## Quy tắc tránh conflict
1. Không commit trực tiếp vào `main`.
2. Không chỉnh cùng một View/Controller của người khác nếu chưa báo.
3. File dùng chung gồm `_Layout.cshtml`, `Web.config`, `RouteConfig.cs`, `WebApiConfig.cs`, `packages.config`: thay đổi cần thông báo trước.
4. Mỗi PR nên nhỏ, một chức năng.
5. PR có mô tả: mục tiêu, file chính, cách test.
6. Reviewer chạy app và test chức năng trước khi approve.
