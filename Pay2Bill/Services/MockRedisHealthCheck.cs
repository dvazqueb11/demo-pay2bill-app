using System.Configuration;
using System.Diagnostics;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Mock Redis health check.
    ///
    /// Toggle health via appSettings:
    ///   <add key="HealthCheck:Redis:IsHealthy" value="true" />
    ///
    /// In Azure App Service, override this via:
    ///   Configuration > Application settings > HealthCheck:Redis:IsHealthy = false
    ///
    /// In production, this would use StackExchange.Redis to ping the cache endpoint.
    /// </summary>
    public class MockRedisHealthCheck : IRedisHealthCheck
    {
        private const string ConfigKey = "HealthCheck:Redis:IsHealthy";

        public DependencyHealthResult Check()
        {
            var sw = Stopwatch.StartNew();

            // Read health toggle from configuration
            var isHealthy = ConfigurationManager.AppSettings[ConfigKey]?.ToLowerInvariant() != "false";
            var endpoint = ConfigurationManager.AppSettings["Redis:Endpoint"] ?? "not configured";

            // Simulate a brief check latency
            System.Threading.Thread.Sleep(isHealthy ? 5 : 50);
            sw.Stop();

            return new DependencyHealthResult
            {
                Name = "Redis Cache",
                Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                Message = isHealthy
                    ? $"Connected to {endpoint}"
                    : $"Unable to connect to {endpoint} (simulated failure)",
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}
