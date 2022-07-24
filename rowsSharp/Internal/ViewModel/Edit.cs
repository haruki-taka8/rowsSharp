using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using rowsSharp.Model;
using System.Reflection;
using System.Windows.Controls;
using System.IO;

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

        private ICommand? beginEditCommand;
        public ICommand BeginEditCommand => beginEditCommand ??=
            new CommandHandler(
                () => viewModel.Edit.IsEditing = true,
                () => viewModel.Config.ReadWrite
            );

        private DelegateCommand<object>? commitEditCommand;
        public DelegateCommand<object> CommitEditCommand => commitEditCommand ??=
            new DelegateCommand<object>(
                (s) => ((DataGrid)s).CommitEdit(),
                (s) => viewModel.Config.ReadWrite
            );

        private DelegateCommand<System.Collections.IList>? updateSelectedCommand;
        public DelegateCommand<System.Collections.IList> UpdateSelectedCommand => updateSelectedCommand ??=
            new DelegateCommand<System.Collections.IList>(
                (s) => SelectedItems = s.Cast<CsvRecord>().ToList(),
                (s) => true
            );

        private bool CanInsertTopOrBottom()
        {
            return viewModel.Config.ReadWrite &&
                (
                    (viewModel.Config.InsertSelectedCount && SelectedItems.Any()) ||
                    (viewModel.Config.InsertSelectedCount == false)
                );
        }

        private bool IsAnyRowSelected()
        {
            return viewModel.Config.ReadWrite &&
                !viewModel.RecordsView.IsEmpty &&
                SelectedIndex != -1;
        }

        private ICommand? insertTopCommand;
        public ICommand InsertTopCommand => insertTopCommand ??=
            new CommandHandler(
                () => Insert(0),
                () => CanInsertTopOrBottom()
            );

        private ICommand? insertAboveCommand;
        public ICommand InsertAboveCommand => insertAboveCommand ??=
            new CommandHandler(
                () => Insert(SelectedIndex),
                () => IsAnyRowSelected()
            );

        private ICommand? insertBelowCommand;
        public ICommand InsertBelowCommand => insertBelowCommand ??=
            new CommandHandler(
                () => Insert(SelectedIndex + SelectedItems.Count),
                () => IsAnyRowSelected()
            );

        private ICommand? insertLastCommand;
        public ICommand InsertLastCommand => insertLastCommand ??= 
            new CommandHandler(
                () => Insert(viewModel.Csv.Records.Count),
                () => CanInsertTopOrBottom()
            );

        private ICommand? removeCommand;
        public ICommand RemoveCommand => removeCommand ??= new CommandHandler(
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
            () => viewModel.Config.ReadWrite && !viewModel.RecordsView.IsEmpty
        );

        private DelegateCommand<DataGridCellEditEndingEventArgs>? endEditCommand;
        public DelegateCommand<DataGridCellEditEndingEventArgs> EndEditCommand => endEditCommand ??= new(
            (e) =>
            {
                int columnIndex = viewModel.Csv.Headers.IndexOf(e.Column.Header.ToString()!);
                string oldString = ((CsvRecord)e.Row.Item).
                    GetType().
                    GetProperty("Column" + columnIndex).
                    GetValue((CsvRecord)e.Row.Item, null).
                    ToString()!;

                if (((TextBox)e.EditingElement).Text == oldString) { return; }
                viewModel.History.AddOperation(
                    OperationEnum.Inline,
                    viewModel.Csv.Records.IndexOf((CsvRecord)e.Row.Item),
                    viewModel.Csv.DeepCopy((CsvRecord)e.Row.Item)
                );
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

                    PropertyInfo? propertyInfo = templatedRow.GetType().GetProperty("Column" + columnIndex);
                    if (propertyInfo is null) { continue; }

                    propertyInfo.SetValue(templatedRow,
                        keyValuePair.Value.
                            Replace("<D>", now.ToString("yyyyMMdd")).
                            Replace("<d>", now.ToString("yyyy-MM-dd")).
                            Replace("<T>", now.ToString("HHmmss")).
                            Replace("<t>", now.ToString("HH:mm:ss"))
                    );
                }
            }

            // Insert
            for (int i = 0; i < count; i++)
            {
                CsvRecord thisRow = viewModel.Csv.DeepCopy(templatedRow);
                foreach (PropertyInfo propertyInfo in templatedRow.GetType().GetProperties())
                {
                    propertyInfo.SetValue(thisRow,
                        propertyInfo.GetValue(thisRow).ToString().
                            Replace("<#>", i.ToString()).
                            Replace("<!#>", (count - i - 1).ToString())
                    );
                }
                viewModel.Csv.Records.Insert(at + i, thisRow);
                viewModel.History.AddOperation(OperationEnum.Insert, at, thisRow);
            }
            viewModel.History.CommitOperation();
            viewModel.Edit.SelectedIndex = at;
        }

        private ICommand? saveCommand;
        public ICommand SaveCommand => saveCommand ??= new CommandHandler(
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
