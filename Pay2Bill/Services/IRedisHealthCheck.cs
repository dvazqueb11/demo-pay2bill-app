using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Interface for Redis health check.
    /// In a real application this would ping the Redis endpoint.
    /// </summary>
    public interface IRedisHealthCheck
    {
        DependencyHealthResult Check();
    }
}
