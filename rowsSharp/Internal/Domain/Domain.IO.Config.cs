using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Markup;

namespace rowsSharp.Domain.IO;

internal static class Config
{
    private const string ConfigPath = "./Userdata/Configurations/Configuration.json";

    internal static DataStore.Config Import(string path = ConfigPath)
    {
        // General configuration
        if (!File.Exists(path))
        {
            FileNotFoundException ex = new(path);
            App.Logger.Fatal(ex, "Base configuration file not found.");
            throw ex;
        }

        App.Logger.Info("Loading base configurations");
        string jsonString = File.ReadAllText(path);
        DataStore.Config config = JsonSerializer.Deserialize<DataStore.Config>(jsonString) ?? new();

        config.OriginalCanEdit = config.CanEdit;
        
        string baseDir = Environment.CurrentDirectory + "./Userdata/";
        config.CsvPath = config.CsvPath.Replace("$baseDir", baseDir);
        config.StylePath = config.StylePath.Replace("$baseDir", baseDir);
        config.PreviewPath = config.PreviewPath.Replace("$baseDir", baseDir);
        config.ThemePath = config.ThemePath.Replace("$baseDir", baseDir);

        // Conditional Formatting
        if (File.Exists(config.StylePath))
        {
            App.Logger.Info("Loading optional conditional formatting configurations");
            jsonString = File.ReadAllText(config.StylePath);
            config.Style = JsonSerializer.Deserialize<Model.ColumnStyle>(jsonString);
        }

        // Themeing
        if (File.Exists(config.ThemePath))
        {
            App.Logger.Info("Loading optional themeing configurations");
            using StreamReader streamReader = new(config.ThemePath);
            var dictionary = (ResourceDictionary)XamlReader.Load(streamReader.BaseStream);
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

        return config;
    }
}
