using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;

namespace MedicalEquipmentManagement.Filters
{
    /// <summary>Áp dụng quyền phía server cho MVC dựa trên Session đã tái xác nhận.</summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>Đối chiếu trạng thái tài khoản và vai trò hiện tại trước khi cho phép action được thực thi.</summary>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            string roleCode;
            using (var db = new MedicalEquipmentContext())
                roleCode = SessionAccount.GetRole(session, db);
            if (string.IsNullOrEmpty(roleCode))
            {
                if (SessionAccount.HasBinding(filterContext.HttpContext))
                    session.Clear();
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                if (!Roles.Split(',').Any(role => string.Equals(role.Trim(), roleCode, StringComparison.OrdinalIgnoreCase)))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" })
                    );
                }
            }


        }
    }
}
