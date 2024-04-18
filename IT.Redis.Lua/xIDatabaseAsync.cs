namespace IT.Redis.Lua;

using Internal;

public static class xIDatabaseAsync
{
    internal static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static async Task<bool> SetAddIfKeyNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue value, DateTime? expiry = null, CommandFlags flags = CommandFlags.None)
        => (expiry == null
           ? (long)await db.ScriptEvaluateAsync(Lua.SetAddIfKeyNotExists, [key], [value], flags).ConfigureAwait(false)
           : (long)await db.ScriptEvaluateAsync(Lua.SetAddAndExpireIfKeyNotExists, [key], [GetMilliseconds(expiry.Value), value], flags).ConfigureAwait(false)) > 0;

    public static async Task<long> SetAddIfKeyNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] values, DateTime? expiry = null, CommandFlags flags = CommandFlags.None)
        => expiry == null
           ? (long)await db.ScriptEvaluateAsync(Lua.SetAddIfKeyNotExists, [key], values, flags).ConfigureAwait(false)
           : (long)await db.ScriptEvaluateAsync(Lua.SetAddAndExpireIfKeyNotExists, [key], Concat(GetMilliseconds(expiry.Value), values), flags).ConfigureAwait(false);

    public static async Task<int> SetAddIfKeyExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        => (int)await db.ScriptEvaluateAsync(Lua.SetAddIfKeyExists, [key], [value], flags).ConfigureAwait(false);

    public static async Task<long> SetAddIfKeyExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.SetAddIfKeyExists, [key], values, flags).ConfigureAwait(false);

    public static async Task<bool> HashSetIfKeyNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue hashField, RedisValue value, DateTime? expiry = null, CommandFlags flags = CommandFlags.None)
        => (expiry == null
           ? (long)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyNotExists, [key], [hashField, value], flags).ConfigureAwait(false)
           : (long)await db.ScriptEvaluateAsync(Lua.HashSetAndExpireIfKeyNotExists, [key], [GetMilliseconds(expiry.Value), hashField, value], flags).ConfigureAwait(false)) > 0;

    public static async Task<long> HashSetIfKeyNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] hashFields, DateTime? expiry = null, CommandFlags flags = CommandFlags.None)
        => expiry == null
           ? (long)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyNotExists, [key], hashFields, flags).ConfigureAwait(false)
           : (long)await db.ScriptEvaluateAsync(Lua.HashSetAndExpireIfKeyNotExists, [key], Concat(GetMilliseconds(expiry.Value), hashFields), flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfKeyNotExistsAsync(this IDatabaseAsync db, RedisKey key, HashEntry[] hashFields, DateTime? expiry = null, CommandFlags flags = CommandFlags.None)
        => expiry == null
           ? (long)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyNotExists, [key], Cast(hashFields), flags).ConfigureAwait(false)
           : (long)await db.ScriptEvaluateAsync(Lua.HashSetAndExpireIfKeyNotExists, [key], Concat(GetMilliseconds(expiry.Value), hashFields), flags).ConfigureAwait(false);

    public static async Task<int> HashSetIfKeyExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue hashField, RedisValue value, CommandFlags flags = CommandFlags.None)
        => ((int)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyExists, [key], [hashField, value], flags).ConfigureAwait(false));

    public static async Task<long> HashSetIfKeyExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyExists, [key], hashFields, flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfKeyExistsAsync(this IDatabaseAsync db, RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfKeyExists, [key], Cast(hashFields), flags).ConfigureAwait(false);

    //HashSetIfKeyExistsAndFieldNotExistsAsync

    public static async Task<int> HashSetIfFieldExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue existsField, RedisValue hashField, RedisValue value, CommandFlags flags = CommandFlags.None)
        => (int)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldExists, [key], [existsField, hashField, value], flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfFieldExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldExists, [key], hashFields, flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfFieldExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue existsField, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldExists, [key], Concat(existsField, hashFields), flags).ConfigureAwait(false);

    public static async Task<int> HashSetIfFieldNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue notExistsField, RedisValue hashField, RedisValue value, CommandFlags flags = CommandFlags.None)
        => (int)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldNotExists, [key], [notExistsField, hashField, value], flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfFieldNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldNotExists, [key], hashFields, flags).ConfigureAwait(false);

    public static async Task<long> HashSetIfFieldNotExistsAsync(this IDatabaseAsync db, RedisKey key, RedisValue notExistsField, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        => (long)await db.ScriptEvaluateAsync(Lua.HashSetIfFieldNotExists, [key], Concat(notExistsField, hashFields), flags).ConfigureAwait(false);

    private static long GetMilliseconds(DateTime expiry) => expiry.Kind switch
    {
        DateTimeKind.Local or DateTimeKind.Utc => (expiry.ToUniversalTime() - UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond,
        _ => throw new ArgumentException("Expiry time must be either Utc or Local", nameof(expiry)),
    };

    private static RedisValue[] Concat(RedisValue firstValue, RedisValue[] values)
    {
        var newValues = new RedisValue[values.Length + 1];

        newValues[0] = firstValue;
        values.CopyTo(newValues, 1);

        return newValues;
    }

    private static RedisValue[] Concat(RedisValue firstValue, HashEntry[] hashFields)
    {
        var values = Cast(hashFields, 1);

        values[0] = firstValue;

        return values;
    }

    private static RedisValue[] Cast(HashEntry[] hashFields, int offset = 0)
    {
        var values = new RedisValue[(hashFields.Length * 2) + offset];

        for (int i = 0; i < hashFields.Length; i++)
        {
            var hashField = hashFields[i];
            values[offset++] = hashField.Name;
            values[offset++] = hashField.Value;
        }

        return values;
    }
}