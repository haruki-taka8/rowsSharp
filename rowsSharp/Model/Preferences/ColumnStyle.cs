using System.Collections.Generic;

namespace RowsSharp.Model;

public class ColumnStyle
{
    /// <summary>
    /// The name of the column.
    /// </summary>
    public string Column { get; set; } = "";

    /// <summary>
    /// Type of this column. It is the basis to decide what kind of DataGridColumn is generated.
    /// </summary>
    public ColumnType ColumnType { get; set; }

    /// <summary>
    /// A value considered true if <see cref="ColumnType"/> is <see cref="ColumnType.CheckBox"/>.
    /// </summary>
    public string CheckBoxTrueValue { get; set; } = "TRUE";

    /// <summary>
    /// A value considered false if <see cref="ColumnType"/> is <see cref="ColumnType.CheckBox"/>.
    /// </summary>
    public string CheckBoxFalseValue { get; set; } = "FALSE";

    /// <summary>
    /// The width of the column
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// The minimum width of the column
    /// </summary>
    public double MinWidth { get; set; } = 32;

    /// <summary>
    /// The default maximum width of the column
    /// </summary>
    public double MaxWidth { get; set; } = double.PositiveInfinity;

    /// <summary>
    /// The template of the column.
    /// This value will be substituted into each new row of that column during insertion if Insertion Template is enabled.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// A cell matching one of the keys completely and literally will have a background color of that key's value.
    /// </summary>
    public IEnumerable<ConditionalFormatting> ConditionalFormatting { get; set; }
        = new List<ConditionalFormatting>();
}