using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FileHandler
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            UnityConfig.RegisterComponents();
        }
        protected void Application_Error(object sender, EventArgs e) ////Якщо файл пройшов перевірку JS але не проходить maxRequestLength
        {
           
            Exception exception = Server.GetLastError();

            if (exception is HttpException httpException &&
                (httpException.Message.Contains("Maximum request length exceeded") ||
                 httpException.Message.Contains("Превышена максимальная длина запроса")))
            {
  
                Server.ClearError();


                Context.Session["ViewBag.ErrorMessage"] =
                    "Помилка: Файл занадто великий! Максимальний розмір: 128 MB.";

  
                Response.Redirect("~/Document/Index");
            }
         
        }
    }
}
