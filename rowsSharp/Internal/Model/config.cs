using System.Collections.Generic;

namespace rowsSharp.Model
{
    public class StyleConfig
    {
        public Dictionary<string, int> Width { get; set; } = new();
        public Dictionary<string, string> Template { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> Alias { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> Color { get; set; } = new();
    }

    public class Config
    {
        public string CsvPath { get; set; } = string.Empty;
        public string PreviewPath { get; set; } = string.Empty;
        public int InsertCount { get; set; }
        public bool InsertSelectedCount { get; set; }
        public bool InputAlias { get; set; }
        public bool OutputAlias { get; set; }
        public bool ReadWrite { get; set; }
        public bool IsTemplate { get; set; }
        public bool HasHeader { get; set; }
        public int FrozenColumn { get; set; }
        public string PreviewWidth { get; set; } = "*";
        public string FontFamily { get; set; } = "Courier New";
        public string CopyRowFormat { get; set; } = string.Empty;
        public StyleConfig Style { get; set; } = new();
    }
}
