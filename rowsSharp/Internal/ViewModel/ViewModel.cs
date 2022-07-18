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
        public Logger Logger = LogManager.GetCurrentClassLogger();

        private ConfigVM config;
        public ConfigVM Config
        {
            get { return config; }
            set
            {
                config = value;
                OnPropertyChanged(nameof(Config));
            }
        }

        private CsvVM csv;
        public CsvVM Csv
        {
            get { return csv; }
            set
            {
                csv = value; 
                OnPropertyChanged(nameof(Csv));
            }
        }

        private ICollectionView recordsView;
        public ICollectionView RecordsView
        {
            get { return recordsView; }
            set
            {
                recordsView = value;
                OnPropertyChanged(nameof(RecordsView));
            }
        }

        private FilterVM filter;
        public FilterVM Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                OnPropertyChanged(nameof(Filter));
            }
        }

        private PreviewVM preview;
        public PreviewVM Preview
        {
            get { return preview; }
            set
            {
                preview = value;
                OnPropertyChanged(nameof(Preview));
            }
        }

        private HistoryVM history;
        public HistoryVM History
        {
            get { return history; }
            set
            {
                history = value;
                OnPropertyChanged(nameof(History));
            }
        }

        private EditVM edit;
        public EditVM Edit
        {
            get { return edit; }
            set
            {
                edit = value;
                OnPropertyChanged(nameof(Edit));
            }
        }

        public RowsVM()
        {
            Logger.Debug("Begin VM construction");
            config  = new(this);
            csv     = new(this, config.CsvPath);
            recordsView = CollectionViewSource.GetDefaultView(csv.Records);
            filter  = new(this);
            preview = new(this);
            history = new(this);
            edit    = new(this);
            Logger.Debug("End VM construction");

            // Open the file creation dialog
            if (csv.Records.Any()) { return; }
            new NewFileWindow(this).ShowDialog();
            csv = new(this, config.CsvPath);
            recordsView = CollectionViewSource.GetDefaultView(csv.Records);

            if (csv.Records.Any()) { return; }
            FileNotFoundException notFoundException = new(config.CsvPath);
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

                if (dialog == MessageBoxResult.Yes)
                {
                    Edit.SaveCommand.Execute(this);
                }
            },
            (e) => true
        );
    }
}
