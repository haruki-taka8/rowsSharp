using ObservableTable.Core;

namespace UnitTest.Domain;

[TestClass]
public class UniqueColumn
{
    [TestMethod]
    public void UniqueColumn_NoParameter_OneColumn()
    {
        var expected = new Column<string>[] { new("1") };

        RowsSharp.Domain.UniqueColumn provider = new();
        var actual = provider.Next();

        Assert.IsTrue(expected.SequenceEqual(actual));
    }


    [TestMethod]
    public void UniqueColumn_NegativeParameter_NoColumn()
    {
        var expected = Array.Empty<Column<string>>();

        RowsSharp.Domain.UniqueColumn provider = new();
        var actual = provider.Next(-1);

        Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [TestMethod]
    public void UniqueColumn_ZeroParameter_NoColumn()
    {
        var expected = Array.Empty<Column<string>>();

        RowsSharp.Domain.UniqueColumn provider = new();
        var actual = provider.Next(0);

        Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [TestMethod]
    public void UniqueColumn_GreaterThanOneParameter_MultipleColumns()
    {
        var expected = new Column<string>[] { new("1"), new("2"), new("3") };

        RowsSharp.Domain.UniqueColumn provider = new();
        var actual = provider.Next(3);

        Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [TestMethod]
    public void UniqueColumn_MultipleTimes_UniqueColumns()
    {
        var expected = new Column<string>[] { new("1"), new("2"), new("3") };

        RowsSharp.Domain.UniqueColumn provider = new();
        List<Column<string>> actual = new(provider.Next(1));
        actual.AddRange(provider.Next(2));

        Assert.IsTrue(expected.SequenceEqual(actual));
    }
}