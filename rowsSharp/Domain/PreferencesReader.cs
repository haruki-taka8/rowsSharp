using System.Text.Json;
using RowsSharp.Model;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace RowsSharp.Domain;

internal static class PreferencesReader
{
    internal static Preferences Import(string path)
    {
        App.Logger.Info("Reading preferences");
       
        Preferences config = FromPath(path);

        return config;
    }

    private static Preferences FromPath(string path)
    {
        string json = File.ReadAllText(path);
        json = BaseDir.ExpandEscaped(json);

        return JsonSerializer.Deserialize<Preferences>(json) ?? new();
    }

    internal static ResourceDictionary GetTheme(string path)
    {
        if (!File.Exists(path)) { return new(); }

        App.Logger.Info("Parsing XAML theme file");

        string xaml = File.ReadAllText(path);
        return (ResourceDictionary)XamlReader.Parse(xaml);
    }
}
