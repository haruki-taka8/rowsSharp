using System.Collections.Generic;

namespace rowsSharp.Model;

public class ColumnStyle
{
    public Dictionary<string, int> Width { get; init; } = new();
    public Dictionary<string, string> Template { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Alias { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> Color { get; init; } = new();
}
