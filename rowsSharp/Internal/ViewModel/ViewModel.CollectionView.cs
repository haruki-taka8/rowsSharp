using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace rowsSharp.ViewModel;
internal class RecordsView : INPC
{
    private ICollectionView collectionView;
    public ICollectionView CollectionView
    {
        get => collectionView;
        set => SetField(ref collectionView, value);
    }

    internal RecordsView(ObservableCollection<Model.Record> csv)
    {
        collectionView = CollectionViewSource.GetDefaultView(csv);
    }
}
