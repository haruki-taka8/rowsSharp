namespace UnitTest.Domain;

[TestClass]
public class BaseDir
{
    [TestMethod]
    public void BaseDir_Empty_Empty()
    {
        string expected = "";
        string actual = "";

        actual = RowsSharp.Domain.BaseDir.Expand(actual);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void BaseDir_Single_Replaced()
    {
        string expected = Environment.CurrentDirectory + "/Userdata/";
        string actual = "$baseDir";

        actual = RowsSharp.Domain.BaseDir.Expand(actual);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void BaseDir_Multiple_Replaced()
    {
        string expected = Environment.CurrentDirectory + "/Userdata/";
        expected += expected;
        string actual = "$baseDir$baseDir";

        actual = RowsSharp.Domain.BaseDir.Expand(actual);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void BaseDir_Mixed_Replaced()
    {
        string expected = Environment.CurrentDirectory + "/Userdata/";
        expected = "Prefix " + expected + " Suffix";
        string actual = "Prefix $baseDir Suffix";

        actual = RowsSharp.Domain.BaseDir.Expand(actual);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void BaseDir_LongPath_Replaced()
    {
        // Windows API defaults path length to a cap of 259 characters + NUL.
        const int MAX_PATH = 259;
        string expected = Environment.CurrentDirectory + "/Userdata/";
        string actual = "$baseDir";

        do
        {
            expected += expected;
            actual += actual;
        }
        while (expected.Length <= MAX_PATH);

        actual = RowsSharp.Domain.BaseDir.Expand(actual);

        Assert.AreEqual(expected, actual);
    }
}