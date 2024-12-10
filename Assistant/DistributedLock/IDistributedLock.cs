using System.Threading.Tasks;
using System;

public interface IDistributedLock
{
    Task<string?> AcquireLockAsync(string lockKey, TimeSpan? ttl = null, TimeSpan? timeout = null);
    Task ReleaseLockAsync(string lockKey, string leaseId);
}