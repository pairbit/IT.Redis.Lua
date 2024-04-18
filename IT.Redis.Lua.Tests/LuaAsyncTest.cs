using StackExchange.Redis;

namespace IT.Redis.Lua.Tests;

public class LuaAsyncTest
{
    private static readonly IDatabaseAsync _db;

    static LuaAsyncTest()
    {
        var connection = ConnectionMultiplexer.Connect("localhost:6381,defaultDatabase=0,syncTimeout=5000,allowAdmin=False,connectTimeout=5000,ssl=False,abortConnect=False");
        _db = connection.GetDatabase();
    }

    [Test]
    public async Task SetAdd()
    {
        RedisKey key = "Test:SetAdd";
        var db = _db;

        Assert.That(await db.KeyExistsAsync(key), Is.False);

        Assert.That(await db.SetAddIfKeyExistsAsync(key, 0), Is.EqualTo(-1));
        Assert.That(await db.SetAddIfKeyExistsAsync(key, [0, 1, 2]), Is.EqualTo(-1));
        Assert.That(await db.SetContainsAsync(key, 0), Is.False);

        try
        {
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, 1), Is.True);

            Assert.That(await db.KeyExistsAsync(key), Is.True);
            Assert.That(await db.KeyExpireTimeAsync(key), Is.Null);
            Assert.That(await db.SetContainsAsync(key, 1), Is.True);
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(1));

            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, 2), Is.False);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, [3, 4]), Is.EqualTo(-1));
            Assert.That(await db.SetContainsAsync(key, [2, 3, 4]), Is.EqualTo(new bool[] { false, false, false }));
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(1));

            Assert.That(await db.SetAddIfKeyExistsAsync(key, 3), Is.EqualTo(1));
            Assert.That(await db.SetContainsAsync(key, 3), Is.True);
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(2));

            Assert.That(await db.SetAddIfKeyExistsAsync(key, 3), Is.EqualTo(0));
            Assert.That(await db.SetContainsAsync(key, 3), Is.True);
            Assert.That(await db.SetAddIfKeyExistsAsync(key, [3, 1]), Is.EqualTo(0));

            Assert.That(await db.SetAddIfKeyExistsAsync(key, [4, 1, 5, 3, 6]), Is.EqualTo(3));
            Assert.That(await db.SetContainsAsync(key, [5, 4, 6]), Is.EqualTo(new bool[] { true, true, true }));
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(5));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, [7, 8]), Is.EqualTo(2));
            Assert.That(await db.KeyExpireTimeAsync(key), Is.Null);

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, [RedisValue.EmptyString]), Is.EqualTo(1));

            var expiry = DateTime.UtcNow.AddMinutes(1);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, 9, expiry), Is.False);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, [10, 11], expiry), Is.EqualTo(-1));
            Assert.That(await db.SetContainsAsync(key, [9, 10, 11]), Is.EqualTo(new bool[] { false, false, false }));
            Assert.That(await db.KeyExpireTimeAsync(key), Is.Null);

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(0));

            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, 12, expiry), Is.True);
            Assert.That(await db.SetContainsAsync(key, 12), Is.True);
            Assert.That(await db.SetLengthAsync(key), Is.EqualTo(1));

            var expireTime = await db.KeyExpireTimeAsync(key);
            Assert.That(expireTime, Is.Not.Null);
            Assert.That((expiry - expireTime.Value).Milliseconds, Is.EqualTo(0));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.SetAddIfKeyNotExistsAsync(key, [13, 14, 15, 16], expiry), Is.EqualTo(4));
            Assert.That((expiry - (await db.KeyExpireTimeAsync(key)).Value).Milliseconds, Is.EqualTo(0));
        }
        finally
        {
            await db.KeyDeleteAsync(key);
        }
    }

    [Test]
    public async Task HashSetIfKey()
    {
        RedisKey key = "Test:HashSetIfKey";
        RedisValue name1 = "name1";
        RedisValue value1 = "value1";
        HashEntry entry1 = new(name1, value1);
        RedisValue name2 = "name2";
        RedisValue value2 = "value2";
        HashEntry entry2 = new(name2, value2);
        var db = _db;

        Assert.That(await db.KeyExistsAsync(key), Is.False);

        Assert.That(await db.HashSetIfKeyExistsAsync(key, name1, value1), Is.EqualTo(-1));
        Assert.That(await db.HashSetIfKeyExistsAsync(key, [name1, value1, name2, value2]), Is.EqualTo(-1));
        Assert.That(await db.HashSetIfKeyExistsAsync(key, [entry1, entry2]), Is.EqualTo(-1));

        try
        {
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, name1, value1), Is.True);
            Assert.That(await db.KeyExistsAsync(key), Is.True);
            Assert.That(await db.KeyExpireTimeAsync(key), Is.Null);
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));

            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, name2, value2), Is.False);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [name1, value1, name2, value2]), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [entry1, entry2]), Is.EqualTo(-1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(1));

            Assert.That(await db.HashSetIfKeyExistsAsync(key, name2, value2), Is.EqualTo(1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value2));

            Assert.That(await db.HashSetIfKeyExistsAsync(key, name2, value2), Is.EqualTo(0));
            Assert.That(await db.HashSetIfKeyExistsAsync(key, [name1, value2, name2, value1]), Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value2));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));

            Assert.That(await db.HashSetIfKeyExistsAsync(key, [entry2, entry1]), Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value2));

            var expiry = DateTime.UtcNow.AddMinutes(1);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, name1, value1, expiry), Is.False);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [name1, value1, name2, value2], expiry), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [entry1, entry2], expiry), Is.EqualTo(-1));
            Assert.That(await db.KeyExpireTimeAsync(key), Is.Null);

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [name1, value1]), Is.EqualTo(1));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [entry2]), Is.EqualTo(1));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(0));

            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, name1, value1, expiry), Is.True);
            Assert.That((expiry - (await db.KeyExpireTimeAsync(key)).Value).Milliseconds, Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(1));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [name1, value2, name2, value1], expiry), Is.EqualTo(2));
            Assert.That((expiry - (await db.KeyExpireTimeAsync(key)).Value).Milliseconds, Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value2));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfKeyNotExistsAsync(key, [entry1, entry2], expiry), Is.EqualTo(2));
            Assert.That((expiry - (await db.KeyExpireTimeAsync(key)).Value).Milliseconds, Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value2));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));
        }
        finally
        {
            await db.KeyDeleteAsync(key);
        }
    }

    [Test]
    public async Task HashSetIfField()
    {
        RedisKey key = "Test:HashSetIfField";
        RedisValue name1 = "name1";
        RedisValue value1 = "value1";
        HashEntry entry1 = new(name1, value1);
        RedisValue name2 = "name2";
        RedisValue value2 = "value2";
        HashEntry entry2 = new(name2, value2);
        var db = _db;

        Assert.That(await db.KeyExistsAsync(key), Is.False);
        Assert.That(await db.HashSetIfFieldExistsAsync(key, name1, name1, value1), Is.EqualTo(-1));
        Assert.That(await db.HashSetIfFieldExistsAsync(key, [name1, name1, value1]), Is.EqualTo(-1));
        Assert.That(await db.HashSetIfFieldExistsAsync(key, name1, [entry1, entry2]), Is.EqualTo(-1));

        try
        {
            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, name1, name1, value1), Is.EqualTo(1));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashExistsAsync(key, name1), Is.True);

            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, name1, name2, value2), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, [name1, name2, value2]), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, name1, [entry2, entry1]), Is.EqualTo(-1));

            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(1));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashSetIfFieldExistsAsync(key, name1, name1, value2), Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value2));

            Assert.That(await db.KeyExistsAsync(key), Is.True);
            Assert.That(await db.HashExistsAsync(key, name2), Is.False);
            Assert.That(await db.HashSetIfFieldExistsAsync(key, name2, name2, value1), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfFieldExistsAsync(key, [name2, name2, value1]), Is.EqualTo(-1));
            Assert.That(await db.HashSetIfFieldExistsAsync(key, name2, [entry2, entry1]), Is.EqualTo(-1));

            Assert.That(await db.HashSetIfFieldExistsAsync(key, [name1, name2, value1]), Is.EqualTo(1));
            Assert.That(await db.HashExistsAsync(key, name2), Is.True);
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));

            Assert.That(await db.HashSetIfFieldExistsAsync(key, name2, [entry1, entry2]), Is.EqualTo(0));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value2));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, [name1, name1, value2, name2, value1]), Is.EqualTo(2));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value2));
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));

            Assert.That(await db.KeyDeleteAsync(key), Is.True);
            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, name1, [entry2]), Is.EqualTo(1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(1));
            Assert.That(await db.HashExistsAsync(key, name1), Is.False);
            Assert.That(await db.HashGetAsync(key, name2), Is.EqualTo(value2));

            Assert.That(await db.HashSetIfFieldNotExistsAsync(key, name1, [entry1]), Is.EqualTo(1));
            Assert.That(await db.HashLengthAsync(key), Is.EqualTo(2));
            Assert.That(await db.HashGetAsync(key, name1), Is.EqualTo(value1));
        }
        finally
        {
            await db.KeyDeleteAsync(key);
        }
    }
}