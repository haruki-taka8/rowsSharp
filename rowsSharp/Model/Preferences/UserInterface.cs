using System.Windows;

namespace RowsSharp.Model;

public class UserInterface
{
    /// <summary>
    /// The width of the window.
    /// </summary>
    /// <remarks>
    /// It is deliberately prohibited to modify the window size from code behind after initialization.
    /// </remarks>
    public double WindowWidth { get; init; } = 1400;

    /// <summary>
    /// The height of the window.
    /// </summary>
    /// <remarks>
    /// It is deliberately prohibited to modify the window size from code behind after initialization.
    /// </remarks>
    public double WindowHeight { get; init; } = 600;

    /// <summary>
    /// The file path to a XAML file containing user-defined color brushes and/or styles.
    /// </summary>
    public string ThemePath { get; init; } = "$baseDir/Configurations/Themes/Light.xaml";

    /// <summary>
    /// Whether user-friendly text is displayed upon hovering an element, especially a button.
    /// </summary>
    public bool IsToolTipEnabled { get; set; } = true;
}
