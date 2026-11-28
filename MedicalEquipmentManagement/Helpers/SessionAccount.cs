using System.Linq;
using System;
using System.Security.Cryptography;
using System.Web;
using MedicalEquipmentManagement.Data;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>Chia sẻ quy tắc tái xác nhận tài khoản giữa bộ lọc MVC, Dashboard và Web API.</summary>
    public static class SessionAccount
    {
        private const string LoginCookie = "MedicalEquipmentLogin";

        /// <summary>Gắn bí mật mới của lần đăng nhập vào Session và Cookie HttpOnly để Session ID trước khi đăng nhập không đủ để tái sử dụng phiên.</summary>
        internal static void BindLogin(HttpContextBase context)
        {
            var bytes = new byte[32];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(bytes);
            var key = HttpServerUtility.UrlTokenEncode(bytes);
            context.Session["LoginBinding"] = key;
            context.Response.Cookies.Set(NewLoginCookie(key, context.Request.IsSecureConnection));
        }

        internal static void ExpireLogin(HttpContextBase context)
        {
            var cookie = NewLoginCookie("", context.Request.IsSecureConnection);
            cookie.Expires = DateTime.UtcNow.AddDays(-1);
            context.Response.Cookies.Set(cookie);
        }

        private static HttpCookie NewLoginCookie(string value, bool secure)
        {
            return new HttpCookie(LoginCookie, value) { HttpOnly = true, Secure = secure, SameSite = SameSiteMode.Lax };
        }

        /// <summary>Xác minh trình duyệt có cả Cookie phiên và Cookie gắn với lần đăng nhập trước khi hiển thị danh tính.</summary>
        public static bool HasBinding(HttpContextBase context)
        {
            var binding = context.Session?["LoginBinding"] as string;
            return binding != null && context.Request.Cookies[LoginCookie]?.Value == binding;
        }

        /// <summary>Trả vai trò nếu tài khoản vẫn hoạt động và thông tin xác thực còn khớp; trả null khi phiên hết hiệu lực.</summary>
        internal static string GetRole(HttpSessionStateBase session, MedicalEquipmentContext db)
        {
            if (!(session?["UserId"] is int userId))
                return null;
            if (HttpContext.Current == null || !HasBinding(new HttpContextWrapper(HttpContext.Current)))
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
