using RowsSharp.Model;

namespace RowsSharp.ViewModel;

public class SettingsViewModel
{
    public SettingsViewModel(CommonViewModel commonViewModel)
    {
        this.commonViewModel = commonViewModel;
    }

    private readonly CommonViewModel commonViewModel;

    public Preferences Preferences => commonViewModel.Preferences;

    public DelegateCommand Return => new(() =>
        commonViewModel.CurrentSection = commonViewModel.PreviousSection
    );
}
