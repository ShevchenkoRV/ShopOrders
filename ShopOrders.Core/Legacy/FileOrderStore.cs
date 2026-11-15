using System.IO;

namespace ShopOrders.Core.Legacy;

public class FileOrderStore
{
    public void SaveToFile(LegacyOrder order, string path)
    {
        File.WriteAllText(path, order.Id.ToString());
    }
}
