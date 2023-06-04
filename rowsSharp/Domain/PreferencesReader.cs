using rowsSharp.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Markup;

namespace rowsSharp.Domain;

internal static class PreferencesReader
{
    internal static Preferences Import(string path)
    {
        App.Logger.Info("Reading preferences");
       
        Preferences config = FromPath(path);
        config.ExpandBaseDir();
        config.ApplyTheming();
        return config;
    }

    private static Preferences FromPath(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Preferences>(json) ?? new();
        }
        catch (Exception ex)
        {
            App.Logger.Fatal(ex, "Error reading preferences");
            throw;
        }
    }

    private static void ExpandBaseDir(this Preferences config)
    {
        config.CsvPath = BaseDir.Expand(config.CsvPath);
        config.PreviewPath = BaseDir.Expand(config.PreviewPath);
        config.StylePath = BaseDir.Expand(config.StylePath);
        config.ThemePath = BaseDir.Expand(config.ThemePath);
    }

    private static void ApplyTheming(this Preferences config)
    {
        config.ColumnStyle = GetColumnStyle(config.StylePath);
        config.Theme = GetTheme(config.ThemePath);
    }

    private static ColumnStyle GetColumnStyle(string path)
    {
        if (!File.Exists(path)) { return new(); }

        App.Logger.Info("Parsing conditional formatting configurations");

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ColumnStyle>(json) ?? new();
    }

    private static ResourceDictionary GetTheme(string path)
    {
        if (!File.Exists(path)) { return new(); }

        App.Logger.Info("Parsing XAML theme file");

        string xaml = File.ReadAllText(path);
        return (ResourceDictionary)XamlReader.Parse(xaml);
    }
}
