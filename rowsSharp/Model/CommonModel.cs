using ObservableTable.Core;
using System;
using System.Reflection;

namespace RowsSharp.Model;

/// <summary>
/// Values that can be accessed by all ViewModels
/// </summary>
public class CommonModel
{
    /// <summary>
    /// The full version object of RowsSharp
    /// </summary>
    public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version!;

    /// <summary>
    /// User settings. Currently read from a JSON file.
    /// </summary>
    public Preferences Preferences { get; }

    /// <summary>
    /// A WPF-friendly representation of the CSV file.
    /// </summary>
    public ObservableTable<string> Table { get; }

    // Constructors
    public CommonModel()
    {
        Preferences = new();
        Table = new();
    }

    public CommonModel(Preferences preferences, ObservableTable<string> table)
    {
        Preferences = preferences;
        Table = table;
    }
}
