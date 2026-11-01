namespace ShopOrders.Core.Reporting;

/// <summary>
/// Виводить готовий звіт у вказаний потік. Про консоль нічого не знає.
/// </summary>
public sealed class DocumentReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>
    /// Створює друкаря, який пише у вказаний потік.
    /// </summary>
    /// <param name="output">Потік виведення.</param>
    public DocumentReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>
    /// Записує текст звіту в потік виведення.
    /// </summary>
    /// <param name="report">Текст звіту, не порожній.</param>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
