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

    private bool canEdit;
    public new bool CanEdit
    {
        get => canEdit;
        set
        {
            canEdit = value;
            OnPropertyChanged(nameof(CanEdit));
        }
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
    });

    private readonly string baseDir = Environment.CurrentDirectory + "./Userdata/";
    private const string ConfigPath = "./Userdata/Configurations/Configuration.json";

    public ConfigVM(RowsVM viewModel)
    {
        // General configuration
        if (!File.Exists(ConfigPath))
        {
            FileNotFoundException ex = new(ConfigPath);
            viewModel.Logger.Fatal(ex, "Base configuration file not found.");
            throw ex;
        }

        viewModel.Logger.Info("Loading base configurations");
        string jsonString = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<Config>(jsonString) ?? new();

        foreach (var configItem in config.GetType().GetProperties())
        {
            configItem.SetValue(this, configItem.GetValue(config));
        }

        CanEdit     = config.CanEdit; // override
        originalCanEdit = CanEdit;
        CsvPath     = CsvPath.Replace("$baseDir", baseDir);
        StylePath   = StylePath.Replace("$baseDir", baseDir);
        PreviewPath = PreviewPath.Replace("$baseDir", baseDir);
        ThemePath   = ThemePath.Replace("$baseDir", baseDir);

        // Conditional Formatting
        if (File.Exists(StylePath))
        {
            viewModel.Logger.Info("Loading optional conditional formatting configurations");
            jsonString = File.ReadAllText(StylePath);
            Style = JsonSerializer.Deserialize<StyleConfig>(jsonString);
        }

        // Themeing
        if (!File.Exists(ThemePath)) { return; }

        viewModel.Logger.Info("Loading optional themeing configurations");
        using StreamReader streamReader = new(ThemePath);
        var dictionary = (ResourceDictionary)XamlReader.Load(streamReader.BaseStream);
        Application.Current.Resources.MergedDictionaries.Add(dictionary);
    }
}
