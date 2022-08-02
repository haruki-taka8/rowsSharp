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
}

public class RecordMap : ClassMap<Record>
{
    public const int MaxColumns = 32;
    public RecordMap()
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
