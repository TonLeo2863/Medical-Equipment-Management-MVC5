using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MedicalEquipmentManagement.Data;

namespace MedicalEquipmentManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_PostAuthorizeRequest()
        {
            if (Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api/", StringComparison.OrdinalIgnoreCase))
                Context.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.ReadOnly);
        }

        protected void Application_Start()
        {
            // Tắt cơ chế tự động khởi tạo/migration của EF6 vì CSDL đã được tạo từ script Database.sql
            Database.SetInitializer<MedicalEquipmentContext>(null);

            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
