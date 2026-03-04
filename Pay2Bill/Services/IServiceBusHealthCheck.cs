using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Interface for Service Bus health check.
    /// In a real application this would attempt a management API call or peek at a queue.
    /// </summary>
    public interface IServiceBusHealthCheck
    {
        DependencyHealthResult Check();
    }
}
