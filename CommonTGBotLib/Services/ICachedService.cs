namespace CommonTGBotLib.Services
{
    public interface ICachedService
    {
        string CreateKey<T>(params object[] param);
        Task<bool> DeleteAsync(string id);
        T? Get<T>(string key) where T : class;
        T? GetAndSet<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class;
        Task<T?> GetAndSetAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class;
        Task<T?> GetAndSetAsync<T>(string key, T data, TimeSpan? time = null) where T : class;
        Task<T?> GetAsync<T>(string key) where T : class;
        string GetCurrentLang();
        bool Set<T>(string key, T data, TimeSpan? time) where T : class;
        Task<bool> SetAsync<T>(string key, T data, TimeSpan? time) where T : class;
    }
}