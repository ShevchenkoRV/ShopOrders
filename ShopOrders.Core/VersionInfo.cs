using System.Reflection;

namespace ShopOrders.Core;

public static class VersionInfo
{
    public static string Current
    {
        get
        {
            var attr = typeof(VersionInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = attr?.InformationalVersion ?? "unknown";
            var plus = version.IndexOf('+', StringComparison.Ordinal);
            return plus >= 0 ? version[..plus] : version;
        }
    }
}
