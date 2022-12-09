namespace CommonTGBotLib.Services
{
    public interface ICachedService
    {
        string CreateKey<T>(params object[] param);
        Task<bool> DeleteAsync(string id);
        T? GetAndSetJson<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class;
        Task<T?> GetAndSetJsonAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class;
        Task<T?> GetAndSetJsonAsync<T>(string key, T data, TimeSpan? time = null) where T : class;
        Task<byte[]> GetByteAsync<T>(string key);
        string GetCurrentLang();
        T? GetJson<T>(string key) where T : class;
        Task<T?> GetJsonAsync<T>(string key) where T : class;
        Task SetByteAsync(string key, byte[] data, TimeSpan? time);
        bool SetJson<T>(string key, T data, TimeSpan? time) where T : class;
        Task SetJsonAsync<T>(string key, T data, TimeSpan? time) where T : class;
    }
}