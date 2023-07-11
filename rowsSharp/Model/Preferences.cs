namespace rowsSharp.Model;

/// <summary>
/// The root of all configurations.
/// </summary>
public class Preferences
{
    public Csv Csv { get; set; } = new();

    public Editor Editor { get; set; } = new();

    public Filter Filter { get; set; } = new();

    public Preview Preview { get; set; } = new();

    public UserInterface UserInterface { get; set; } = new();
}