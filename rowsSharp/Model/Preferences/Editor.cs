using System.Collections.Generic;

namespace RowsSharp.Model;

public class Editor
{
    /// <summary>
    /// The width of the DataGrid.
    /// <see cref="System.Windows.Controls.DataGridLength"/> values like "Auto" and "*" are supported.
    /// </summary>
    public string Width { get; set; } = "2*";

    /// <summary>
    /// Whether the CSV file can be edited by the user.
    /// </summary>
    public bool CanEdit { get; set; } = true;

    /// <summary>
    /// Describes whether the user can add multiple lines to a single cell.
    /// This value does not affect existing multiline cells.
    /// </summary>
    public bool CanInsertNewline { get; init; }

    /// <summary>
    /// Describes whether Insertion Templates are used or not.
    /// </summary>
    public bool IsTemplateEnabled { get; set; } = true;

    /// <summary>
    /// Describes whether changes are saved automatically and periodically.
    /// </summary>
    public bool IsAutosaveEnabled { get; set; } = true;

    /// <summary>
    /// The period, in milliseconds, when an autosave occurs.
    /// </summary>
    public int AutosaveInterval { get; set; } = 60000;

    /// <summary>
    /// Number of columns from the left that cannot be scrolled out of view.
    /// </summary>
    public int FrozenColumn { get; set; }

    /// <summary>
    /// Fallback height of a row.
    /// </summary>
    public double DefaultRowHeight { get; set; } = 33;

    /// <summary>
    /// Fallback width of a column.
    /// </summary>
    /// <remarks>
    /// ColumnStyles can override this value on a column-to-column basis.
    /// </remarks>
    public double DefaultColumnWidth { get; set; } = 50;

    /// <summary>
    /// Whether scrolling only occurs after the scrollbar has finished moving.
    /// This saves computing resources at an expense of less responsiveness.
    /// </summary>
    public bool IsDeferredScrollingEnabled { get; set; }

    /// <summary>
    /// Describes columns with user-provided definitions.
    /// </summary>
    /// <remarks>
    /// Each key-value pair is converted to a <see cref="System.Windows.Style"/> internally.
    /// </remarks>
    public Dictionary<string, ColumnStyle> ColumnStyles { get; set; } = new();
}
