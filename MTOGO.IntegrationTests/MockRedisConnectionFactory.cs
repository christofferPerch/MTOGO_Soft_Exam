using Moq;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MTOGO.IntegrationTests
{
    public static class MockRedisConnectionFactory
    {
        public static IConnectionMultiplexer CreateMockConnection()
        {
            var mockConnection = new Mock<IConnectionMultiplexer>();
            var mockDatabase = new Mock<IDatabase>();

            // In-memory storage to simulate Redis key-value pairs
            var storage = new ConcurrentDictionary<RedisKey, RedisValue>();

            // Mock behavior for StringSetAsync
            mockDatabase
                .Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisKey key, RedisValue value, TimeSpan? expiry, When when, CommandFlags flags) =>
                {
                    storage[key] = value;
                    return true;
                });

            // Mock behavior for StringGetAsync
            mockDatabase
                .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisKey key, CommandFlags flags) =>
                {
                    return storage.TryGetValue(key, out var value) ? value : RedisValue.Null;
                });

            // Mock behavior for KeyDeleteAsync
            mockDatabase
                .Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisKey key, CommandFlags flags) =>
                {
                    return storage.TryRemove(key, out _);
                });

            // Mock behavior for KeyExistsAsync
            mockDatabase
                .Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisKey key, CommandFlags flags) =>
                {
                    return storage.ContainsKey(key);
                });

            mockConnection
                .Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(mockDatabase.Object);

            return mockConnection.Object;
        }
    }
}
