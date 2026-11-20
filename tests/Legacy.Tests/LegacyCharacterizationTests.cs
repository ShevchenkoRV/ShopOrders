using System.Globalization;
using ShopOrders.Legacy;
using Xunit;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // формат чисел залежить від культури середовища,
        // тому фіксуємо її явно
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static List<OrderLine> TwoLines() => new()
    {
        new OrderLine { Code = "A-1", Qty = 2, Price = 150m },
        new OrderLine { Code = "B-2", Qty = 1, Price = 400m }
    };

    private static ClientKind K(string s) => Enum.Parse<ClientKind>(s, true);
    private static DocState S(string s) => Enum.Parse<DocState>(s, true);

    [Theory]
    [InlineData(null, "new", "a@b.c", "ERR: null")]
    [InlineData("empty", "new", "a@b.c", "ERR: empty")]
    [InlineData("two", "draft", "a@b.c", "ERR: state")]
    [InlineData("two", "new", "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfItems, string state, string mail, string expected)
    {
        var items = kindOfItems switch
        {
            "two" => TwoLines(),
            "empty" => new List<OrderLine>(),
            _ => null
        };
        var sut = new LegacyOrderProcessor();
        var actual = sut.Handle(1001, 7, "Іван", mail, ClientKind.Regular, 0,
            items, S(state), "UAH", new DateTime(2026, 3, 1), false);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipOrder_ReturnsExactReport()
    {
        var sut = new LegacyOrderProcessor();
        var report = sut.Handle(1001, 7, "Іван", "a@b.c",
            ClientKind.Vip, 3, TwoLines(), DocState.New, "UAH",
            new DateTime(2026, 3, 1), false);
        var expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "A-1 x2 = 300.00 UAH\n" +
            "B-2 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Theory]
    [InlineData("regular", 0, 760)]
    [InlineData("vip", 0, 655)]
    [InlineData("staff", 0, 550)]
    [InlineData("regular", 11, 725)]
    public void Preview_ByKind_ReturnsTotal(string kind, int done, decimal expected)
    {
        var sut = new LegacyOrderProcessor();
        var total = sut.Preview(K(kind), done, TwoLines());
        Assert.Equal(expected, total);
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        var c = new Buyer
        {
            Name = " іван ", Email = "IVAN@MAIL.COM",
            Kind = ClientKind.Vip, DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1)
        };
        // метод залежить від системного годинника,
        // тому очікування обчислюємо, а не вписуємо
        var years = DateTime.Now.Year - 2020;
        var expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] " +
            "<ivan@mail.com> стаж " + years;
        var actual = new LegacyOrderProcessor().DescribeClient(c);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Preview_ExactlyAtFreeShippingBoundary_ChargesNoShipping()
    {
        // сума після знижки рівно 1000 (regular, без знижки):
        // умова «< 1000» хибна, доставка не стягується
        var items = new List<OrderLine>
        {
            new OrderLine { Code = "X", Qty = 1, Price = 1000m }
        };
        var total = new LegacyOrderProcessor().Preview(ClientKind.Regular, 0, items);
        Assert.Equal(1000m, total);
    }

    [Fact]
    public void DumpLog_AfterHandleWithMail_ReturnsLogLines()
    {
        var sut = new LegacyOrderProcessor();
        sut.Handle(1001, 7, "Іван", "a@b.c", ClientKind.Regular, 0,
            TwoLines(), DocState.New, "UAH", new DateTime(2026, 3, 1), true);
        Assert.Equal("ok 1001\nmail -> a@b.c\n", sut.DumpLog());
    }
}
