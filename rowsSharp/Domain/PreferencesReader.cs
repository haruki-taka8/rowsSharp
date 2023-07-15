using Newtonsoft.Json;
using rowsSharp.Model;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace rowsSharp.Domain;

internal static class PreferencesReader
{
    internal static Preferences Import(string path)
    {
        App.Logger.Info("Reading preferences");
       
        Preferences config = FromPath(path);
        config.Editor.AutosaveInterval *= 1000;
        config.UserInterface.Theme = GetTheme(config.UserInterface.ThemePath);

        return config;
    }

    private static Preferences FromPath(string path)
    {
        string json = File.ReadAllText(path);
        json = BaseDir.ExpandEscaped(json);

        return JsonConvert.DeserializeObject<Preferences>(json) ?? new();
    }

    private static ResourceDictionary? GetTheme(string path)
    {
        if (!File.Exists(path)) { return null; }

        App.Logger.Info("Parsing XAML theme file");

        string xaml = File.ReadAllText(path);
        return (ResourceDictionary)XamlReader.Parse(xaml);
    }
}
