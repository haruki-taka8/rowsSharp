using System.Collections.Generic;

namespace rowsSharp.Model;

public class StyleConfig
{
    public Dictionary<string, int> Width { get; init; } = new();
    public Dictionary<string, string> Template { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Alias { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Color { get; init; } = new();
}

public class Config
{
    public string CsvPath { get; init; } = "$baseDir/CSVData/data.csv";
    public bool HasHeader { get; init; } = true;
    public string StylePath { get; init; } = string.Empty;
    public string PreviewPath { get; init; } = string.Empty;
    public string CopyRowFormat { get; init; } = string.Empty;
    public bool UseInputAlias { get; set; }
    public bool UseOutputAlias { get; set; }
    public bool UseRegexFilter { get; init; }
    public bool UseToolTip { get; init; } = true;
    public bool CanEdit { get; set; } = true;
    public bool AllowMultiline { get; set; } = true;
    public bool InsertSelectedCount { get; set; }
    public int InsertCount { get; init; }
    public bool UseInsertTemplate { get; set; } = true;
    public string ThemePath { get; init; } = "$baseDir/Configurations/Themes/Light.xaml";
    public double StartupWidth { get; init; } = 1400;
    public double StartupHeight { get; init; } = 600;
    public int FrozenColumn { get; init; }
    public int MinRowHeight { get; init; } = 25;
    public string PreviewWidth { get; init; } = string.Empty;
    public string FontFamily { get; init; } = "Courier New,Roboto Mono";
    public double PrimaryFontSize { get; init; }
    public double SecondaryFontSize { get; init; }
    public StyleConfig Style { get; init; } = new();
}
