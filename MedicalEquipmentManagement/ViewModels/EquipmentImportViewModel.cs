using System.Collections.Generic;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// Kết quả đọc và xử lý từng dòng dữ liệu khi Import danh sách thiết bị từ file Excel (.xlsx).
    /// </summary>
    public class EquipmentImportRowResult
    {
        public int RowNumber { get; set; }
        public string AssetCode { get; set; }
        public string SerialNumber { get; set; }
        public string ModelCode { get; set; }
        public string DepartmentCode { get; set; }
        public string Status { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// ViewModel cho chức năng mở rộng: Import danh sách thiết bị từ file Excel (.xlsx).
    /// </summary>
    public class EquipmentImportViewModel
    {
        public bool HasProcessed { get; set; }
        public string FileName { get; set; }
        public int TotalRowsRead { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedOrErrorCount { get; set; }

        public List<EquipmentImportRowResult> RowResults { get; set; } = new List<EquipmentImportRowResult>();

        // Hiển thị danh sách Mã Model & Mã Khoa hợp lệ trên giao diện để người dùng dễ đối chiếu khi điền file Excel
        public List<EquipmentModel> AvailableModels { get; set; } = new List<EquipmentModel>();
        public List<Department> AvailableDepartments { get; set; } = new List<Department>();
    }
}
