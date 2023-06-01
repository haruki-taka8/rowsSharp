 using System.Windows.Controls;

namespace rowsSharp.ViewModel;

public class SettingsVM : NotifyPropertyChanged
{
    private readonly RootVM rootVM;
    private readonly UserControl previous;

    public SettingsVM(RootVM rootViewModel, UserControl previousUserControl)
    {
        rootVM = rootViewModel;
        previous = previousUserControl;
    }

    public DelegateCommand Return => new(() => rootVM.CurrentUserControl = previous);
}
