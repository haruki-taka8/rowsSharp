using rowsSharp.Model;

namespace rowsSharp.DataStore;

public class Config : INPC
{
    public string CsvPath { get; set; } = "$baseDir/CSVData/data.csv";
    public bool HasHeader { get; init; } = true;
    public string StylePath { get; set; } = string.Empty;
    public string PreviewPath { get; set; } = string.Empty;
    public string CopyRowFormat { get; init; } = string.Empty;

    public bool UseInputAlias { get; set; }

    private bool useOutputAlias;
    public bool UseOutputAlias
    {
        get => useOutputAlias;
        set => SetField(ref useOutputAlias, value);
    }

    public bool UseRegexFilter { get; init; }
    public bool UseToolTip { get; init; } = true;

    private bool canEdit;
    public bool CanEdit
    {
        get => canEdit;
        set => SetField(ref canEdit, value);
    }

    internal bool OriginalCanEdit { get; set; } = true;
    public bool AllowMultiline { get; set; } = true;

    private bool insertSelectedCount;
    public bool InsertSelectedCount
    {
        get => insertSelectedCount;
        set => SetField(ref insertSelectedCount, value);
    }

    public int InsertCount { get; init; }
    public bool UseInsertTemplate { get; set; }
    public string ThemePath { get; set; } = "$baseDir/Configurations/Themes/Light.xaml";
    public ColumnStyle Style { get; set; }
}
