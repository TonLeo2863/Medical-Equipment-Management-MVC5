using System.Linq;
using System.Web;
using MedicalEquipmentManagement.Data;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>Chia sẻ quy tắc tái xác nhận tài khoản giữa bộ lọc MVC, Dashboard và Web API.</summary>
    internal static class SessionAccount
    {
        /// <summary>Trả vai trò nếu tài khoản vẫn hoạt động và thông tin xác thực còn khớp; trả null khi phiên hết hiệu lực.</summary>
        internal static string GetRole(HttpSessionStateBase session, MedicalEquipmentContext db)
        {
            if (!(session?["UserId"] is int userId))
                return null;

            var account = db.Users.Where(u => u.UserId == userId && u.IsActive)
                .Select(u => new { u.PasswordHash, u.Role.RoleCode }).SingleOrDefault();
            if (account == null || account.RoleCode != session["RoleCode"] as string
                || PasswordHasher.SessionCredential(account.PasswordHash) != session["Credential"] as string)
                return null;

            return account.RoleCode;
        }
    }
}
