using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>
    /// Helper xử lý và nhận diện các lỗi ràng buộc từ SQL Server / Entity Framework.
    /// Giúp các Controller của Module 2 bắt lỗi khóa ngoại (Cascade Delete bị chặn) một cách thống nhất và sạch sẽ.
    /// </summary>
    public static class DbErrorHelper
    {
        /// <summary>
        /// Mã lỗi SQL Server 547: The DELETE/UPDATE statement conflicted with the REFERENCE constraint.
        /// </summary>
        public const int SqlForeignKeyViolationNumber = 547;

        /// <summary>
        /// Mã lỗi SQL Server 2601 / 2627: Vi phạm ràng buộc UNIQUE KEY hoặc PRIMARY KEY.
        /// </summary>
        public const int SqlUniqueIndexViolationNumber = 2601;
        public const int SqlUniqueConstraintViolationNumber = 2627;

        /// <summary>
        /// Kiểm tra xem ngoại lệ có phải do vi phạm khóa ngoại (Foreign Key Constraint - Error 547) hay không.
        /// </summary>
        public static bool IsForeignKeyViolation(Exception ex)
        {
            var sqlEx = FindSqlException(ex);
            return sqlEx != null && sqlEx.Number == SqlForeignKeyViolationNumber;
        }

        /// <summary>
        /// Kiểm tra xem ngoại lệ có phải do trùng dữ liệu UNIQUE (Error 2601 / 2627) hay không.
        /// </summary>
        public static bool IsUniqueViolation(Exception ex)
        {
            var sqlEx = FindSqlException(ex);
            return sqlEx != null &&
                   (sqlEx.Number == SqlUniqueIndexViolationNumber || sqlEx.Number == SqlUniqueConstraintViolationNumber);
        }

        /// <summary>
        /// Duyệt chuỗi InnerException của DbUpdateException để tìm đối tượng SqlException gốc từ SQL Server.
        /// </summary>
        public static SqlException FindSqlException(Exception ex)
        {
            var current = ex;
            while (current != null)
            {
                var sqlEx = current as SqlException;
                if (sqlEx != null)
                {
                    return sqlEx;
                }
                current = current.InnerException;
            }
            return null;
        }
    }
}
