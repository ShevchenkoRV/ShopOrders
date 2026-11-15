namespace ShopOrders.Core.Abstractions;

/// <summary>
/// Сповіщувач: відповідає лише за повідомлення.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Надсилає повідомлення.
    /// </summary>
    /// <param name="recipient">Отримувач.</param>
    /// <param name="message">Текст.</param>
    void Notify(string recipient, string message);
}
