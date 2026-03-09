using Ecom.OrderService.Application.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ecom.OrderService.Application.Service
{
    public class CacheService : ICacheService
    {
        private readonly ILogger<CacheService> _logger;
        private readonly IDistributedCache _cache;
        // Đây là "vùng tên" riêng cho Identity để không lẫn với UserSession của Gateway

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // Lưu dữ liệu vào Cache
        public async Task SetAsync<T>(string key, T value, TimeSpan expirationMinutes) where T : class
        {
            try
            {
                var cacheKey = $"{key}";

                var options = new DistributedCacheEntryOptions
                {
                    // Hết hạn tuyệt đối (thường trừ đi 60s để an toàn cho Token)
                    AbsoluteExpirationRelativeToNow = expirationMinutes
                };

                var jsonData = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(cacheKey, jsonData, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu dữ liệu vào Cache với key: {CacheKey}", key);
               
            }
        }

        // Lấy dữ liệu từ Cache
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cacheKey = $"{key}";
                var jsonData = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(jsonData))
                    return null;

                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu từ Cache với key: {CacheKey}", key);
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync($"{key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa dữ liệu từ Cache với key: {CacheKey}", key);
            }
        }
    }
}
