using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Threading
{
    public class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _redisDatabase;
        private readonly TimeSpan _ttl;
        private readonly TimeSpan _timeout;
        private readonly string _prefix;

        public RedisDistributedLock(IDatabase redisDatabase, TimeSpan ttl, TimeSpan timeout, string prefix = "lock_")
        {
            _redisDatabase = redisDatabase;
            _ttl = ttl;
            _timeout = timeout;
            _prefix = prefix;
        }

        public async Task<string> AcquireLockAsync(string lockKey, TimeSpan? ttl = null, TimeSpan? timeout = null)
        {
            ttl ??= _ttl;
            timeout ??= _timeout;

            var lockValue = Guid.NewGuid().ToString();
            var stopWatch = Stopwatch.StartNew();

            do
            {
                bool acquired = await _redisDatabase.StringSetAsync(
                    key: _prefix + lockKey,
                    value: lockValue,
                    when: When.NotExists,
                    expiry: ttl);

                if (acquired)
                {
                    return lockValue;
                }

                if (timeout.HasValue && stopWatch.Elapsed >= timeout.Value)
                {
                    return null;
                }

                await Task.Delay(100);
            }
            while (true);
        }

        public async Task ReleaseLockAsync(string lockKey, string leaseId)
        {
            const string script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

            await _redisDatabase.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _prefix + lockKey },
                new RedisValue[] { leaseId });
        }
    }
}
