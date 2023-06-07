using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace rowsSharp;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    private protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        field = value;
        OnPropertyChanged(propertyName);
    }
}