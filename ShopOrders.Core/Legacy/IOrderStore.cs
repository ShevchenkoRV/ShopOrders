namespace ShopOrders.Core.Legacy;

public interface IOrderStore
{
    void Add(LegacyOrder order);

    LegacyOrder GetById(int id);

    void SaveToFile(LegacyOrder order, string path);

    void SendEmail(string to, string body);

    string ExportCsv();

    void Backup(string folder);

    void PrintInvoice(LegacyOrder order);
}
