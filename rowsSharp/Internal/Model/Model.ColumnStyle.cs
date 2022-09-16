using System.Collections.Generic;

namespace rowsSharp.Model;

public struct ColumnStyle
{
    public ColumnStyle() { }
    public Dictionary<string, int> Width { get; init; } = new();
    public Dictionary<string, string> Template { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Alias { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Color { get; init; } = new();
}
