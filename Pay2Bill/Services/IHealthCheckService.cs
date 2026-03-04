using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    public interface IHealthCheckService
    {
        HealthCheckResult GetHealthStatus();
    }
}
