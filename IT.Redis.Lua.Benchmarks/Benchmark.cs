using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using StackExchange.Redis;

namespace IT.Redis.Lua.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class Benchmark
{
    private static readonly IDatabase _db;
    private static readonly DateTime Expiry = DateTime.UtcNow.AddHours(2);

    static Benchmark()
    {
        var connection = ConnectionMultiplexer.Connect("localhost:6381,defaultDatabase=0,syncTimeout=5000,allowAdmin=False,connectTimeout=5000,ssl=False,abortConnect=False");
        _db = connection.GetDatabase()!;
    }

    [Benchmark]
    public async Task<bool> Lua()
    {
        RedisKey key = Guid.NewGuid().ToByteArray();

        var isTrue = await _db.HashSetIfKeyNotExistsAsync(key, "lua", "value", Expiry);

        if (!isTrue) throw new InvalidOperationException("Lua");

        await _db.KeyDeleteAsync(key);

        return isTrue;
    }

    [Benchmark]
    public async Task<bool> Tran()
    {
        var tran = _db.CreateTransaction();

        RedisKey key = Guid.NewGuid().ToByteArray();

        tran.AddCondition(Condition.KeyNotExists(key));

        var hashRes = tran.HashSetAsync(key, "tran", "value");

        var keyExpire = tran.KeyExpireAsync(key, Expiry);

        var isTrue = await tran.ExecuteAsync() && hashRes.Result && keyExpire.Result;

        if (!isTrue) throw new InvalidOperationException("Tran");

        await _db.KeyDeleteAsync(key);

        return isTrue;
    }
}