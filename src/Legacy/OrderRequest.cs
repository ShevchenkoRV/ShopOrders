namespace ShopOrders.Legacy;

public sealed record OrderRequest(
    int DocId,
    Buyer Client,
    IReadOnlyList<OrderLine>? Items,
    DocState State,
    string Currency,
    bool SendMail);
