using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using rowsSharp.Model;
using System.Reflection;
using System.Windows.Controls;
using System.IO;
using System.Windows;

namespace rowsSharp.ViewModel
{
    public class EditVM : ViewModelBase
    {
        private bool isDirtyEditor;
        public bool IsDirtyEditor
        {
            get { return isDirtyEditor; }
            set
            {
                isDirtyEditor = value;
                OnPropertyChanged(nameof(IsDirtyEditor));
            }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get { return isEditing; }
            set
            {
                isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        private bool isInsertExpanded;
        public bool IsInsertExpanded
        {
            get { return isInsertExpanded; }
            set
            {
                isInsertExpanded = value;
                OnPropertyChanged(nameof(IsInsertExpanded));
            }
        }

        private List<CsvRecord> selectedItems;
        public List<CsvRecord> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                List<CsvRecord> old = selectedItems;
                selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));

                if (old.Any() && value.Any() && old[0] == value[0]) { return; }
                viewModel.Preview.UpdatePreviewCommand.Execute(this);
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        private readonly RowsVM viewModel;
        public EditVM(RowsVM inViewModel)
        {
            viewModel = inViewModel;
            isInsertExpanded = false;
            isDirtyEditor = false;
            selectedItems = new List<CsvRecord>();
            selectedIndex = -1;
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
                (s) => { ((DataGrid)s).CommitEdit(); },
                (s) => true
            );

        private DelegateCommand<System.Collections.IList>? updateSelectedCommand;
        public DelegateCommand<System.Collections.IList> UpdateSelectedCommand => updateSelectedCommand ??=
            new DelegateCommand<System.Collections.IList>(
                (s) => { SelectedItems = s.Cast<CsvRecord>().ToList(); },
                (s) => true
            );

        private ICommand? outputAliasCommand;
        public ICommand OutputAliasCommand => outputAliasCommand ??=
            new CommandHandler(
                () => ToggleReadWrite(),
                () => true
            );

        private void ToggleReadWrite()
        {
            viewModel.Config.SetReadWriteCommand.Execute(this);
            viewModel.Filter.FilterCommand.Execute(this);
        }

        private bool IsAnyRowSelected()
        {
            if (viewModel.RecordsView is null) { return false; }
            return viewModel.Config.ReadWrite && SelectedIndex != -1;
        }

        private ICommand? insertTopCommand;
        public ICommand InsertTopCommand => insertTopCommand ??=
            new CommandHandler(
                () => Insert(0, 0),
                () => viewModel.Config.ReadWrite
            );

        private ICommand? insertAboveCommand;
        public ICommand InsertAboveCommand => insertAboveCommand ??=
            new CommandHandler(
                () => Insert(SelectedIndex, 0),
                () => IsAnyRowSelected()
            );

        private ICommand? insertBelowCommand;
        public ICommand InsertBelowCommand => insertBelowCommand ??=
            new CommandHandler(
                () => Insert(SelectedIndex, SelectedItems.Count),
                () => IsAnyRowSelected()
            );

        private ICommand? insertLastCommand;
        public ICommand InsertLastCommand => insertLastCommand ??= 
            new CommandHandler(
                () => Insert(viewModel.Csv.Records.Count - 1, 1),
                () => viewModel.Config.ReadWrite
            );

        private DelegateCommand<object>? removeCommand;
        public DelegateCommand<object> RemoveCommand => removeCommand ??= new(
            (s) =>
            {
                viewModel.Logger.Info("Removing rows (x{Count})", SelectedItems.Count);

                foreach (var item in SelectedItems)
                {
                    viewModel.History.UndoStack.Push(
                        new Operation()
                        {
                            OperationEnum = OperationEnum.Remove,
                            OldRow = item,
                            At = viewModel.Csv.Records.IndexOf(item)
                        }
                    );
                    viewModel.Logger.Fatal(viewModel.Csv.Records.IndexOf(item));
                    viewModel.Csv.Records.Remove(item);
                }

                viewModel.History.RedoStack.Clear();
                IsDirtyEditor = true;
            },
            (s) => viewModel.Config.ReadWrite
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
                viewModel.History.UndoStack.Push(
                    new Operation()
                    {
                        OperationEnum = OperationEnum.Inline,
                        OldRow = viewModel.Csv.DeepCopy((CsvRecord)e.Row.Item),
                        At = viewModel.Csv.Records.IndexOf((CsvRecord)e.Row.Item)
                    }
                );
                viewModel.History.RedoStack.Clear();
                IsDirtyEditor = true;
            },
            (e) => viewModel.Config.ReadWrite
        );

        public void Insert (int at, int offset)
        {
            at += offset;
            int count = viewModel.Config.InsertSelectedCount
                ? selectedItems.Count
                : viewModel.Config.InsertCount;

            viewModel.Logger.Info("Inserting CSV (@{At} x{Count}, Template: {Template})", at, count, viewModel.Config.IsTemplate);
            IsInsertExpanded = false;

            DateTime now = DateTime.Now;
            CsvRecord templatedRow = new();

            // Templating. Expand static <[DdTt]> fields beforehand.
            if(viewModel.Config.IsTemplate)
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
                            Replace("<!#>", (count-i).ToString())
                    );
                }
                Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    viewModel.Csv.Records.Insert(at + i, thisRow);
                });
                viewModel.History.UndoStack.Push(
                    new Operation()
                    {
                        OperationEnum = OperationEnum.Insert,
                        OldRow = thisRow,
                        At = at
                    }
                );
            }

            viewModel.History.RedoStack.Clear();
            IsDirtyEditor = true;
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
                     viewModel.Csv.Headers.Select(m => "\"" + m + "\"")
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
