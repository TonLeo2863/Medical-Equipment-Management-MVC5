using System.Collections.Generic;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>
    /// Hằng số trạng thái chuẩn cho Thiết bị vật lý (EquipmentAsset) và Loại nhà cung cấp (Supplier).
    /// Tái sử dụng thống nhất trên toàn bộ Module 2, Module 3 (Bảo trì/Luân chuyển) và Module 4 (Thống kê).
    /// </summary>
    public static class AssetStatus
    {
        public const string Active = "Đang hoạt động";
        public const string Maintenance = "Đang bảo trì";
        public const string Broken = "Hỏng";
        public const string Liquidated = "Đã thanh lý";

        /// <summary>
        /// Danh sách tất cả các trạng thái thiết bị hợp lệ để bind ra DropdownList.
        /// </summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            Active,
            Maintenance,
            Broken,
            Liquidated
        };
    }

    /// <summary>
    /// Hằng số phân loại Nhà cung cấp & Hãng sản xuất trong bảng Suppliers.
    /// </summary>
    public static class SupplierTypeConst
    {
        public const string Manufacturer = "Manufacturer";
        public const string Vendor = "Vendor";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Manufacturer,
            Vendor
        };

        /// <summary>
        /// Chuyển mã loại nhà cung cấp sang nhãn tiếng Việt hiển thị trên giao diện.
        /// </summary>
        public static string GetDisplayName(string type)
        {
            if (type == Manufacturer) return "Hãng sản xuất (Manufacturer)";
            if (type == Vendor) return "Nhà phân phối / Cung ứng (Vendor)";
            return string.IsNullOrWhiteSpace(type) ? "Nhà phân phối (Vendor)" : type;
        }
    }
}
