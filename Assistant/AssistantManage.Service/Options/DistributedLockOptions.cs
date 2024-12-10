namespace Assistant.Service.Options;

public class DistributedLockOptions
{
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}