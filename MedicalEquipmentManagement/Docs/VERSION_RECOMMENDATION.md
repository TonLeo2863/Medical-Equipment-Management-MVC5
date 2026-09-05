# Phiên bản đề xuất

## Nên dùng cho đồ án
- Visual Studio 2019: đúng phần mềm được nêu trong đề cương.
- .NET Framework 4.7.2: phù hợp VS2019 và ASP.NET MVC 5.
- ASP.NET MVC 5.2.9.
- Entity Framework 6.5.1: ưu tiên hơn EF 6.4.4 vì 6.4.4 đã bị NuGet đánh dấu legacy/deprecated.
- ASP.NET Web API 2: dùng bộ package 5.2.9 nếu muốn giữ tương thích ổn định với tài liệu môn học.
- SQL Server 2016: đúng phần mềm được nêu trong đề cương. Có thể dùng SQL Server Express/Developer cùng major compatibility nếu máy cá nhân khác.
- Bootstrap 3.x nếu dùng template MVC 5; không cần chạy theo framework frontend mới.

## Lý do
Đề cương yêu cầu ASP.NET MVC, Entity Framework và Web API; tài liệu thực hành cũng dùng Layout, Partial View, ViewBag/ViewData/ViewModel, DataAnnotations, Session, EF, LINQ và CRUD API. Do đó ưu tiên stack “đúng môn + dễ đồng bộ” hơn stack mới nhất.
