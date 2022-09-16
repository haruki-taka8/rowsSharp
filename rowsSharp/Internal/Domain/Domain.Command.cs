using rowsSharp.DataStore;
using rowsSharp.ViewModel;
using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.Domain;
internal class Command
{
    private readonly Edit edit;
    private readonly Filter filter;
    private readonly Preview preview;
    private readonly History history;
    private readonly Status status;
    private readonly Config config;
    private readonly OperationHistory operationHistory;
    private readonly bool hasCopyRowFormat;

    public Command(Edit edit, Filter filter, Preview preview, History history, Status status, Config config, OperationHistory operationHistory)
    {
        this.edit = edit;
        this.filter = filter;
        this.preview = preview;
        this.history = history;
        this.status = status;
        this.config = config;
        this.operationHistory = operationHistory;
        hasCopyRowFormat = !string.IsNullOrWhiteSpace(config.CopyRowFormat);
    }

    public DelegateCommand OutputAlias => new(() => { edit.OutputAliasEditing(); });
    public DelegateCommand BeginEdit => new(() => edit.BeginEdit());
    public DelegateCommand<DataGridCellEditEndingEventArgs> EndEdit => new((e) => edit.EndEdit(e));
    public DelegateCommand CanInsert => new(
        () => { }, // do nothing
        () => edit.CanInsertTopOrBottom()
    );
    public DelegateCommand InsertTop => new(
        () => edit.Insert(0),
        () => edit.CanInsertTopOrBottom()
    );
    public DelegateCommand InsertAbove => new(
        () => edit.Insert(status.SelectedIndex),
        () => edit.IsAnyRowSelected()
    );
    public DelegateCommand InsertBelow => new(
        () => edit.Insert(status.SelectedIndex + status.SelectedItems.Count),
        () => edit.IsAnyRowSelected()
    );
    public DelegateCommand InsertLast => new(
        () => edit.Insert(-1),
        () => edit.CanInsertTopOrBottom()
    );
    public DelegateCommand Remove => new(
        () => edit.Remove(),
        () => config.CanEdit && status.SelectedIndex != -1
    );
    public DelegateCommand Save => new(
        () => edit.Save(),
        () => config.CanEdit && status.IsDirtyEditor
    );

    public DelegateCommand Focus => new(() => filter.FocusFilter());
    public DelegateCommand Filter => new(() => filter.DoFilter());

    public DelegateCommand Undo => new(
        () => history.Undo(),
        () => config.CanEdit && operationHistory.UndoStack.Any()
    );
    public DelegateCommand Redo => new(
        () => history.Redo(),
        () => config.CanEdit && operationHistory.RedoStack.Any()
    );

    public DelegateCommand CopyImage => new(
        () => preview.CopyImage(),
        () => status.PreviewBitmap.UriSource is not null
    );
    public DelegateCommand CopyString => new(
        () => preview.CopyString(),
        () => hasCopyRowFormat && (status.SelectedIndex != -1)
    );
}
