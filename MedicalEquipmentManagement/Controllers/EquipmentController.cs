using MedicalEquipmentManagement.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>
    /// Controller Quản lý Tài sản thực tế (EquipmentAsset):
    /// 1. Danh sách có Tìm kiếm (theo Tên/Serial), Lọc (theo Khoa/Trạng thái) và Phân trang (Pagination).
    /// 2. CRUD sử dụng EquipmentAssetViewModel để truyền DropdownList (Model và Department) ra View, không dùng ViewBag.
    /// 3. Bắt lỗi và hiện thông báo thân thiện khi cố XÓA thiết bị đang có lịch sử bảo trì (DB chặn Cascade Delete).
    /// 4. Phần mở rộng: Import danh sách thiết bị từ Excel (.xlsx) & Tự động sinh mã vạch (Barcode/QR Code) từ Số Serial.
    /// </summary>
    [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER,STAFF")]
    public class EquipmentController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // ============================================================
        // 1. DANH SÁCH THIẾT BỊ: TÌM KIẾM + LỌC (KHOA/TRẠNG THÁI) + PHÂN TRANG
        // ============================================================
        public ActionResult Index(string keyword, int? departmentId, string status, string sortBy = "code", string sortDir = "asc", int page = 1)
        {
            const int pageSize = 8;
            if (page < 1) page = 1;

            var query = db.EquipmentAssets
                .Include(a => a.EquipmentModel.Category)
                .Include(a => a.EquipmentModel.ManufacturerSupplier)
                .Include(a => a.Department)
                .Include(a => a.MaintenanceRecords)
                .Where(a => !a.IsDeleted);

            // Tìm kiếm theo Tên mẫu thiết bị, Mã tài sản hoặc Số Serial
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(a =>
                    a.SerialNumber.Contains(keyword) ||
                    a.AssetCode.Contains(keyword) ||
                    a.EquipmentModel.ModelName.Contains(keyword));
            }

            // Lọc theo Khoa / Phòng ban
            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId.Value);
            }

            // Lọc theo Trạng thái
            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                query = query.Where(a => a.Status == status);
            }

            // Phân trang (Pagination)
            int totalItems = query.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalItems / pageSize));
            if (page > totalPages) page = totalPages;

            // Sắp xếp theo mã/tên/giá/ngày mua (mặc định: mã tài sản tăng dần)
            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            sortDir = desc ? "desc" : "asc";
            IOrderedQueryable<EquipmentAsset> ordered;
            switch ((sortBy ?? "code").ToLowerInvariant())
            {
                case "name":
                    sortBy = "name";
                    ordered = desc ? query.OrderByDescending(a => a.EquipmentModel.ModelName) : query.OrderBy(a => a.EquipmentModel.ModelName);
                    break;
                case "price":
                    sortBy = "price";
                    ordered = desc ? query.OrderByDescending(a => a.PurchasePrice) : query.OrderBy(a => a.PurchasePrice);
                    break;
                case "date":
                    sortBy = "date";
                    ordered = desc ? query.OrderByDescending(a => a.PurchaseDate) : query.OrderBy(a => a.PurchaseDate);
                    break;
                default:
                    sortBy = "code";
                    ordered = desc ? query.OrderByDescending(a => a.AssetCode) : query.OrderBy(a => a.AssetCode);
                    break;
            }

            var pagedItems = ordered
                .ThenBy(a => a.AssetId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new EquipmentAssetIndexViewModel
            {
                Keyword = keyword,
                DepartmentId = departmentId,
                Status = status,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = pagedItems
            };

            PopulateIndexFilterLists(vm);
            return View(vm);
        }

        // ============================================================
        // 2. XEM CHI TIẾT THIẾT BỊ + QR CODE / BARCODE + LỊCH SỬ BẢO TRÌ
        // ============================================================
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var asset = db.EquipmentAssets
                .Include(a => a.EquipmentModel.Category)
                .Include(a => a.EquipmentModel.ManufacturerSupplier)
                .Include(a => a.Department)
                .Include(a => a.MaintenanceRecords)
                .FirstOrDefault(a => a.AssetId == id.Value && !a.IsDeleted);

            if (asset == null)
            {
                return HttpNotFound();
            }

            var role = (string)Session["RoleCode"];
            var userId = (int)Session["UserId"];
            var orders = db.WorkOrders.Where(w => w.AssetId == asset.AssetId);
            if (role == "STAFF")
                orders = orders.Where(w => w.ReporterVerified && w.ReportedByUserId == userId);
            else if (role == "ENGINEER")
                orders = orders.Where(w => w.AssignedToUserId == userId);
            ViewBag.WorkOrderHistory = orders.OrderByDescending(w => w.CreatedDate).ToList();
            ViewBag.CalibrationHistory = db.CalibrationLogs.Where(c => c.AssetId == asset.AssetId)
                .OrderByDescending(c => c.CalibrationDate).ThenByDescending(c => c.CalibrationId).ToList();
            ViewBag.TransferHistory = db.AssetTransfers.Include(t => t.FromDepartment).Include(t => t.ToDepartment)
                .Where(t => t.AssetId == asset.AssetId).OrderByDescending(t => t.TransferDate).ThenByDescending(t => t.TransferId).ToList();
            return View(asset);
        }

        // ============================================================
        // 3. THÊM MỚI THIẾT BỊ (BẮT BUỘC DÙNG EquipmentAssetViewModel)
        // ============================================================
        [HttpGet, CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Create(int? modelId, int? departmentId)
        {
            var vm = new EquipmentAssetViewModel
            {
                ModelId = modelId,
                DepartmentId = departmentId,
                PurchaseDate = DateTime.Today,
                Status = AssetStatus.Active
            };

            PopulateFormDropdownLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Create(EquipmentAssetViewModel vm)
        {
            vm.AssetId = 0;
            ValidateAssetFields(vm, null);
            ValidateAssetUniqueness(vm);

            if (ModelState.IsValid)
            {
                var asset = new EquipmentAsset();
                vm.ApplyToEntity(asset);

                db.EquipmentAssets.Add(asset);
                if (!SaveAsset())
                {
                    PopulateFormDropdownLists(vm);
                    return View(vm);
                }

                TempData["Success"] = $"Đã thêm thiết bị [{asset.AssetCode}] (Số Serial: {asset.SerialNumber}) thành công.";
                return RedirectToAction("Index");
            }

            PopulateFormDropdownLists(vm);
            return View(vm);
        }

        // ============================================================
        // 4. SỬA THIẾT BỊ (BẮT BUỘC DÙNG EquipmentAssetViewModel)
        // ============================================================
        [HttpGet, CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var asset = FindActiveAsset(id.Value);
            if (asset == null)
            {
                return HttpNotFound();
            }

            var vm = EquipmentAssetViewModel.FromEntity(asset);
            ViewBag.FixedDepartment = asset.DepartmentId.HasValue ? asset.Department?.Name : null;
            PopulateFormDropdownLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Edit(EquipmentAssetViewModel vm)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                db.LockAsset(vm.AssetId);
                var asset = FindActiveAsset(vm.AssetId);
                if (asset == null)
                    return HttpNotFound();

                ViewBag.FixedDepartment = asset.DepartmentId.HasValue ? asset.Department?.Name : null;
                ValidateAssetFields(vm, asset);
                ValidateAssetUniqueness(vm);
                var changeError = EquipmentPolicy.ValidateChange(db, vm, asset);
                if (changeError != null)
                    ModelState.AddModelError("", changeError);
                if (ModelState.IsValid)
                {
                    vm.ApplyToEntity(asset);
                    if (SaveAsset())
                    {
                        transaction.Commit();
                        TempData["Success"] = $"Đã cập nhật thiết bị [{asset.AssetCode}].";
                        return RedirectToAction("Index");
                    }
                }
                if (asset.DepartmentId.HasValue)
                {
                    ModelState.Remove(nameof(vm.DepartmentId));
                    vm.DepartmentId = asset.DepartmentId;
                }
            }
            PopulateFormDropdownLists(vm);
            return View(vm);
        }

        // ============================================================
        // 5. XÓA THIẾT BỊ & BẮT LỖI KHÓA NGOẠI LỊCH SỬ BẢO TRÌ THÂN THIỆN
        // ============================================================
        [HttpGet, CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var asset = db.EquipmentAssets
                .Include(a => a.EquipmentModel.Category)
                .Include(a => a.Department)
                .Include(a => a.MaintenanceRecords)
                .FirstOrDefault(a => a.AssetId == id.Value && !a.IsDeleted);

            if (asset == null)
            {
                return HttpNotFound();
            }

            return View(asset);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                db.LockAsset(id);
                var asset = FindActiveAsset(id);
                if (asset == null)
                    return HttpNotFound();
                var conflict = EquipmentPolicy.RemovalConflict(db, id);
                if (conflict != null)
                {
                    TempData["Error"] = conflict;
                    return RedirectToAction("Index");
                }
                asset.IsDeleted = true;
                db.SaveChanges();
                transaction.Commit();
                TempData["Success"] = $"Đã xóa mềm thiết bị [{asset.AssetCode}].";
            }
            return RedirectToAction("Index");
        }

        // ============================================================
        // 6. PHẦN MỞ RỘNG: TỰ ĐỘNG SINH QR CODE & BARCODE TỪ SỐ SERIAL
        // ============================================================
        public ActionResult QrCode(int id)
        {
            var asset = FindActiveAsset(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            byte[] pngBytes = BarcodeQrHelper.GenerateQrCodePng(asset.SerialNumber, 10);
            return File(pngBytes, "image/png");
        }

        public ActionResult Barcode(int id)
        {
            var asset = FindActiveAsset(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            if (!BarcodeQrHelper.CanEncodeCode128(asset.SerialNumber))
                return new HttpStatusCodeResult(422, "Legacy serial cannot be encoded with Code 128 B.");
            byte[] pngBytes = BarcodeQrHelper.GenerateCode128BarcodePng(asset.SerialNumber, asset.AssetCode);
            return File(pngBytes, "image/png");
        }

        // ============================================================
        // 7. PHẦN MỞ RỘNG: IMPORT DANH SÁCH THIẾT BỊ TỪ FILE EXCEL
        // ============================================================
        [HttpGet, CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Import()
        {
            return View(BuildImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult Import(HttpPostedFileBase excelFile)
        {
            var vm = BuildImportViewModel();

            if (excelFile == null || excelFile.ContentLength <= 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn một file Excel (.xlsx) để import.");
                return View(vm);
            }

            if (excelFile.ContentLength > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File Excel không được vượt quá 2 MB.");
                return View(vm);
            }
            var extension = Path.GetExtension(excelFile.FileName);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Chỉ hỗ trợ định dạng file Excel .xlsx.");
                return View(vm);
            }

            vm.HasProcessed = true;
            vm.FileName = Path.GetFileName(excelFile.FileName);

            var modelLookup = db.EquipmentModels.Where(m => m.IsActive)
                .ToList()
                .GroupBy(m => (m.ModelCode ?? "").Trim().ToUpperInvariant())
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key, g => g.First());

            var deptLookup = db.Departments.Where(d => d.IsActive)
                .ToList()
                .GroupBy(d => (d.DepartmentCode ?? "").Trim().ToUpperInvariant())
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key, g => g.First());

            var existingAssetCodes = new HashSet<string>(
                db.EquipmentAssets.Select(a => a.AssetCode).ToList().Where(c => c != null).Select(c => c.Trim().ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase);

            var existingSerials = new HashSet<string>(
                db.EquipmentAssets.Select(a => a.SerialNumber).ToList().Where(s => s != null).Select(s => s.Trim().ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var package = new ZipArchive(excelFile.InputStream, ZipArchiveMode.Read, true))
                {
                    if (package.Entries.Count > 300 || package.Entries.Sum(entry => entry.Length) > 24L * 1024 * 1024
                        || package.GetEntry("xl/workbook.xml") == null)
                        throw new InvalidDataException();
                }
                excelFile.InputStream.Position = 0;
                using (var workbook = new XLWorkbook(excelFile.InputStream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError("", "File Excel không hợp lệ.");
                        return View(vm);
                    }

                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                    if (lastRow > 2001)
                    {
                        ModelState.AddModelError("", "File Excel chỉ được chứa tối đa 2.000 dòng dữ liệu.");
                        return View(vm);
                    }
                    var assetsToAdd = new List<EquipmentAsset>();

                    for (int rowIdx = 2; rowIdx <= lastRow; rowIdx++)
                    {
                        var row = worksheet.Row(rowIdx);
                        string assetCode = row.Cell(1).GetString().Trim().ToUpperInvariant();
                        string serialNumber = row.Cell(2).GetString().Trim();
                        string modelCode = row.Cell(3).GetString().Trim().ToUpperInvariant();
                        string deptCode = row.Cell(4).GetString().Trim().ToUpperInvariant();
                        string purchaseDateStr = row.Cell(5).GetString().Trim();
                        string purchasePriceStr = row.Cell(6).GetString().Trim();
                        string statusStr = row.Cell(7).GetString().Trim();

                        if (row.IsEmpty())
                        {
                            continue;
                        }

                        vm.TotalRowsRead++;

                        var rowResult = new EquipmentImportRowResult
                        {
                            RowNumber = rowIdx,
                            AssetCode = assetCode,
                            SerialNumber = serialNumber,
                            ModelCode = modelCode,
                            DepartmentCode = deptCode,
                            Status = string.IsNullOrEmpty(statusStr) ? AssetStatus.Active : statusStr
                        };

                        if (string.IsNullOrEmpty(assetCode) || string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(modelCode)
                            || assetCode.Length > 50 || serialNumber.Length > 100 || !BarcodeQrHelper.CanEncodeCode128(serialNumber))
                        {
                            rowResult.IsSuccess = false;
                            rowResult.Message = "Kiểm tra mã tài sản (tối đa 50 ký tự), Serial (tối đa 100 ký tự ASCII) và mã mẫu.";
                            vm.RowResults.Add(rowResult);
                            vm.SkippedOrErrorCount++;
                            continue;
                        }

                        if (existingAssetCodes.Contains(assetCode))
                        {
                            rowResult.IsSuccess = false;
                            rowResult.Message = $"Mã tài sản '{assetCode}' đã tồn tại.";
                            vm.RowResults.Add(rowResult);
                            vm.SkippedOrErrorCount++;
                            continue;
                        }

                        if (existingSerials.Contains(serialNumber.ToUpperInvariant()))
                        {
                            rowResult.IsSuccess = false;
                            rowResult.Message = $"Số Serial '{serialNumber}' đã tồn tại.";
                            vm.RowResults.Add(rowResult);
                            vm.SkippedOrErrorCount++;
                            continue;
                        }

                        EquipmentModel matchedModel;
                        if (!modelLookup.TryGetValue(modelCode, out matchedModel))
                        {
                            rowResult.IsSuccess = false;
                            rowResult.Message = $"Không tìm thấy Mã Model '{modelCode}'.";
                            vm.RowResults.Add(rowResult);
                            vm.SkippedOrErrorCount++;
                            continue;
                        }

                        int? departmentId = null;
                        if (!string.IsNullOrEmpty(deptCode))
                        {
                            Department matchedDept;
                            if (!deptLookup.TryGetValue(deptCode, out matchedDept))
                            {
                                rowResult.IsSuccess = false;
                                rowResult.Message = $"Không tìm thấy Mã Khoa '{deptCode}'.";
                                vm.RowResults.Add(rowResult);
                                vm.SkippedOrErrorCount++;
                                continue;
                            }
                            departmentId = matchedDept.DepartmentId;
                        }

                        DateTime? purchaseDate = null;
                        DateTime parsedDate;
                        if (row.Cell(5).DataType == XLDataType.DateTime)
                        {
                            purchaseDate = row.Cell(5).GetDateTime().Date;
                        }
                        else if (!string.IsNullOrEmpty(purchaseDateStr) &&
                                 DateTime.TryParse(purchaseDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        {
                            purchaseDate = parsedDate.Date;
                        }

                        decimal? purchasePrice = null;
                        decimal parsedPrice;
                        if (row.Cell(6).DataType == XLDataType.Number)
                        {
                            purchasePrice = (decimal)row.Cell(6).GetDouble();
                        }
                        else if (!string.IsNullOrEmpty(purchasePriceStr) &&
                                 decimal.TryParse(purchasePriceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedPrice))
                        {
                            purchasePrice = parsedPrice;
                        }

                        string finalStatus = statusStr.Length == 0 ? AssetStatus.Active : statusStr;
                        if ((purchaseDateStr.Length != 0 && !purchaseDate.HasValue) || purchaseDate > DateTime.Today
                            || (purchasePriceStr.Length != 0 && !purchasePrice.HasValue) || purchasePrice < 0
                            || purchasePrice > 1000000000000m || !AssetStatus.All.Contains(finalStatus))
                        {
                            rowResult.Message = "Ngày mua, giá mua hoặc trạng thái không hợp lệ.";
                            vm.RowResults.Add(rowResult);
                            vm.SkippedOrErrorCount++;
                            continue;
                        }

                        assetsToAdd.Add(new EquipmentAsset
                        {
                            AssetCode = assetCode,
                            SerialNumber = serialNumber,
                            ModelId = matchedModel.ModelId,
                            DepartmentId = departmentId,
                            PurchaseDate = purchaseDate,
                            PurchasePrice = purchasePrice,
                            Status = finalStatus,
                            IsDeleted = false
                        });

                        existingAssetCodes.Add(assetCode);
                        existingSerials.Add(serialNumber.ToUpperInvariant());

                        rowResult.Status = finalStatus;
                        rowResult.IsSuccess = true;
                        rowResult.Message = $"Đã nhập vào mẫu [{matchedModel.ModelName}].";
                        vm.RowResults.Add(rowResult);
                        vm.ImportedCount++;
                    }

                    if (assetsToAdd.Any())
                    {
                        db.EquipmentAssets.AddRange(assetsToAdd);
                        db.SaveChanges();
                        TempData["Success"] = $"Đã import thành công {vm.ImportedCount}/{vm.TotalRowsRead} thiết bị từ file Excel.";
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Trace.TraceWarning("Không lưu được Excel, loại lỗi {0}.", ex.GetBaseException().GetType().Name);
                foreach (var row in vm.RowResults.Where(r => r.IsSuccess))
                {
                    row.IsSuccess = false;
                    row.Message = "Giao dịch nhập thất bại, dòng này chưa được lưu.";
                }
                vm.SkippedOrErrorCount += vm.ImportedCount;
                vm.ImportedCount = 0;
                ModelState.AddModelError("", "Dữ liệu thay đổi khi nhập. Vui lòng kiểm tra mã tài sản, Serial và thử lại.");
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is FormatException || ex is IOException
                || ex is ArgumentException || ex is InvalidOperationException || ex is OverflowException
                || ex is DocumentFormat.OpenXml.Packaging.OpenXmlPackageException)
            {
                vm.HasProcessed = false;
                ModelState.AddModelError("", "Không đọc được file Excel. Vui lòng dùng mẫu .xlsx của hệ thống.");
            }

            return View(vm);
        }

        [HttpGet, CustomAuthorize(Roles = "ADMIN,MANAGER")]
        public ActionResult DownloadImportTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Mau_Import_ThietBi");
                string[] headers =
                {
                    "AssetCode (Mã tài sản *)",
                    "SerialNumber (Số Serial *)",
                    "ModelCode (Mã Model *)",
                    "DepartmentCode (Mã Khoa)",
                    "PurchaseDate (yyyy-MM-dd)",
                    "PurchasePrice (VNĐ)",
                    "Status (Trạng thái)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                }

                ws.Cell(2, 1).Value = "SA-101";
                ws.Cell(2, 2).Value = "SN-PH7-202601";
                ws.Cell(2, 3).Value = "PH-EPIQ7";
                ws.Cell(2, 4).Value = "KCDHA";
                ws.Cell(2, 5).Value = "2026-01-15";
                ws.Cell(2, 6).Value = 1480000000;
                ws.Cell(2, 7).Value = AssetStatus.Active;

                ws.Cell(3, 1).Value = "MON-102";
                ws.Cell(3, 2).Value = "SN-MX40-202602";
                ws.Cell(3, 3).Value = "PH-MX40";
                ws.Cell(3, 4).Value = "KCC";
                ws.Cell(3, 5).Value = "2026-02-20";
                ws.Cell(3, 6).Value = 125000000;
                ws.Cell(3, 7).Value = AssetStatus.Active;

                ws.Columns().AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return File(
                        ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Mau_Import_ThietBi.xlsx");
                }
            }
        }

        // ============================================================
        // CÁC HÀM HELPER NỘI BỘ
        // ============================================================
        /// <summary>
        /// Tìm thiết bị theo ID, bỏ qua các bản ghi đã đánh dấu xóa mềm (IsDeleted).
        /// </summary>
        private EquipmentAsset FindActiveAsset(int id)
        {
            return db.EquipmentAssets.FirstOrDefault(a => a.AssetId == id && !a.IsDeleted);
        }

        private void ValidateAssetFields(EquipmentAssetViewModel vm, EquipmentAsset current)
        {
            var message = EquipmentPolicy.ValidateFields(db, vm, current);
            if (message != null)
                ModelState.AddModelError("", message);
        }

        private bool SaveAsset()
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateException error) when (error.GetBaseException() is System.Data.SqlClient.SqlException sql
                && (sql.Number == 2601 || sql.Number == 2627 || sql.Number == 547))
            {
                ModelState.AddModelError("", "Mã tài sản, Serial hoặc dữ liệu liên quan đã thay đổi. Vui lòng kiểm tra lại.");
                return false;
            }
        }

        private void ValidateAssetUniqueness(EquipmentAssetViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.AssetCode))
            {
                var normalizedCode = vm.AssetCode.Trim().ToLower();
                if (db.EquipmentAssets.Any(a => a.AssetCode.ToLower() == normalizedCode && a.AssetId != vm.AssetId))
                {
                    ModelState.AddModelError("AssetCode", "Mã tài sản này đã tồn tại trong hệ thống.");
                }
            }

            if (!string.IsNullOrWhiteSpace(vm.SerialNumber))
            {
                var normalizedSerial = vm.SerialNumber.Trim().ToLower();
                if (db.EquipmentAssets.Any(a => a.SerialNumber.ToLower() == normalizedSerial && a.AssetId != vm.AssetId))
                {
                    ModelState.AddModelError("SerialNumber", "Số Serial này đã tồn tại trong hệ thống.");
                }
            }
        }

        /// <summary>
        /// Nạp dữ liệu DropdownList (Model và Department) vào EquipmentAssetViewModel.
        /// Hiển thị đúng Tên mẫu thiết bị và Tên Khoa/Phòng chứ không hiển thị ID.
        /// </summary>
        private void PopulateFormDropdownLists(EquipmentAssetViewModel vm)
        {
            vm.ModelList = db.EquipmentModels
                .Include(m => m.Category)
                .Where(m => m.IsActive || m.ModelId == vm.ModelId)
                .OrderBy(m => m.ModelName)
                .ToList()
                .Select(m => new SelectListItem
                {
                    Value = m.ModelId.ToString(),
                    Text = $"{m.ModelName} ({m.ModelCode})" + (m.Category != null ? $" - {m.Category.Name}" : ""),
                    Selected = vm.ModelId.HasValue && vm.ModelId.Value == m.ModelId
                })
                .ToList();

            vm.DepartmentList = db.Departments
                .Where(d => d.IsActive || d.DepartmentId == vm.DepartmentId)
                .OrderBy(d => d.Name)
                .ToList()
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name,
                    Selected = vm.DepartmentId.HasValue && vm.DepartmentId.Value == d.DepartmentId
                })
                .ToList();

            vm.StatusList = AssetStatus.All
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s,
                    Selected = string.Equals(vm.Status, s, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }

        private void PopulateIndexFilterLists(EquipmentAssetIndexViewModel vm)
        {
            vm.DepartmentList = db.Departments
                .OrderBy(d => d.Name)
                .ToList()
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name,
                    Selected = vm.DepartmentId.HasValue && vm.DepartmentId.Value == d.DepartmentId
                })
                .ToList();

            vm.StatusList = AssetStatus.All
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s,
                    Selected = string.Equals(vm.Status, s, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }

        private EquipmentImportViewModel BuildImportViewModel()
        {
            return new EquipmentImportViewModel
            {
                AvailableModels = db.EquipmentModels.Include(m => m.Category).OrderBy(m => m.ModelCode).ToList(),
                AvailableDepartments = db.Departments.OrderBy(d => d.DepartmentCode).ToList()
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
