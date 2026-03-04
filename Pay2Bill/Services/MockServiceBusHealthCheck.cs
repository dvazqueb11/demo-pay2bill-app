using System.Configuration;
using System.Diagnostics;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Mock Service Bus health check.
    ///
    /// Toggle health via appSettings:
    ///   <add key="HealthCheck:ServiceBus:IsHealthy" value="true" />
    ///
    /// In Azure App Service, override this via:
    ///   Configuration > Application settings > HealthCheck:ServiceBus:IsHealthy = false
    ///
    /// In production, this would use Azure.Messaging.ServiceBus to peek at a management queue.
    /// </summary>
    public class MockServiceBusHealthCheck : IServiceBusHealthCheck
    {
        private const string ConfigKey = "HealthCheck:ServiceBus:IsHealthy";

        public DependencyHealthResult Check()
        {
            var sw = Stopwatch.StartNew();

            var isHealthy = ConfigurationManager.AppSettings[ConfigKey]?.ToLowerInvariant() != "false";
            var ns = ConfigurationManager.AppSettings["ServiceBus:Namespace"] ?? "not configured";

            // Simulate a brief check latency
            System.Threading.Thread.Sleep(isHealthy ? 8 : 100);
            sw.Stop();

            return new DependencyHealthResult
            {
                Name = "Service Bus",
                Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                Message = isHealthy
                    ? $"Connected to {ns}"
                    : $"Unable to reach {ns} (simulated failure)",
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}
