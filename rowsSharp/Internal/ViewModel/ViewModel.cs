using NLog;
using rowsSharp.View;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace rowsSharp.ViewModel;

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
    public string Version { get; }
        = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public ConfigVM Config { get; init; }
    public CsvVM Csv { get; init; }
    private ICollectionView csvView;
    public ICollectionView CsvView
    {
        get => csvView;
        set
        {
            csvView = value;
            OnPropertyChanged(nameof(CsvView));
        }
    }
    public FilterVM Filter { get; init; }
    public PreviewVM Preview { get; init; }
    public HistoryVM History { get; init; }
    public EditVM Edit { get; init; }

    public RowsVM()
    {
        Logger.Debug("Begin VM construction");
        Config  = new(this);
        Csv     = new(this);
        csvView = CollectionViewSource.GetDefaultView(Csv.Records);
        Filter  = new(this);
        Preview = new(this);
        History = new(this);
        Edit    = new(this);
        Logger.Debug("End VM construction");

        // Open the file creation dialog
        if (Csv.Records.Any()) { return; }
        Logger.Warn("CSV file not found. Starting creation wizard.");
        new NewFileWindow(Config).ShowDialog();
        Csv = new(this);
        csvView = CollectionViewSource.GetDefaultView(Csv.Records);

        if (Csv.Records.Any()) { return; }
        FileNotFoundException ex = new(Config.CsvPath);
        Logger.Fatal(ex, "CSV file still not found. Bailing out.");
        throw ex;
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
        }
    );
}
