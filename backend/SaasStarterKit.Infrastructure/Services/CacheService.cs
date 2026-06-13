using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Common.Settings;
using System.Text.Json;

namespace SaasStarterKit.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly long _cacheDuration;

        public CacheService(IDistributedCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _cacheDuration = cacheSettings.Value.DurationMinutes;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cached = await _cache.GetStringAsync(key, cancellationToken);
            if (cached == null) return default;
            return JsonSerializer.Deserialize<T>(cached);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(1)
            };
            var serialized = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serialized, options, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
