using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using rowsSharp.Model;
using System.Windows.Input;
using System.Windows;
using System.Linq;

namespace rowsSharp.ViewModel
{
    public class PreviewVM : ViewModelBase
    {
        private readonly RowsVM viewModel;

        public PreviewVM(RowsVM inViewModel)
        {
            viewModel = inViewModel;
        }

        private BitmapImage previewSource = new();
        public BitmapImage PreviewSource
        {
            get { return previewSource; }
            set
            {
                previewSource = value;
                OnPropertyChanged(nameof(PreviewSource));
            }
        }

        private string ExpandColumnNotation (string inString, CsvRecord activeRow)
        {
            if (activeRow is null) { return string.Empty; }

            MatchCollection matches = Regex.Matches(inString, @"(?<=<)(.+?)(?=>)");
            foreach (Match match in matches)
            {
                int columnIndex = viewModel.Csv.Headers.IndexOf(match.Value);
                if (columnIndex == -1) { return string.Empty; }

                string replaceFrom = string.Format("<{0}>", match.Value);
                string replaceTo = activeRow.GetType().GetProperty("Column" + columnIndex).GetValue(activeRow, null).ToString()!;
                
                inString = inString.Replace(replaceFrom, replaceTo);
            }
            return inString; 
        }

        private ICommand? updatePreviewCommand;
        public ICommand UpdatePreviewCommand
        {
            get { return updatePreviewCommand ??= new CommandHandler(() => UpdatePreview(), () => true); }
        }

        private void UpdatePreview()
        {
            string path = viewModel.Config.PreviewPath;
            if (!viewModel.Edit.SelectedItems.Any()) { return; }

            path = ExpandColumnNotation(path, viewModel.Edit.SelectedItems[0]);

            if (!File.Exists(path)) {
                viewModel.Logger.Warn("Failed to set preview image due to non-existent file @ {path}", path);
                previewSource = new();
                PreviewSource = new();
                return;
            }

            // Don't permanently lock the image
            viewModel.Logger.Info("Setting preview image to {path}", path);
            previewSource = new();
            previewSource.BeginInit();
            previewSource.UriSource = new Uri(path);
            previewSource.CacheOption = BitmapCacheOption.OnLoad;
            previewSource.EndInit();

            // INPC
            PreviewSource = previewSource;
        }

        private ICommand? _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get { return _copyImageCommand ??= new CommandHandler(() => CopyImage(), () => previewSource.UriSource != null); }
        }

        private void CopyImage()
        {
            viewModel.Logger.Info("Copying preview image");
            Clipboard.SetImage(previewSource);
        }

        private ICommand? _copyStringCommand;
        public ICommand CopyStringCommand
        {
            get {
                return _copyStringCommand ??= new CommandHandler(
                    () => CopyString(),
                    () => (viewModel.Edit.SelectedIndex != -1) && !string.IsNullOrWhiteSpace(viewModel.Config.CopyRowFormat)
                );
            }
        }

        private void CopyString()
        {
            if (!viewModel.Edit.SelectedItems.Any()) { return; }

            viewModel.Logger.Info("Copying row string");
            string copyRowFormat = viewModel.Config.CopyRowFormat;
            copyRowFormat = ExpandColumnNotation(copyRowFormat, viewModel.Edit.SelectedItems[0]);
            Clipboard.SetText(copyRowFormat);
        }
    }
}
