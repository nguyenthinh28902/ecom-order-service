namespace Ecom.OrderService.Application.Interface
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expirationMinutes) where T : class;
        Task<T?> GetAsync<T>(string key) where T : class;
        Task RemoveAsync(string key);
    }
}
