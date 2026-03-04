using System.Net;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pay2Bill.Models;
using Pay2Bill.Services;

namespace Pay2Bill.Controllers
{
    /// <summary>
    /// Serves the /health endpoint used by:
    ///   1. Azure App Service Health Check (Monitoring > Health check > Path = /health)
    ///   2. Azure Load Balancer / Traffic Manager probes
    ///   3. Custom monitoring dashboards
    ///
    /// Response contract:
    ///   HTTP 200 = overall healthy
    ///   HTTP 503 = one or more dependencies unhealthy
    ///
    /// Azure App Service behaviour:
    ///   - If /health returns non-2xx for > 2 minutes, the instance is restarted.
    ///   - Unhealthy instances are removed from the load balancer.
    ///   - Configure minimum healthy instances to prevent full outage.
    /// </summary>
    public class HealthController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController()
        {
            var telemetry = new TelemetryClient();
            _healthCheckService = new HealthCheckService(
                new MockRedisHealthCheck(),
                new MockServiceBusHealthCheck(),
                telemetry);
        }

        // GET: /health
        public ActionResult Index()
        {
            var result = _healthCheckService.GetHealthStatus();

            var statusCode = result.OverallStatus == HealthStatus.Unhealthy
                ? HttpStatusCode.ServiceUnavailable   // 503 - unhealthy instances removed from LB
                : HttpStatusCode.OK;                   // 200 - healthy

            Response.StatusCode = (int)statusCode;

            var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });

            return Content(json, "application/json");
        }

        // GET: /health/ui
        public ActionResult Dashboard()
        {
            var result = _healthCheckService.GetHealthStatus();
            return View(result);
        }
    }
}
