using System.Linq;
using Microsoft.ApplicationInsights;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Aggregates all dependency health checks into a single result.
    /// Called by HealthController to serve the /health endpoint.
    ///
    /// Azure App Service Health Check:
    ///   - Configure path: /health in App Service > Monitoring > Health check
    ///   - Returns HTTP 200 = healthy, HTTP 503 = unhealthy
    ///   - App Service will remove unhealthy instances from load balancer rotation
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IRedisHealthCheck _redisCheck;
        private readonly IServiceBusHealthCheck _serviceBusCheck;
        private readonly TelemetryClient _telemetry;

        public HealthCheckService(
            IRedisHealthCheck redisCheck,
            IServiceBusHealthCheck serviceBusCheck,
            TelemetryClient telemetryClient)
        {
            _redisCheck = redisCheck;
            _serviceBusCheck = serviceBusCheck;
            _telemetry = telemetryClient;
        }

        public HealthCheckResult GetHealthStatus()
        {
            var result = new HealthCheckResult();

            // Check application itself (always healthy if we get here)
            result.Dependencies.Add(new DependencyHealthResult
            {
                Name = "Application",
                Status = HealthStatus.Healthy,
                Message = "Application is running.",
                LatencyMs = 0
            });

            // Check Redis
            var redisResult = _redisCheck.Check();
            result.Dependencies.Add(redisResult);

            // Check Service Bus
            var serviceBusResult = _serviceBusCheck.Check();
            result.Dependencies.Add(serviceBusResult);

            // Determine overall status
            if (result.Dependencies.Any(d => d.Status == HealthStatus.Unhealthy))
                result.OverallStatus = HealthStatus.Unhealthy;
            else if (result.Dependencies.Any(d => d.Status == HealthStatus.Degraded))
                result.OverallStatus = HealthStatus.Degraded;
            else
                result.OverallStatus = HealthStatus.Healthy;

            // Track health check results in Application Insights
            _telemetry.TrackEvent("HealthCheck", new System.Collections.Generic.Dictionary<string, string>
            {
                ["OverallStatus"] = result.OverallStatus.ToString(),
                ["Redis"] = redisResult.Status.ToString(),
                ["ServiceBus"] = serviceBusResult.Status.ToString()
            });

            return result;
        }
    }
}
