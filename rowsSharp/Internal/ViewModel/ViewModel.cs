using NLog;
using rowsSharp.View;
using System.ComponentModel;
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
    public readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public ConfigVM Config { get; }
    public CsvVM Csv { get; }
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
    public FilterVM Filter { get; }
    public PreviewVM Preview { get; }
    public HistoryVM History { get; }
    public EditVM Edit { get; }

    public RowsVM()
    {
        Logger.Debug("Begin VM construction");
        Config  = new(this);
        Csv     = new(this);
        CsvView = csvView = CollectionViewSource.GetDefaultView(Csv.Records);
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
        CsvView = CollectionViewSource.GetDefaultView(Csv.Records);
    }

    private DelegateCommand<CancelEventArgs>? exitCommand;
    public DelegateCommand<CancelEventArgs> ExitCommand => exitCommand ??= new(
        (e) =>
        {
            Logger.Info("Changes unsaved, asking for confirmation before exiting.");
            MessageBoxResult dialog = MessageBox.Show(
                "Save changes before exiting?",
                "RowsSharp",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            if (dialog == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (dialog == MessageBoxResult.Yes)
            {
                Edit.SaveCommand.Execute(this);
            }
        },
        (e) => Edit.IsDirtyEditor
    );
}
