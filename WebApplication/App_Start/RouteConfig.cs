using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account" }
            );

            routes.MapRoute(
                name: "File",
                url: "{action}",
                defaults: new { controller = "File", id = UrlParameter.Optional }
            );
        }
    }
}
