using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.ViewModel
{
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

        private List<CsvRecord> selectedItems = new();
        public List<CsvRecord> SelectedItems
        {
            get => selectedItems;
            set
            {
                List<CsvRecord> old = selectedItems;
                selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));

                if (old.Any() && value.Any() && old[0] == value[0]) { return; }
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
            new(
                () => viewModel.Edit.IsEditing = true,
                () => viewModel.Config.ReadWrite
            );

        private DelegateCommand<object>? commitEditCommand;
        public DelegateCommand<object> CommitEditCommand => commitEditCommand ??=
            new(
                (s) => ((DataGrid)s).CommitEdit(),
                (s) => viewModel.Config.ReadWrite
            );
        private bool CanInsertTopOrBottom() => 
            viewModel.Config.ReadWrite &&
            (
                (viewModel.Config.InsertSelectedCount && SelectedItems.Any()) ||
                (viewModel.Config.InsertSelectedCount == false)
            );

        private bool IsAnyRowSelected() => 
            viewModel.Config.ReadWrite &&
            !viewModel.RecordsView.IsEmpty &&
            SelectedIndex != -1;

        private DelegateCommand? canInsertCommand;
        public DelegateCommand CanInsertCommand => canInsertCommand ??=
            new(
                () => { }, // do nothing
                () => viewModel.Config.ReadWrite &&
                    (viewModel.Config.InsertSelectedCount && SelectedItems.Count > 0) ||
                    (!viewModel.Config.InsertSelectedCount)
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

        private DelegateCommand<DataGrid>? updateSelectedCommand;
        public DelegateCommand<DataGrid> UpdateSelectedCommand => updateSelectedCommand ??=
            new(
                (s) =>
                {
                    SelectedItems = s.SelectedItems.Cast<CsvRecord>().ToList();
                    if (SelectedItems.Any()) { s.ScrollIntoView(SelectedItems[0]); }
                }
            );

        private DelegateCommand? removeCommand;
        public DelegateCommand RemoveCommand => removeCommand ??= new(
            () =>
            {
                viewModel.Logger.Info("Removing rows (x{Count})", SelectedItems.Count);
                foreach (var item in SelectedItems)
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
            () => viewModel.Config.ReadWrite && viewModel.Edit.SelectedIndex != -1
        );

        private DelegateCommand<DataGridCellEditEndingEventArgs>? endEditCommand;
        public DelegateCommand<DataGridCellEditEndingEventArgs> EndEditCommand => endEditCommand ??= new(
            (e) =>
            {
                if (e is null) { return; }
                int columnIndex = viewModel.Csv.Headers.IndexOf(e.Column.Header.ToString()!);
                string oldString = CsvVM.GetField(
                    (CsvRecord)e.Row.Item,
                    columnIndex
                );

                if (((TextBox)e.EditingElement).Text == oldString) { return; }
                viewModel.History.AddOperation(
                    OperationEnum.Inline,
                    viewModel.Csv.Records.IndexOf((CsvRecord)e.Row.Item),
                    viewModel.Csv.DeepCopy((CsvRecord)e.Row.Item)
                );

                viewModel.Logger.Debug(viewModel.Csv.ConcatenateFields(viewModel.Csv.DeepCopy((CsvRecord)e.Row.Item)));
                viewModel.History.CommitOperation();
            },
            (e) => viewModel.Config.ReadWrite
        );

        public void Insert(int at)
        {
            int count = viewModel.Config.InsertSelectedCount
                ? selectedItems.Count
                : viewModel.Config.InsertCount;

            viewModel.Logger.Info("Inserting CSV (@{At} x{Count}, Template: {Template})", at, count, viewModel.Config.IsTemplate);
            IsInsertExpanded = false;

            DateTime now = DateTime.Now;
            CsvRecord templatedRow = new();

            // Templating. Expand static <[DdTt]> fields beforehand.
            if (viewModel.Config.IsTemplate)
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
                CsvRecord thisRow = viewModel.Csv.DeepCopy(templatedRow);
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
                viewModel.History.AddOperation(OperationEnum.Insert, at, thisRow);
            }
            viewModel.History.CommitOperation();
            viewModel.Edit.SelectedIndex = at;
        }

        private DelegateCommand? saveCommand;
        public DelegateCommand SaveCommand => saveCommand ??= new(
            () =>
            {
                viewModel.Logger.Info("Saving");
                // Write original header manually
                using var writer = new StreamWriter(viewModel.Config.CsvPath);
            
                string fullHeader = string.Join(
                    ",",
                    viewModel.Csv.Headers.Select(m => "\"" + m.Replace("\"", "\"\"") + "\"")
                );
                writer.WriteLine(fullHeader);
            
                foreach (CsvRecord record in viewModel.Csv.Records)
                {
                    string toOutput = viewModel.Csv.ConcatenateFields(record);
                    writer.WriteLine(toOutput);
                }
            
                IsDirtyEditor = false;
            },
            () => viewModel.Config.ReadWrite && isDirtyEditor
        );
    }    
}
