using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.ViewModel;

public class EditVM : ViewModelBase
{
    private readonly RowsVM viewModel;
    public EditVM(RowsVM inViewModel)
    {
        viewModel = inViewModel;
    }

    private bool isDirtyEditor;
    public bool IsDirtyEditor
    {
        get => isDirtyEditor;
        set
        {
            isDirtyEditor = value;
            OnPropertyChanged(nameof(IsDirtyEditor));
        }
    }

    private bool isEditing;
    public bool IsEditing
    {
        get => isEditing;
        set
        {
            isEditing = value;
            OnPropertyChanged(nameof(IsEditing));
        }
    }

    private bool isInsertExpanded;
    public bool IsInsertExpanded
    {
        get => isInsertExpanded;
        set
        {
            isInsertExpanded = value;
            OnPropertyChanged(nameof(IsInsertExpanded));
        }
    }

    private List<Record> selectedItems = new();
    public List<Record> SelectedItems
    {
        get => selectedItems;
        set
        {
            Record? firstOfOld = selectedItems.FirstOrDefault();
            selectedItems = value;
            OnPropertyChanged(nameof(SelectedItems));

            if (firstOfOld == value.FirstOrDefault()) { return; }
            viewModel.Preview.UpdatePreviewCommand.Execute(this);
        }
    }

    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            selectedIndex = value;
            OnPropertyChanged(nameof(SelectedIndex));
        }
    }

    private DelegateCommand? beginEditCommand;
    public DelegateCommand BeginEditCommand => beginEditCommand ??=
        new(() => IsEditing = true);

    private bool CanInsertTopOrBottom() =>
        viewModel.Config.CanEdit &&
        (
            (viewModel.Config.InsertSelectedCount && SelectedItems.Any()) ||
            (!viewModel.Config.InsertSelectedCount)
        );

    private bool IsAnyRowSelected() =>
        viewModel.Config.CanEdit && SelectedIndex != -1;

    private DelegateCommand? canInsertCommand;
    public DelegateCommand CanInsertCommand => canInsertCommand ??=
        new(
            () => { }, // do nothing
            () => CanInsertTopOrBottom()
        );

    private DelegateCommand? insertTopCommand;
    public DelegateCommand InsertTopCommand => insertTopCommand ??= new(
        () => Insert(0),
        () => CanInsertTopOrBottom()
    );

    private DelegateCommand? insertAboveCommand;
    public DelegateCommand InsertAboveCommand => insertAboveCommand ??= new(
        () => Insert(SelectedIndex),
        () => IsAnyRowSelected()
    );

    private DelegateCommand? insertBelowCommand;
    public DelegateCommand InsertBelowCommand => insertBelowCommand ??= new(
        () => Insert(SelectedIndex + SelectedItems.Count),
        () => IsAnyRowSelected()
    );

    private DelegateCommand? insertLastCommand;
    public DelegateCommand InsertLastCommand => insertLastCommand ??= new(
        () => Insert(viewModel.Csv.Records.Count),
        () => CanInsertTopOrBottom()
    );

    private DelegateCommand? removeCommand;
    public DelegateCommand RemoveCommand => removeCommand ??= new(
        () =>
        {
            viewModel.Logger.Info("Removing rows (x{Count})", SelectedItems.Count);
            foreach (Record item in SelectedItems)
            {
                viewModel.History.AddOperation(
                    OperationEnum.Remove,
                    viewModel.Csv.Records.IndexOf(item),
                    item
                );
                viewModel.Csv.Records.Remove(item);
            }
            viewModel.History.CommitOperation();
        },
        () => viewModel.Config.CanEdit && SelectedIndex != -1
    );

    private DelegateCommand<DataGridCellEditEndingEventArgs>? endEditCommand;
    public DelegateCommand<DataGridCellEditEndingEventArgs> EndEditCommand => endEditCommand ??= new(
        (e) =>
        {
            Record record = (Record)e.Row.Item;
            int columnIndex = viewModel.Csv.Headers.IndexOf(e.Column.Header.ToString()!);
            string oldString = CsvVM.GetField(record, columnIndex);

            if (((TextBox)e.EditingElement).Text == oldString) { return; }
            viewModel.History.AddOperation(
                OperationEnum.Inline,
                viewModel.Csv.Records.IndexOf(record),
                viewModel.Csv.DeepCopy(record)
            );
            viewModel.History.CommitOperation();
        }
    );

    public void Insert(int at)
    {
        int count = viewModel.Config.InsertSelectedCount
            ? selectedItems.Count
            : viewModel.Config.InsertCount;

        viewModel.Logger.Info("Inserting CSV (@{At} x{Count}, Template: {Template})", at, count, viewModel.Config.UseInsertTemplate);
        IsInsertExpanded = false;

        DateTime now = DateTime.Now;
        Record templatedRow = new();

        // Templating. Expand static <[DdTt]> fields beforehand.
        if (viewModel.Config.UseInsertTemplate)
        {
            foreach (KeyValuePair<string,string> keyValuePair in viewModel.Config.Style.Template)
            {
                int columnIndex = viewModel.Csv.Headers.IndexOf(keyValuePair.Key);
                CsvVM.SetField(
                    templatedRow,
                    columnIndex,
                    keyValuePair.Value
                        .Replace("<D>", now.ToString("yyyyMMdd"))
                        .Replace("<d>", now.ToString("yyyy-MM-dd"))
                        .Replace("<T>", now.ToString("HHmmss"))
                        .Replace("<t>", now.ToString("HH:mm:ss"))
                );
            }
        }

        // Insert
        for (int i = 0; i < count; i++)
        {
            Record thisRow = viewModel.Csv.DeepCopy(templatedRow);
            for (int j = 0; j < viewModel.Csv.Headers.Count; j++)
            {
                CsvVM.SetField(
                    thisRow,
                    j,
                    CsvVM.GetField(thisRow, j)
                        .Replace("<#>", i.ToString())
                        .Replace("<!#>", (count - i - 1).ToString())
                );
            }

            viewModel.Csv.Records.Insert(at + i, thisRow);
            viewModel.History.AddOperation(OperationEnum.Insert, at + i, thisRow);
        }
        viewModel.History.CommitOperation();
        SelectedIndex = at;
    }

    private DelegateCommand? saveCommand;
    public DelegateCommand SaveCommand => saveCommand ??= new(
        () =>
        {
            viewModel.Logger.Info("Saving");
            using StreamWriter writer = new(viewModel.Config.CsvPath);
        
            string fullHeader = string.Join(
                ",",
                viewModel.Csv.Headers.Select(m => "\"" + m.Replace("\"", "\"\"") + "\"")
            );
            writer.WriteLine(fullHeader);
        
            foreach (Record record in viewModel.Csv.Records)
            {
                string toOutput = viewModel.Csv.ConcatenateFields(record);
                writer.WriteLine(toOutput);
            }
        
            IsDirtyEditor = false;
        },
        () => viewModel.Config.CanEdit && isDirtyEditor
    );
}    
