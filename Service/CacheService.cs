using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Service.Helper.Cache;
using ServiceAbstraction;

namespace Service
{
    public class CacheService(ICacheRepository cacheRepository) : ICacheService
    {
        public async Task<string?> GetCacheValueAsync(string key)
        {
            var value = await cacheRepository.GetAsync(key);
            return value == null ? null : value;
        }

        public async Task SetCacheValueAsync(string key, object value, TimeSpan expiration )
        {
            await cacheRepository.SetAsync(key,value, expiration);  
        }
    }
}
