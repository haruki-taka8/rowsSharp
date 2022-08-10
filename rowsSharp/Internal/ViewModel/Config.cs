using rowsSharp.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Markup;

namespace rowsSharp.ViewModel;

public class ConfigVM : Config, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool originalCanEdit;
    private DelegateCommand? outputAliasCommand;
    public DelegateCommand OutputAliasCommand => outputAliasCommand ??=
        new(() =>
        {
            // Make ReadWrite FALSE when OutputAlias is TRUE
            // Revert originalReadWrite when OutputAlias is FALSE
            if (UseOutputAlias) { originalCanEdit = CanEdit; }
            CanEdit = !UseOutputAlias && originalCanEdit;

            OnPropertyChanged(nameof(UseOutputAlias));
            OnPropertyChanged(nameof(CanEdit));
        });

    private readonly string baseDir = Environment.CurrentDirectory + "./Userdata/";
    private const string InputPath = "./Userdata/Configurations/Configuration.json";

    public ConfigVM(RowsVM viewModel)
    {
        // General configuration
        if (!File.Exists(InputPath))
        {
            FileNotFoundException ex = new(InputPath);
            viewModel.Logger.Fatal(ex, "Base configuration file not found.");
            throw ex;
        }

        viewModel.Logger.Info("Loading base configurations");
        string jsonString = File.ReadAllText(InputPath);
        Config config = JsonSerializer.Deserialize<Config>(jsonString) ?? new();

        foreach (var configItem in config.GetType().GetProperties())
        {
            configItem.SetValue(this, configItem.GetValue(config));
        }

        CsvPath     = CsvPath.Replace("$baseDir", baseDir);
        StylePath   = StylePath.Replace("$baseDir", baseDir);
        PreviewPath = PreviewPath.Replace("$baseDir", baseDir);
        ThemePath   = ThemePath.Replace("$baseDir", baseDir);
        originalCanEdit = CanEdit;

        // Conditional Formatting
        if (!File.Exists(StylePath)) { return; }

        viewModel.Logger.Info("Loading optional conditional formatting configurations");
        jsonString = File.ReadAllText(StylePath);
        Style = JsonSerializer.Deserialize<StyleConfig>(jsonString);

        // Themeing
        if (!File.Exists(ThemePath)) { return; }

        viewModel.Logger.Info("Loading optional themeing configurations");
        StreamReader streamReader = new(ThemePath);
        ResourceDictionary dictionary = (ResourceDictionary)XamlReader.Load(streamReader.BaseStream);
        Application.Current.Resources.MergedDictionaries.Add(dictionary);
    }
}
