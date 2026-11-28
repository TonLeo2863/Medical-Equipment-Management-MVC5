using System;
using System.Linq;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>Áp dụng quy tắc tài sản chung cho biểu mẫu MVC và JSON của Web API.</summary>
    internal static class EquipmentPolicy
    {
        internal static bool HasOpenOrder(MedicalEquipmentContext db, int assetId)
        {
            return db.WorkOrders.Any(w => w.AssetId == assetId && w.Status != WorkOrderStatus.Completed
                && w.Status != WorkOrderStatus.Cancelled);
        }

        internal static string RemovalConflict(MedicalEquipmentContext db, int assetId)
        {
            if (HasOpenOrder(db, assetId))
                return "Không thể xóa thiết bị còn phiếu sửa chữa chưa hoàn tất.";
            if (db.MaintenanceRecords.Any(m => m.AssetId == assetId))
                return "Thiết bị có lịch sử bảo trì. Hãy thanh lý để giữ hồ sơ tra cứu.";
            return null;
        }

        internal static bool HasFailedCalibration(MedicalEquipmentContext db, int assetId)
        {
            return db.CalibrationLogs.Where(c => c.AssetId == assetId)
                .OrderByDescending(c => c.CalibrationDate).ThenByDescending(c => c.CalibrationId)
                .Select(c => c.Result).FirstOrDefault() == "Không đạt";
        }

        internal static bool HasFailedRepair(MedicalEquipmentContext db, int assetId)
        {
            return db.WorkOrders.Where(w => w.AssetId == assetId && w.Status == WorkOrderStatus.Completed)
                .OrderByDescending(w => w.ResolvedDate).ThenByDescending(w => w.WorkOrderId)
                .Select(w => w.TechnicalPassed).FirstOrDefault() == false;
        }

        internal static bool HasFailedMaintenance(MedicalEquipmentContext db, int assetId)
        {
            var result = db.LatestMaintenance().Where(m => m.AssetId == assetId).Select(m => m.ResultStatus).FirstOrDefault();
            return result == "Không thể khắc phục" || result == "Không đạt";
        }

        internal static string ValidateFields(MedicalEquipmentContext db, EquipmentAssetViewModel input, EquipmentAsset current)
        {
            if (string.IsNullOrWhiteSpace(input.AssetCode) || string.IsNullOrWhiteSpace(input.SerialNumber)
                || !AssetStatus.All.Contains(input.Status))
                return "Vui lòng nhập mã tài sản, số Serial và trạng thái hợp lệ.";
            if (input.SerialNumber.Trim() != current?.SerialNumber && !BarcodeQrHelper.CanEncodeCode128(input.SerialNumber.Trim()))
                return "Serial phải gồm các ký tự ASCII có thể in để tạo Barcode đúng nội dung.";
            if (input.PurchaseDate > DateTime.Today)
                return "Ngày mua không được sau ngày hiện tại.";
            if (input.ModelId != current?.ModelId && !db.EquipmentModels.Any(m => m.ModelId == input.ModelId && m.IsActive))
                return "Mẫu thiết bị không tồn tại hoặc đã ngừng hoạt động.";
            if (input.DepartmentId != current?.DepartmentId && input.DepartmentId.HasValue
                && !db.Departments.Any(d => d.DepartmentId == input.DepartmentId && d.IsActive))
                return "Khoa/phòng không tồn tại hoặc đã ngừng hoạt động.";
            return null;
        }

        /// <summary>Bảo vệ lịch sử điều chuyển và ngăn đưa thiết bị vào sử dụng khi còn phiếu mở hoặc kết quả kỹ thuật không đạt.</summary>
        internal static string ValidateChange(MedicalEquipmentContext db, EquipmentAssetViewModel input, EquipmentAsset current)
        {
            if (current.DepartmentId.HasValue && current.DepartmentId != input.DepartmentId)
                return "Hãy lập phiếu điều chuyển để đổi khoa/phòng của thiết bị đã phân bổ.";
            if (current.Status == AssetStatus.Liquidated && input.Status != AssetStatus.Liquidated)
                return "Thiết bị đã thanh lý không thể đưa lại vào sử dụng.";
            if (current.Status != input.Status && (input.Status == AssetStatus.Active || input.Status == AssetStatus.Liquidated)
                && HasOpenOrder(db, current.AssetId))
                return "Thiết bị vẫn còn phiếu sửa chữa chưa hoàn tất.";
            if (current.Status != input.Status && input.Status == AssetStatus.Active
                && (HasFailedCalibration(db, current.AssetId) || HasFailedRepair(db, current.AssetId)
                    || HasFailedMaintenance(db, current.AssetId)))
                return "Kết quả kỹ thuật gần nhất không đạt.";
            return null;
        }

        internal static bool HasDuplicate(MedicalEquipmentContext db, EquipmentAsset asset)
        {
            var code = asset.AssetCode.ToUpperInvariant();
            var serial = asset.SerialNumber.ToUpperInvariant();
            return db.EquipmentAssets.Any(a => a.AssetId != asset.AssetId
                && (a.AssetCode.ToUpper() == code || a.SerialNumber.ToUpper() == serial));
        }
    }
}
