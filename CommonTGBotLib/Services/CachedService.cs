using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTGBotLib.Services
{
    public class CachedService : ICachedService
    {

        private readonly IDistributedCache _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CachedService(
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache db

            )
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public async Task<byte[]> GetByteAsync<T>(string key)
        {
            return await _db.GetAsync(key);
        }

        public async Task SetByteAsync(string key, byte[] data, TimeSpan? time)
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            await _db.SetAsync(key, data, options);
        }


        public async Task<T?> GetJsonAsync<T>(string key) where T : class
        {
            string data = await _db.GetStringAsync(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return default(T);
        }

        public T? GetJson<T>(string key) where T : class
        {
            string data = _db.GetString(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return default(T);
        }




        public async Task SetJsonAsync<T>(string key, T data, TimeSpan? time) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            await _db.SetStringAsync(
                  key, JsonConvert.SerializeObject(data),
                  options);

        }

        public bool SetJson<T>(string key, T data, TimeSpan? time) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            _db.SetString(
                  key, JsonConvert.SerializeObject(data),
                  options);
            return true;
        }

        public async Task<T?> GetAndSetJsonAsync<T>(string key, T data, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetJsonAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            await SetJsonAsync(key, data, time);
            return await GetJsonAsync<T>(key);
        }

        public async Task<T?> GetAndSetJsonAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetJsonAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = await acquire();

            if (data != null)
            {
                await SetJsonAsync(key, data, time);
            }

            return data;
        }

        public T? GetAndSetJson<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = GetJson<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = acquire();

            if (data != null)
            {
                SetJson(key, data, time);
            }
            return data;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _db.RemoveAsync(id);
            return true;
        }


        public string CreateKey<T>(params object[] param)
        {
            string typeName = typeof(T).Name;
            var currentLang = _httpContextAccessor.HttpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (string.IsNullOrEmpty(currentLang))
            {
                currentLang = "en-US";
            }
            StringBuilder builder = new StringBuilder($"defaultKey_{currentLang}_{typeName}_");
            foreach (var p in param)
            {
                builder.Append(p);
                builder.Append("_");
            }
            return builder.ToString();
        }


        public string GetCurrentLang()
        {
            var currentLang = _httpContextAccessor.HttpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (string.IsNullOrEmpty(currentLang))
            {
                currentLang = "en-US";
            }
            return currentLang;
        }
    }
}
