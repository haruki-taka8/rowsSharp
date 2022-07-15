using System;
using rowsSharp.Model;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace rowsSharp.ViewModel
{
    public class ConfigVM : Config
    {
        private ICommand? setReadWriteCommand;
        public ICommand SetReadWriteCommand
        {
            get { return setReadWriteCommand ??= new CommandHandler(() => SetReadWrite(), () => true); }
        }

        private bool originalReadWrite;

        private void SetReadWrite()
        {
            if (OutputAlias)
            {
                originalReadWrite = ReadWrite;
                ReadWrite = false;
            }
            else
            {
                ReadWrite = originalReadWrite;
            }
        }

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
            Config configJson = JsonSerializer.Deserialize<Config>(jsonString);
            if (configJson is null) { throw new NullReferenceException(); }

            CsvPath             = configJson.CsvPath;
            PreviewPath         = configJson.PreviewPath;
            InsertCount         = configJson.InsertCount;
            InsertSelectedCount = configJson.InsertSelectedCount;
            InputAlias          = configJson.InputAlias;
            OutputAlias         = configJson.OutputAlias;
            ReadWrite           = configJson.ReadWrite;
            IsTemplate          = configJson.IsTemplate;
            HasHeader           = configJson.HasHeader;
            FrozenColumn        = configJson.FrozenColumn;
            PreviewWidth        = configJson.PreviewWidth;
            FontFamily          = configJson.FontFamily;
            CopyRowFormat       = configJson.CopyRowFormat;

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

                Style = styleJson is null ? new StyleConfig() : styleJson;
                return;
            }

            viewModel.Logger.Info("No styling configurations found, proceeding with defaults");
            Style = new StyleConfig();
        }
    }
}
