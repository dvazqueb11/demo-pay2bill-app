using System.Collections.Generic;

namespace Pay2Bill.Models
{
    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public class DependencyHealthResult
    {
        public string Name { get; set; }
        public HealthStatus Status { get; set; }
        public string Message { get; set; }
        public long LatencyMs { get; set; }
        public string StatusLabel => Status.ToString().ToLower();
    }

    public class HealthCheckResult
    {
        public HealthStatus OverallStatus { get; set; }
        public string ApplicationVersion { get; set; }
        public System.DateTime Timestamp { get; set; }
        public string Environment { get; set; }
        public List<DependencyHealthResult> Dependencies { get; set; }
        public string OverallStatusLabel => OverallStatus.ToString().ToLower();

        public HealthCheckResult()
        {
            Dependencies = new List<DependencyHealthResult>();
            Timestamp = System.DateTime.UtcNow;
            ApplicationVersion = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version
                .ToString();
            Environment = System.Configuration.ConfigurationManager.AppSettings["Environment"] ?? "Development";
        }
    }
}
