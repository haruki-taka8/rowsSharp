using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
    public string CheckBoxTrueValue { get; set; } = "true";

    /// <summary>
    /// A value considered false if <see cref="ColumnType"/> is <see cref="ColumnType.CheckBox"/>.
    /// </summary>
    public string CheckBoxFalseValue { get; set; } = "false";

    /// <summary>
    /// Values that can be chosen from if <see cref="ColumnType"/> is <see cref="ColumnType.ComboBox"/>.
    /// </summary>
    public string[] ComboBoxOptions { get; set; } = Array.Empty<string>();

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
    public IEnumerable<ConditionalFormatting> ConditionalFormatting { get; set; }
        = new List<ConditionalFormatting>();
}