using CsvHelper.Configuration;

namespace rowsSharp.Model;

public class Record
{
    public string Column0 { get; set; } = string.Empty;
    public string Column1 { get; set; } = string.Empty;
    public string Column2 { get; set; } = string.Empty;
    public string Column3 { get; set; } = string.Empty;
    public string Column4 { get; set; } = string.Empty;
    public string Column5 { get; set; } = string.Empty;
    public string Column6 { get; set; } = string.Empty;
    public string Column7 { get; set; } = string.Empty;
    public string Column8 { get; set; } = string.Empty;
    public string Column9 { get; set; } = string.Empty;
    public string Column10 { get; set; } = string.Empty;
    public string Column11 { get; set; } = string.Empty;
    public string Column12 { get; set; } = string.Empty;
    public string Column13 { get; set; } = string.Empty;
    public string Column14 { get; set; } = string.Empty;
    public string Column15 { get; set; } = string.Empty;
    public string Column16 { get; set; } = string.Empty;
    public string Column17 { get; set; } = string.Empty;
    public string Column18 { get; set; } = string.Empty;
    public string Column19 { get; set; } = string.Empty;
    public string Column20 { get; set; } = string.Empty;
    public string Column21 { get; set; } = string.Empty;
    public string Column22 { get; set; } = string.Empty;
    public string Column23 { get; set; } = string.Empty;
    public string Column24 { get; set; } = string.Empty;
    public string Column25 { get; set; } = string.Empty;
    public string Column26 { get; set; } = string.Empty;
    public string Column27 { get; set; } = string.Empty;
    public string Column28 { get; set; } = string.Empty;
    public string Column29 { get; set; } = string.Empty;
    public string Column30 { get; set; } = string.Empty;
    public string Column31 { get; set; } = string.Empty;

    internal string GetField(int column)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return string.Empty; }

        // We're absolutely sure that
        // record is not null && "Column" + column is a valid field
        #pragma warning disable CS8602, CS8603
        return GetType()
            .GetProperty("Column" + column)
            .GetValue(this).ToString();
        #pragma warning restore CS8602, CS8603
    }

    internal void SetField(int column, string value)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return; }
        #pragma warning disable CS8602
        GetType()
            .GetProperty("Column" + column)
            .SetValue(this, value);
        #pragma warning restore CS8602
    }

    internal Record DeepCopy(int columnCount = RecordMap.MaxColumns - 1)
    {
        Record output = new();
        for (int i = 0; i < columnCount; i++)
        {
            output.SetField(i, GetField(i));
        }
        return output;
    }

    internal string ConcatenateFields(int columnCount = RecordMap.MaxColumns - 1)
    {
        string output = string.Empty;
        for (int i = 0; i < columnCount; i++)
        {
            output += '"' + GetField(i).Replace("\"", "\"\"") + "\",";
        }
        return output.TrimEnd(',');
    }
}

internal class RecordMap : ClassMap<Record>
{
    internal const int MaxColumns = 32;
    internal RecordMap()
    {
        for (int i = 0; i < MaxColumns - 1; i++)
        {
            // Ultra thanks to David Specht on https://stackoverflow.com/a/62601123
            Map(
                typeof(Record),
                typeof(Record).GetProperty("Column" + i))
                .Optional()
                .Index(i);
        }
    }
}
