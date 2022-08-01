using rowsSharp.Model;
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

        private DelegateCommand? canEditCommand;
        public DelegateCommand CanEditCommand => canEditCommand ??=
            new(
                () => OnPropertyChanged(nameof(CanEdit)),
                () => !UseOutputAlias
            );

        private DelegateCommand? outputAliasCommand;
        public DelegateCommand OutputAliasCommand => outputAliasCommand ??=
            new(() => SetCanEdit());

        private bool originalCanEdit;
        private void SetCanEdit()
        {
            // Make ReadWrite FALSE when OutputAlias is TRUE
            // Revert originalReadWrite when OutputAlias is FALSE
            if (UseOutputAlias) { originalCanEdit = CanEdit; }
            CanEdit = !UseOutputAlias && originalCanEdit;

            OnPropertyChanged(nameof(UseOutputAlias));
            OnPropertyChanged(nameof(CanEdit));
            viewModel.Filter.FilterCommand.Execute(this);
        }

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
            CsvPath = CsvPath.Replace("$baseDir", baseDir);
            StylePath = StylePath.Replace("$baseDir", baseDir);
            PreviewPath = PreviewPath.Replace("$baseDir", baseDir);
            originalCanEdit = CanEdit;

            // Styling
            if (!File.Exists(StylePath))
            {
                viewModel.Logger.Info("No styling configurations found, proceeding with defaults");
                return;
            }

            viewModel.Logger.Info("Loading styling configurations");
            jsonString = File.ReadAllText(StylePath);
            StyleConfig? styleJson = JsonSerializer.Deserialize<StyleConfig>(jsonString);
            if (styleJson is not null) { Style = styleJson; }
        }
    }
}
