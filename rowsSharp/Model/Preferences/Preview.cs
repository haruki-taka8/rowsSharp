namespace RowsSharp.Model;

public class Preview
{
    /// <summary>
    /// Path to a preview image when a row is selected.
    /// A value of &lt;Column&gt; will be substituted by the content of that column of the selected row.
    /// </summary>
    public string Path { get; set; } = "$baseDir/CSVData/<A>.png";

    /// <summary>
    /// The width of the preview image.
    /// Values like "Auto" and "*" are supported.
    /// </summary>
    public string Width { get; set; } = "*";
}