namespace UnitTest.Domain;

[TestClass]
public class ColumnNotation
{
    private readonly static List<string> Headers = new() { "A0", "B0", "C0" };
    private readonly static List<string?> Row = new() { "A1", "B1", "C1" };

    [TestMethod]
    public void ColumnNotation_Empty_Empty()
    {
        string expected = "";
        string actual = "";

        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Array.Empty<string>(), Array.Empty<string?>());

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ColumnNotation_Single_Replaced()
    {
        string expected = "A1";
        string actual = "<A0>";

        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Headers, Row);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ColumnNotation_Multiple_Replaced()
    {
        string expected = "A1B1";
        string actual = "<A0><B0>";

        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Headers, Row);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ColumnNotation_Mixed_Replaced()
    {
        string expected = "A1 B1 foo bar C1 baz";
        string actual = "<A0> <B0> foo bar <C0> baz";

        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Headers, Row);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ColumnNotation_InvalidColumn_Ignored()
    {
        string expected = "<InvalidColumn>";
        string actual = "<InvalidColumn>";

        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Headers, Row);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ColumnNotation_DifferentLengthLists_Replaced()
    {
        string expected = "A1";
        string actual = "<A0>";

        var row = Row.ToList();
        row.RemoveAt(2);
        actual = RowsSharp.Domain.ColumnNotation.Expand(actual, Headers, row);

        Assert.AreEqual(expected, actual);
    }
}