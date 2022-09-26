using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.ViewModel;
internal class Command
{
    private DataContext DataContext { get; init; }

    internal Command (DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DelegateCommand OutputAlias => new(() => { DataContext.Edit.OutputAliasEditing(); });

    public DelegateCommand BeginEdit => new(() => DataContext.Edit.BeginEdit());
    public DelegateCommand<DataGridCellEditEndingEventArgs> EndEdit => new((e) => DataContext.Edit.EndEdit(e));

    public DelegateCommand CanInsert => new(
        () => { }, // do nothing
        () => DataContext.Edit.CanInsertTopOrBottom()
    );
    public DelegateCommand InsertTop => new(
        () => DataContext.Edit.Insert(0),
        () => DataContext.Edit.CanInsertTopOrBottom()
    );
    public DelegateCommand InsertAbove => new(
        () => DataContext.Edit.Insert(DataContext.Status.SelectedIndex),
        () => DataContext.Edit.IsAnyRowSelected()
    );
    public DelegateCommand InsertBelow => new(
        () => DataContext.Edit.Insert(DataContext.Status.SelectedIndex + DataContext.Status.SelectedItems.Count),
        () => DataContext.Edit.IsAnyRowSelected()
    );
    public DelegateCommand InsertLast => new(
        () => DataContext.Edit.Insert(-1),
        () => DataContext.Edit.CanInsertTopOrBottom()
    );
    public DelegateCommand Remove => new(
        () => DataContext.Edit.Remove(),
        () => DataContext.Config.CanEdit && DataContext.Status.SelectedIndex != -1
    );
    public DelegateCommand Save => new(
        () => DataContext.Edit.Save(),
        () => DataContext.Config.CanEdit && DataContext.Status.IsDirtyEditor
    );

    public DelegateCommand Focus => new(() => DataContext.Filter.FocusFilter());
    public DelegateCommand Filter => new(() => DataContext.Filter.DoFilter());

    public DelegateCommand Undo => new(
        () => DataContext.History.Undo(),
        () => DataContext.Config.CanEdit && DataContext.OperationHistory.UndoStack.Any()
    );
    public DelegateCommand Redo => new(
        () => DataContext.History.Redo(),
        () => DataContext.Config.CanEdit && DataContext.OperationHistory.RedoStack.Any()
    );

    public DelegateCommand CopyImage => new(
        () => DataContext.Preview.CopyImage(),
        () => DataContext.Status.PreviewBitmap.UriSource is not null
    );
    public DelegateCommand CopyString => new(
        () => DataContext.Preview.CopyString(),
        () => (!string.IsNullOrWhiteSpace(DataContext.Config.CopyRowFormat)) && (DataContext.Status.SelectedIndex != -1)
    );
}
