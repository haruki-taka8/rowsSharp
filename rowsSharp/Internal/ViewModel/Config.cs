﻿using rowsSharp.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace rowsSharp.ViewModel
{
    public class ConfigVM : Config, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DelegateCommand? readWriteCommand;
        public DelegateCommand ReadWriteCommand => readWriteCommand ??=
            new(
                () => OnPropertyChanged(nameof(ReadWrite)),
                () => !OutputAlias
            );

        private DelegateCommand? outputAliasCommand;
        public DelegateCommand OutputAliasCommand => outputAliasCommand ??=
            new(() => SetReadWrite());

        private bool originalReadWrite;
        private void SetReadWrite()
        {
            // Make ReadWrite FALSE when OutputAlias is TRUE
            // Revert originalReadWrite when OutputAlias is FALSE
            if (OutputAlias) { originalReadWrite = ReadWrite; }
            ReadWrite = !OutputAlias && originalReadWrite;

            OnPropertyChanged(nameof(ReadWrite));
            viewModel.Filter.FilterCommand.Execute(this);
        }

        private DelegateCommand? insertSelectedCommand;
        public DelegateCommand InsertSelectedCommand => insertSelectedCommand ??=
            new(
                () => { }, // Do nothing
                () => ReadWrite
            );

        private readonly RowsVM viewModel;
        private const string InputPath = "./Userdata/Configurations/Configuration.json";
        private const string StylePath = "./Userdata/Configurations/Style.json";

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
            var jsonConfig = JsonSerializer.Deserialize<Config>(jsonString);

            if (jsonConfig is null) { throw new InvalidDataException(); }
            Config config = jsonConfig;

            CsvPath             = config.CsvPath;
            PreviewPath         = config.PreviewPath;
            InsertCount         = config.InsertCount;
            InsertSelectedCount = config.InsertSelectedCount;
            InputAlias          = config.InputAlias;
            OutputAlias         = config.OutputAlias;
            ReadWrite           = config.ReadWrite;
            IsTemplate          = config.IsTemplate;
            HasHeader           = config.HasHeader;
            FrozenColumn        = config.FrozenColumn;
            PreviewWidth        = config.PreviewWidth;
            FontFamily          = config.FontFamily;
            CopyRowFormat       = config.CopyRowFormat;

            string baseDir = Environment.CurrentDirectory + "./Userdata/";
            CsvPath = CsvPath.Replace("$baseDir", baseDir);
            PreviewPath = PreviewPath.Replace("$baseDir", baseDir);
            originalReadWrite = ReadWrite;

            // Styling
            if (File.Exists(StylePath))
            {
                viewModel.Logger.Info("Loading styling configurations");
                jsonString = File.ReadAllText(StylePath);
                var styleJson = JsonSerializer.Deserialize<StyleConfig>(jsonString);
                if (styleJson is not null) { Style = styleJson; }
                return;
            }

            viewModel.Logger.Info("No styling configurations found, proceeding with defaults");
        }
    }
}