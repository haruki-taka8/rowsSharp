using RowsSharp.Domain;

namespace RowsSharp.ViewModel;

public class WelcomeViewModel
{
    public WelcomeViewModel(CommonViewModel commonViewModel)
    {
        CommonViewModel = commonViewModel;
    }

    public CommonViewModel CommonViewModel { get; private set; }

    public DelegateCommand OpenFile => new(() =>
    {
        string path = FileDialogHelper.RequestReadPath();

        CommonViewModel.Preferences.Csv.Path = path;
        CommonViewModel = new(CommonViewModel.Preferences);
    });

    public DelegateCommand NewFile => new(() => 
        CommonViewModel.CurrentSection = Section.Editor
    );

    public DelegateCommand OpenSettings => new(() =>
        CommonViewModel.CurrentSection = Section.Settings
    );
}
