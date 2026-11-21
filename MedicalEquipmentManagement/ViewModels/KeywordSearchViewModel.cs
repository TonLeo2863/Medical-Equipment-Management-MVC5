using System.Collections.Generic;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// ViewModel dùng chung cho các trang danh sách có ô tìm kiếm theo từ khóa
    /// (Category, Supplier), thay cho ViewBag.Keyword.
    /// </summary>
    public class KeywordSearchViewModel<T>
    {
        public string Keyword { get; set; }

        public IList<T> Items { get; set; } = new List<T>();
    }
}