namespace RowsSharp.Model;

/// <summary>
/// The root of all configurations.
/// This class is hierarchically structured in the same way as the JSON file.
/// </summary>
public class Preferences
{
    public Csv Csv { get; init; } = new();

    public Editor Editor { get; init; } = new();

    public Filter Filter { get; init; } = new();

    public Preview Preview { get; init; } = new();

    public UserInterface UserInterface { get; init; } = new();
}