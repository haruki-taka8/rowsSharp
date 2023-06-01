using rowsSharp.Domain;
using rowsSharp.Model;
using System.Reflection;
using System;
using System.Windows.Controls;
using rowsSharp.View;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using ObservableTable.Core;
using System.ComponentModel;

namespace rowsSharp.ViewModel;

public class RootVM : NotifyPropertyChanged
{
    private const string ConfigPath = "./Userdata/Configurations/Configuration.json";

    // Common
    private UserControl currentUserControl = default!;
    public UserControl CurrentUserControl
    {
        get => currentUserControl;
        set => SetField(ref currentUserControl, value);
    }

    public static Version Version => Assembly.GetExecutingAssembly().GetName().Version!;
    public static string VersionShort => string.Format("{0}.{1:00}", Version.Major, Version.Minor);

    // Models
    public Preferences Preferences { get; init; }
    public ObservableTable<string> Table { get; set; } = new();

    // ViewModels
    public SplashVM? SplashVM { get; set; }
    public WelcomeVM? WelcomeVM { get; set; }
    public EditorVM? EditorVM { get; set; }
    public SettingsVM? SettingsVM { get; set; }

    public RootVM()
    {
        App.Logger.Info("Building DataContext");

        Preferences = PreferencesReader.Import(ConfigPath);
        if (Preferences.Theme is not null)
        {
            Application.Current.Resources.MergedDictionaries.Add(Preferences.Theme);
        }

        Initialize();
    }

    internal async void Initialize(bool hasFilePath = true)
    {
        CancellationTokenSource token = new();
        SplashVM = new(token.Token);
        CurrentUserControl = new Splash();

        bool isTableValid = await Task.Run(() => BackgroundTask(hasFilePath));
        token.Cancel();

        // Switching to the Editor must be done in UI thread
        // because an ICollectionView cannot be accessed from a different thread.
        if (isTableValid)
        {
            EditorVM = new(this);
            CurrentUserControl = new View.Editor();
            return;
        }
        
        WelcomeVM = new(this);
        CurrentUserControl = new Welcome();
    }

    internal bool BackgroundTask(bool hasFilePath = true)
    {
        if (!hasFilePath) { return true; }

        Table = CsvFile.Import(Preferences.CsvPath, Preferences.HasHeader);
        return Table.Headers.Count != 0;
    }

    public DelegateCommand<CancelEventArgs> Exit => new(
        (e) =>
        {
            if (!EditorVM?.Edit.IsEditorDirty ?? true) { return; }

            MessageBoxResult action = MessageBox.Show(
                "Save changes before exiting?",
                "RowsSharp",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            if (action == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (action == MessageBoxResult.Yes)
            {
                EditorVM?.Edit.Save.Execute(this);
            }
        }
    );
}
