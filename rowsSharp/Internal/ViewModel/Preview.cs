using rowsSharp.Model;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace rowsSharp.ViewModel
{
    public class PreviewVM : ViewModelBase
    {
        private readonly RowsVM viewModel;
        public PreviewVM(RowsVM inViewModel) => viewModel = inViewModel;

        private BitmapImage previewSource = new();
        public BitmapImage PreviewSource
        {
            get => previewSource;
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
                string replaceTo = CsvVM.GetField(activeRow, columnIndex);
                
                inString = inString.Replace(replaceFrom, replaceTo);
            }
            return inString; 
        }

        private DelegateCommand? updatePreviewCommand;
        public DelegateCommand UpdatePreviewCommand => updatePreviewCommand ??= new(
            () => UpdatePreview()
        );

        private void UpdatePreview()
        {
            if (!viewModel.Edit.SelectedItems.Any()) { return; }

            string path = viewModel.Config.PreviewPath;
            path = ExpandColumnNotation(path, viewModel.Edit.SelectedItems[0]);

            if (!File.Exists(path)) {
                viewModel.Logger.Warn("Failed to set preview image due to non-existent file @ {path}", path);
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

        private DelegateCommand? copyImageCommand;
        public DelegateCommand CopyImageCommand => copyImageCommand ??= new(
            () => CopyImage(),
            () => previewSource.UriSource is not null
        );

        private void CopyImage()
        {
            viewModel.Logger.Info("Copying preview image");
            Clipboard.SetImage(previewSource);
        }

        private DelegateCommand? copyStringCommand;
        public DelegateCommand CopyStringCommand => copyStringCommand ??= new(
            () => CopyString(),
            () => (viewModel.Edit.SelectedIndex != -1) && !string.IsNullOrWhiteSpace(viewModel.Config.CopyRowFormat)
        );

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
