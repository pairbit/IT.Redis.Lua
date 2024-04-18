using System.Reflection;

namespace IT.Redis.Lua.Internal;

internal static class Lua
{
    public static readonly string SetAddIfKeyNotExists;
    public static readonly string SetAddAndExpireIfKeyNotExists;
    public static readonly string SetAddIfKeyExists;

    public static readonly string HashSetIfKeyNotExists;
    public static readonly string HashSetAndExpireIfKeyNotExists;
    public static readonly string HashSetIfKeyExists;

    public static readonly string HashSetIfFieldExists;
    public static readonly string HashSetIfFieldNotExists;

    static Lua()
    {
        var assembly = Assembly.GetExecutingAssembly();

        SetAddIfKeyNotExists = assembly.GetLua(nameof(SetAddIfKeyNotExists));
        SetAddAndExpireIfKeyNotExists = assembly.GetLua(nameof(SetAddAndExpireIfKeyNotExists));
        SetAddIfKeyExists = assembly.GetLua(nameof(SetAddIfKeyExists));

        HashSetIfKeyNotExists = assembly.GetLua(nameof(HashSetIfKeyNotExists));
        HashSetAndExpireIfKeyNotExists = assembly.GetLua(nameof(HashSetAndExpireIfKeyNotExists));
        HashSetIfKeyExists = assembly.GetLua(nameof(HashSetIfKeyExists));

        HashSetIfFieldExists = assembly.GetLua(nameof(HashSetIfFieldExists));
        HashSetIfFieldNotExists = assembly.GetLua(nameof(HashSetIfFieldNotExists));
    }

    private static string GetLua(this Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream($"IT.Redis.Lua.Lua.{name}.lua")
            ?? throw new InvalidOperationException($"Lua script '{name}' not found");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}