using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.ApplicationInsights.Extensibility;
using Pay2Bill.App_Start;

namespace Pay2Bill
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Configure Application Insights
            // In production, InstrumentationKey is set via Azure App Service Application Settings
            // or sourced from Azure Key Vault
            TelemetryConfiguration.Active.InstrumentationKey =
                System.Configuration.ConfigurationManager.AppSettings["AppInsights:InstrumentationKey"];
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                // Track unhandled exceptions in Application Insights
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(exception);
            }
        }
    }
}
