using StackExchange.Redis;

namespace IT.Redis.Lua.Tests;

public class TransactionTest
{
    private static readonly IDatabase _db;

    static TransactionTest()
    {
        var connection = ConnectionMultiplexer.Connect("localhost:6381,defaultDatabase=0,syncTimeout=5000,allowAdmin=False,connectTimeout=5000,ssl=False,abortConnect=False");
        _db = connection.GetDatabase();
    }

    [Test]
    public async Task HashTransaction()
    {
        RedisKey key = "Test:HashTransaction";
        RedisValue name1 = "name1";
        RedisValue value1 = "value1";
        var db = _db;

        try
        {
            var t = db.CreateTransaction();

            t.AddCondition(Condition.KeyNotExists(key));

            var hashRes = t.HashSetAsync(key, name1, value1);

            var keyExpire = t.KeyExpireAsync(key, DateTime.UtcNow.AddMinutes(1));

            var exec = await t.ExecuteAsync();

            Assert.That(exec, Is.True);
        }
        finally
        {
            await db.KeyDeleteAsync(key);
        }
    }
}