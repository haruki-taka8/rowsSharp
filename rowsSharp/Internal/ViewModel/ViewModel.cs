using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NLog;
using rowsSharp.View;

namespace rowsSharp.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowsVM : ViewModelBase
    {
        public Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        public ConfigVM Config { get; set; }
        public CsvVM Csv { get; set; }

        private ICollectionView recordsView;
        public ICollectionView RecordsView
        {
            get => recordsView;
            set
            {
                recordsView = value;
                OnPropertyChanged(nameof(RecordsView));
            }
        }

        public FilterVM Filter { get; set; }
        public PreviewVM Preview { get; set; }
        public HistoryVM History { get; set; }
        public EditVM Edit { get; set; }

        public RowsVM()
        {
            Logger.Debug("Begin VM construction");
            Config  = new(this);
            Csv     = new(this, Config.CsvPath);
            recordsView = CollectionViewSource.GetDefaultView(Csv.Records);
            Filter  = new(this);
            Preview = new(this);
            History = new(this);
            Edit    = new(this);
            Logger.Debug("End VM construction");

            // Open the file creation dialog
            if (Csv.Records.Any()) { return; }
            new NewFileWindow(this).ShowDialog();
            Csv = new(this, Config.CsvPath);
            recordsView = CollectionViewSource.GetDefaultView(Csv.Records);

            if (Csv.Records.Any()) { return; }
            FileNotFoundException notFoundException = new(Config.CsvPath);
            Logger.Fatal(notFoundException, "CSV file still not found. Bailing out.");
            throw notFoundException;
        }

        private DelegateCommand<CancelEventArgs>? exitCommand;
        public DelegateCommand<CancelEventArgs> ExitCommand => exitCommand ??= new DelegateCommand<CancelEventArgs>(
            (e) =>
            {
                Logger.Info("Trigger exit");
                MessageBoxResult dialog = MessageBoxResult.No;
                if (Edit.IsDirtyEditor)
                {
                    Logger.Info("Changes unsaved, asking for confirmation before exiting.");
                    dialog = MessageBox.Show(
                        "Save changes before exiting?",
                        "RowsSharp",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question
                    );
                }

                if (dialog == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (dialog == MessageBoxResult.Yes)
                {
                    Edit.SaveCommand.Execute(this);
                }
            },
            (e) => true
        );
    }
}
