using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ObservableTable.Core;
using RowsSharp.Domain;
using RowsSharp.Model;

namespace RowsSharp.ViewModel;

public class CommonViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// Version of RowsSharp, expressed in YY.MM, for user-friendly View consumption
    /// </summary>
    public static string Version { get; } =
        string.Format("{0:00}.{1:00}", CommonModel.Version.Major, CommonModel.Version.Minor);

    /// <summary>
    /// Build and revision of RowsSharp, expressed in Build.Revision, for user-friendly View consumption.
    /// Build and revision numbers are based on compilation date and time.
    /// </summary>
    public static string Build { get; } =
        string.Format("{0}.{1}", CommonModel.Version.Build, CommonModel.Version.Revision);

    /// <summary>
    /// Denotes the previous section. Used for the Settings View.
    /// </summary>
    public Section PreviousSection { get; private set; }

    private Section currentSection = Section.Splash;
    /// <summary>
    /// Denotes the current section. The actual mapping to an UserControl is handled by View code.
    /// </summary>
    public Section CurrentSection
    {
        get => currentSection;
        internal set
        {
            PreviousSection = currentSection;
            SetField(ref currentSection, value);
        }
    }

    /// <summary>
    /// Reference to the CommonModel
    /// </summary>
    private readonly CommonModel model = new();

    /// <summary>
    /// Reference to the Preferences object
    /// </summary>
    public Preferences Preferences
    {
        get => model.Preferences;
        private set => model.Preferences = value;
    }

    /// <summary>
    /// Filename of the CSV in editor
    /// </summary>
    private string CsvName => Preferences.Csv.Path.Split('/', '\\')[^1];

    /// <summary>
    /// Reference to the ObservableTable object
    /// </summary>
    public ObservableTable<string> Table
    {
        get => model.Table;
        private set => model.Table = value;
    }

    private bool isEditorDirty;
    /// <summary>
    /// Whether the ObservableTable has been modified or not.
    /// Set this value to false upon saving to a file.
    /// </summary>
    public bool IsEditorDirty
    {
        get => isEditorDirty;
        internal set
        {
            SetField(ref isEditorDirty, value);
            OnPropertyChanged(nameof(Title));
        }
    }

    /// <summary>
    /// View-friendly presentation of IsEditorDirty.
    /// </summary>
    private string EditorSaveStatus => IsEditorDirty ? "*" : "";

    /// <summary>
    /// MainWindow title
    /// </summary>
    public string Title => $"{EditorSaveStatus}{CsvName} - RowsSharp";

    private const string ConfigurationPath =
        "./Userdata/Configurations/Configuration.json";

    public CommonViewModel()
    {
        Preferences = PreferencesReader.Import(ConfigurationPath);
    }

    public async void InitializeAsync()
    {
        // Don't block the UI thread
        await Task.Run(() => Initialize());
    }

    private void Initialize()
    {
        if (Preferences.UserInterface.ShowWelcomeOnStartup)
        {
            CurrentSection = Section.Welcome;
            return;
        }

        Table = CsvFile.Import(Preferences.Csv.Path, Preferences.Csv.HasHeader);

        CurrentSection = File.Exists(Preferences.Csv.Path)
            ? Section.Editor
            : Section.Welcome;
    }

    public DelegateCommand<CancelEventArgs> Exit => new(
        (e) =>
        {
            if (!IsEditorDirty) { return; }

            MessageBoxResult dialog = MessageBox.Show(
                "Save changes before exiting?",
                "RowsSharp",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            e.Cancel = dialog == MessageBoxResult.Cancel;

            if (dialog == MessageBoxResult.Yes)
            {
                CsvFile.Export(Preferences.Csv.Path, Table, Preferences.Csv.HasHeader);
            }
        }
    );
}
