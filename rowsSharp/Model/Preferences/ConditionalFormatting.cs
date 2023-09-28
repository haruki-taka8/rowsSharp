namespace RowsSharp.Model;

public class ConditionalFormatting
{
    /// <summary>
    /// When a cell is exactly equal to this string, conditional formatting will be applied.
    /// </summary>
    public string Match { get; set; } = "";

    /// <summary>
    /// The background color of the cell should the cell content match <see cref="Match"/>.
    /// </summary>
    /// <remarks>
    /// This value is convert into a <see cref="System.Windows.Media.SolidColorBrush"/> internally.
    /// </remarks>
    public string Background { get; set; } = "";
}
