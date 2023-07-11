using Microsoft.Win32;
using rowsSharp.View;

namespace rowsSharp.ViewModel;

public class WelcomeVM : NotifyPropertyChanged
{
    private readonly RootVM rootVM;

    public WelcomeVM(RootVM rootViewModel)
    {
        rootVM = rootViewModel;
    }

    private static string RequestFilePath()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Comma-separated values (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv"
        };

        dialog.ShowDialog();

        return dialog.FileName;
    }

    public DelegateCommand OpenFile => new(() =>
    {
        rootVM.Preferences.Csv.Path = RequestFilePath();
        rootVM.Initialize();
    });

    public DelegateCommand NewFile => new(
        () => rootVM.Initialize(false)
    );

    public DelegateCommand OpenPreferences => new(() =>
    {
        rootVM.SettingsVM = new(rootVM, rootVM.CurrentUserControl);
        rootVM.CurrentUserControl = new Settings(); 
    });
}
