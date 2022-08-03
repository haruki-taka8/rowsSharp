﻿using rowsSharp.Model;
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

    private DelegateCommand? canEditCommand;
    public DelegateCommand CanEditCommand => canEditCommand ??=
        new(
            () => OnPropertyChanged(nameof(CanEdit)),
            () => !UseOutputAlias
        );

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
            viewModel.Filter.FilterCommand.Execute(this);
        });

    private readonly RowsVM viewModel;
    private const string InputPath = "./Userdata/Configurations/Configuration.json";

    public ConfigVM (RowsVM inViewModel)
    {
        viewModel = inViewModel;

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

        string baseDir = Environment.CurrentDirectory + "./Userdata/";
        CsvPath     = CsvPath.Replace("$baseDir", baseDir);
        StylePath   = StylePath.Replace("$baseDir", baseDir);
        PreviewPath = PreviewPath.Replace("$baseDir", baseDir);
        ThemePath   = ThemePath.Replace("$baseDir", baseDir);
        originalCanEdit = CanEdit;

        // Conditional Formatting
        if (!File.Exists(StylePath))
        {
            viewModel.Logger.Info("No conditional formatting configurations found, proceeding with defaults");
            return;
        }

        viewModel.Logger.Info("Loading conditional formatting configurations");
        jsonString = File.ReadAllText(StylePath);
        StyleConfig? styleJson = JsonSerializer.Deserialize<StyleConfig>(jsonString);
        if (styleJson is not null) { Style = styleJson; }

        // Themeing
        if (!File.Exists(ThemePath)) { return; }

        viewModel.Logger.Info("Loading themeing configurations");
        Application app = Application.Current;
        StreamReader streamReader = new(ThemePath);
        ResourceDictionary dictionary = (ResourceDictionary)XamlReader.Load(streamReader.BaseStream);
        app.Resources.MergedDictionaries.Add(dictionary);
    }
}
