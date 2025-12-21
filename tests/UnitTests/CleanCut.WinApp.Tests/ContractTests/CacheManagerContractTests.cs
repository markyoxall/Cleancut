using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CleanCut.Infrastructure.Caching.Constants;
using CleanCut.Application.Common.Interfaces;
using CleanCut.WinApp.Services.Caching;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace CleanCut.WinApp.Tests.ContractTests
{
    public class CacheManagerContractTests
    {
        private class TestCacheService : ICacheService
        {
            private readonly ConcurrentDictionary<string, object> _store = new();

            public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
            {
                if (_store.TryGetValue(key, out var obj) && obj is T t)
                    return Task.FromResult<T?>(t);
                return Task.FromResult<T?>(null);
            }

            public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
            {
                _store[key] = value ?? throw new ArgumentNullException(nameof(value));
                return Task.CompletedTask;
            }

            public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            {
                _store.TryRemove(key, out _);
                return Task.CompletedTask;
            }

            public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrEmpty(pattern)) return Task.CompletedTask;

                // Convert glob to regex
                var escaped = Regex.Escape(pattern).Replace("\\*", ".*");
                var regex = new Regex("^" + escaped + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var keysToRemove = _store.Keys.Where(k => regex.IsMatch(k)).ToList();
                foreach (var k in keysToRemove) _store.TryRemove(k, out _);
                return Task.CompletedTask;
            }

            public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_store.ContainsKey(key));
            }
        }

        [Fact]
        public async Task CacheManager_Should_Remove_Customer_Keys_When_Invalidated()
        {
            // Arrange
            var testCache = new TestCacheService();
            var cacheManager = new CacheManager(testCache);

            var key = CacheKeys.AllCustomers();
            await testCache.SetAsync(key, new List<string> { "foo" });

            var existsBefore = await testCache.ExistsAsync(key);
            Assert.True(existsBefore);

            // Act
            await cacheManager.InvalidateCustomersAsync();

            // Assert
            var existsAfter = await testCache.ExistsAsync(key);
            Assert.False(existsAfter);
        }

        [Fact]
        public async Task LoggingCacheManager_Should_Preserve_Behavior()
        {
            // Arrange
            var testCache = new TestCacheService();
            var baseManager = new CacheManager(testCache);
            var logging = new LoggingCacheManager(baseManager, new NullLogger<LoggingCacheManager>());

            var key = CacheKeys.AllCustomers();
            await testCache.SetAsync(key, new List<string> { "bar" });
            Assert.True(await testCache.ExistsAsync(key));

            // Act
            await logging.InvalidateCustomersAsync();

            // Assert
            Assert.False(await testCache.ExistsAsync(key));
        }
    }
}
