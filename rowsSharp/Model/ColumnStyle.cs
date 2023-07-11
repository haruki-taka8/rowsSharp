using System.Collections.Generic;

namespace rowsSharp.Model;

public class ColumnStyle
{
    /// <summary>
    /// The width of the column
    /// </summary>
    /// <remarks>
    /// This value is converted to a <see cref="System.Windows.Controls.DataGridLength"/> internally.
    /// </remarks>
    public double Width { get; set; }

    /// <summary>
    /// The template of the column.
    /// This value will be substituted into each new row of that column during insertion if Insertion Template is enabled.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// A cell matching one of the keys completely and literally will have a background color of that key's value.
    /// </summary>
    /// <remarks>
    /// This value is convert into a <see cref="System.Windows.Media.SolidColorBrush"/> internally.
    /// </remarks>
    public Dictionary<string, string> ConditionalFormatting { get; init; } = new();
}