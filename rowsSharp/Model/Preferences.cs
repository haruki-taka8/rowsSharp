using System.IO;
using System.Windows;

namespace rowsSharp.Model;

public class Preferences : NotifyPropertyChanged
{
    public string CsvPath { get; set; } = "";
    public string CsvName => Path.GetFileName(CsvPath);

    public bool HasHeader { get; init; } = true;
    public string StylePath { get; set; } = "";
    public string PreviewPath { get; set; } = "";

    public bool UseInputAlias { get; set; }
    private bool useOutputAlias;
    public bool UseOutputAlias
    {
        get => useOutputAlias;
        set => SetField(ref useOutputAlias, value);
    }
    public bool UseRegexFilter { get; set; }

    private bool canEdit;
    public bool CanEdit
    {
        get => canEdit;
        set
        {
            SetField(ref canEdit, value);
            OnPropertyChanged(nameof(IsReadOnly));
        }
    }
    public bool IsReadOnly => !CanEdit;
    public bool AllowMultiline { get; set; }
    public bool UseInsertTemplate { get; set; } = true;

    public bool UseAutosave { get; set; } = true;
    public int AutosavePeriod { get; set; } = 60;

    public bool UseToolTip { get; set; } = true;

    public string ThemePath { get; set; } = "";
    internal ResourceDictionary? Theme { get; set; }

    public ColumnStyle ColumnStyle { get; set; } = new();
}
