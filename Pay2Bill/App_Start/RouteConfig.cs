using System.Web.Mvc;
using System.Web.Routing;

namespace Pay2Bill.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Health check endpoint - mapped to /health
            // This is monitored by Azure App Service Health Check feature
            // Configuration: App Service > Monitoring > Health check > Path = /health
            routes.MapRoute(
                name: "Health",
                url: "health",
                defaults: new { controller = "Health", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
