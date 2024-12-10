using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.DistributedLock
{
    public class BlobStorageDistributedLock : IDistributedLock
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly TimeSpan _ttl;
        private readonly TimeSpan _timeout;
        private readonly string _prefix;
        private readonly TimeSpan _renewInterval = TimeSpan.FromSeconds(30);

        // Зберігатиме токени для припинення задач поновлення оренди
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _renewCancellations = new ConcurrentDictionary<string, CancellationTokenSource>();

        public BlobStorageDistributedLock(BlobContainerClient blobContainerClient, TimeSpan ttl, TimeSpan timeout, string prefix = "lock_")
        {
            _blobContainerClient = blobContainerClient;
            _ttl = ttl;
            _timeout = timeout;
            _prefix = prefix;
        }

        public async Task<string> AcquireLockAsync(string lockKey, TimeSpan? ttl = null, TimeSpan? timeout = null)
        {
            ttl ??= _ttl;
            timeout ??= _timeout;

            var stopWatch = Stopwatch.StartNew();
            var blobName = _prefix + lockKey;
            var blobClient = _blobContainerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                await blobClient.UploadAsync(new BinaryData(Array.Empty<byte>()), overwrite: true);

            var leaseClient = blobClient.GetBlobLeaseClient();
            string leaseId = null;

            do
            {
                try
                {
                    // Намагаємося отримати оренду
                    var response = await leaseClient.AcquireAsync(TimeSpan.FromMinutes(1));
                    leaseId = response.Value.LeaseId;

                    // Запускаємо фоновий процес для поновлення оренди
                    StartLeaseRenewal(lockKey, blobClient, leaseId, ttl.Value);
                    return leaseId;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
                {
                    // Хтось інший утримує оренду. Перевіряємо таймаут
                    if (timeout.HasValue && stopWatch.Elapsed >= timeout.Value)
                    {
                        return null;
                    }

                    await Task.Delay(100);
                }
            } while (true);
        }

        public async Task ReleaseLockAsync(string lockKey, string leaseId)
        {
            var blobName = _prefix + lockKey;
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient(leaseId);

            // Зупиняємо фонове поновлення оренди
            if (_renewCancellations.TryRemove(lockKey, out var cts))
            {
                cts.Cancel();
            }

            await leaseClient.ReleaseAsync();
        }

        private void StartLeaseRenewal(string lockKey, BlobClient blobClient, string leaseId, TimeSpan? renewTimeout = null)
        {
            var cts = new CancellationTokenSource();
            _renewCancellations[lockKey] = cts;
            var token = cts.Token;

            Task.Run(async () =>
            {
                var leaseClient = blobClient.GetBlobLeaseClient(leaseId);
                var startTime = DateTime.UtcNow;

                while (!token.IsCancellationRequested)
                {
                    // Перевірка, чи вийшли за межі максимально дозволеного часу поновлення.
                    if (renewTimeout.HasValue && DateTime.UtcNow - startTime > renewTimeout.Value)
                    {
                        // Перевищено ліміт часу поновлення – припиняємо цикл.
                        break;
                    }

                    try
                    {
                        await Task.Delay(_renewInterval, token);
                        await leaseClient.RenewAsync(cancellationToken: token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Завдання скасовано – виходимо з циклу
                        break;
                    }
                    catch (Exception)
                    {
                        // У разі помилки (наприклад, якщо оренду не вдалось поновити) –
                        // припиняємо поновлення, можемо додати логування
                        break;
                    }
                }
            }, token);
        }
    }
}
