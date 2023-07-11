using rowsSharp.Domain;
using rowsSharp.Model;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;
using ObservableTable.Core;

namespace rowsSharp.ViewModel.Editor;

public class Preview : NotifyPropertyChanged
{
    private readonly RootVM rootVM;
    private Preferences Preferences => rootVM.Preferences;
    private ObservableTable<string> Table => rootVM.Table;

    public Preview(RootVM rootViewModel)
    {
        rootVM = rootViewModel;
    }

    public DelegateCommand<IList<string?>> ChangePreview => new(
        (item) =>
        {
            string path = Preferences.Preview.Path;
            path = ColumnNotation.Expand(path, Table.Headers, item);
            
            Bitmap = Domain.Preview.FromPath(path);
        }
     );

    public DelegateCommand CopyPreview => new(
        () => Clipboard.SetImage(bitmap),
        () => Bitmap is not null
    );

    private BitmapImage? bitmap;
    public BitmapImage? Bitmap
    {
        get => bitmap;
        set => SetField(ref bitmap, value);
    }
}
