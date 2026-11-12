using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MedicalEquipmentManagement.Filters
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var userName = session["UserName"] as string;
            var roleCode = session["RoleCode"] as string;
            if (string.IsNullOrEmpty(userName))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                bool isAuthorized = false;
                var allowedRoles = Roles.Split(',');

                foreach (var role in allowedRoles)
                {
                    if (role.Trim().ToUpper() == roleCode?.Trim().ToUpper())
                    {
                        isAuthorized = true;
                        break;
                    }
                }

                if (!isAuthorized)
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" })
                    );
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}