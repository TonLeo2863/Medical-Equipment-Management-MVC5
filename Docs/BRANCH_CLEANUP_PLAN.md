# Kế hoạch giữ và dọn nhánh

| Nhánh | Mốc khảo sát | Khuyến nghị |
|---|---|---|
| `main` | `a4347f0` | Giữ nhánh bàn giao. |
| `develop` | `1aa1b8e` | Giữ nhánh tích hợp và đích của ba PR nguồn. |
| `beta-projects` | `c9ce363` | Giữ mốc Beta để so sánh và phục hồi. |
| `release/final-candidate` | Gốc `c9ce363` | Giữ nhánh phát hành và bằng chứng QA. |
| `feature/module3-kho-baotri` | `d367764` | Giữ trong khi PR #5 mở. |
| `feature/module1_account_layout` | `f0ead93` | Giữ trong khi PR #6 mở. |
| `feature/member2-equipment-catalog` | `aea606b` | Giữ trong khi PR #7 mở. |

`git branch -r --merged HEAD` tại mốc tạo nhánh xác nhận cả ba tip tính năng là tổ tiên của nhánh RC. [PR #5](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/5) · [PR #6](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/6) · [PR #7](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/7)

Trước khi đề xuất xóa, fetch lại và chạy `git merge-base --is-ancestor origin/<nhánh> origin/<nhánh giữ>`, so `git log <nhánh giữ>..<nhánh>`, kiểm tra các worktree và xác nhận kết quả từng PR. Chỉ xem xét dọn nhánh tính năng sau khi đã bàn giao lịch sử và nhóm thống nhất xử lý PR.