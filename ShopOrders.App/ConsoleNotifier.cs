using System;
using ShopOrders.Core.Abstractions;

namespace ShopOrders.App;

/// <summary>
/// Сповіщувач, що пише в консоль.
/// </summary>
internal sealed class ConsoleNotifier : INotifier
{
    /// <inheritdoc />
    public void Notify(string recipient, string message)
    {
        Console.WriteLine(recipient + ": " + message);
    }
}
