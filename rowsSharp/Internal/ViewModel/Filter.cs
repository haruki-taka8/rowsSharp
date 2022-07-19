using System.Windows.Input;
using rowsSharp.Model;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows;

namespace rowsSharp.ViewModel
{
    public class FilterVM : ViewModelBase
    {
        private readonly RowsVM viewModel;
        public FilterVM(RowsVM inViewModel)
        {
            viewModel = inViewModel;
        }

        private bool hasFocus;
        public bool HasFocus
        {
            get { return hasFocus; }
            set
            {
                hasFocus = value;
                OnPropertyChanged(nameof(HasFocus));
            }
        }

        private bool isFiltering;
        public bool IsFiltering
        {
            get { return isFiltering; }
            set
            {
                isFiltering = value;
                OnPropertyChanged(nameof(IsFiltering));
            }
        }

        private string filterText = string.Empty;
        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                OnPropertyChanged(nameof(FilterText));
            }
        }

        private ICommand? focusCommand;
        public ICommand FocusCommand => focusCommand ??= new CommandHandler(
            () => { HasFocus = false; HasFocus = true; },  // Force update view
            () => true
        );

        private ICommand? filterCommand;
        public ICommand FilterCommand => filterCommand ??= new CommandHandler(
            () => Filter(),
            () => !isFiltering
        );

        private void Filter()
        {
            Application.Current.Dispatcher.Invoke(() => IsFiltering = true);

            viewModel.Logger.Info("Filtering CSV, ({filter}, IOAlias: {IAlias}, {OAlias})",
                filterText,
                viewModel.Config.InputAlias,
                viewModel.Config.OutputAlias
            );

            // Parse input
            List<KeyValuePair<string, string>> criteria = new();
            string[]? filterTextSplitted = Regex.Split(filterText, " (?=(?:\"[^\"]*\"|[^\"])*$)");

            foreach (string entry in filterTextSplitted)
            {
                if (string.IsNullOrWhiteSpace(filterTextSplitted[0])) { continue; }

                string[] keyvalue = Regex.Split(entry, ":(?=(?:\"[^\"]*\"|[^\"])*$)");
                keyvalue[0] = keyvalue[0].Trim().Trim('"');

                // Convert column name into ColumnX, where 0 <= [int] X <= Column.Count-1
                if (keyvalue.Length == 2)
                {
                    int columnIndex = viewModel.Csv.Headers.IndexOf(keyvalue[0]);
                    if (columnIndex == -1)
                    {
                        viewModel.Logger.Warn("Invalid column {column}", keyvalue[0]);
                        viewModel.RecordsView.Filter = (object obj) => false;
                        return;
                    }

                    keyvalue[0] = "Column" + columnIndex;
                    keyvalue[1] = keyvalue[1].Trim().Trim('"');

                    // Input Alias
                    if (viewModel.Config.InputAlias)
                    {
                        var thisAlias = new Dictionary<string,string>();
                        if (viewModel.Config.Style.Alias.TryGetValue(keyvalue[0], out thisAlias))
                        {
                            foreach (KeyValuePair<string, string> aliasKeyValue in thisAlias)
                            {
                                keyvalue[1] = keyvalue[1].Replace(aliasKeyValue.Value, aliasKeyValue.Key);
                            }
                        }
                    }
                }

                // Check if regex is valid
                try
                {
                    Regex.IsMatch("", keyvalue[^1]);
                    criteria.Add(new KeyValuePair<string, string>(
                        keyvalue[0],
                        keyvalue.Length == 2 ? keyvalue[1] : string.Empty
                    ));
                }
                catch
                {
                    viewModel.Logger.Warn(
                        "Invalid regex at {header}, {value}.",
                        keyvalue[0],
                        keyvalue.Length == 2 ? keyvalue[1] : string.Empty
                    );
                    viewModel.RecordsView.Filter = (object obj) => false;
                    return;
                }
            }

            if (viewModel.Csv.Records is null)
            {
                viewModel.Logger.Warn("Empty filtering source");
                return;
            }

            // Filtering
            ObservableCollection<CsvRecord> originalList = viewModel.Csv.Records;
            viewModel.RecordsView.Filter = RecordsViewFilter;

            // Output alias
            if (viewModel.Config.OutputAlias)
            {
                ObservableCollection<CsvRecord> tempRecords = new();
                foreach (CsvRecord record in originalList)
                {
                    tempRecords.Add(viewModel.Csv.DeepCopy(record));
                }

                foreach (var record in tempRecords)
                {
                    for (int i = 0; i < viewModel.Csv.Headers.Count - 1; i++)
                    {
                        string originalColumnName = viewModel.Csv.Headers[i];

                        Dictionary<string, string>? thisAlias = new();
                        if (viewModel.Config.Style.Alias.TryGetValue(originalColumnName, out thisAlias))
                        {
                            foreach (KeyValuePair<string, string> aliasKeyValue in thisAlias)
                            {
                                PropertyInfo propertyInfo = record.GetType().GetProperty("Column" + i)!;
                                if (propertyInfo is null) { continue; }
                                propertyInfo.SetValue(
                                    record,
                                    propertyInfo.GetValue(record).ToString().Replace(aliasKeyValue.Key, aliasKeyValue.Value)
                                );
                            }
                        }
                    }
                }
            }

            bool RecordsViewFilter(object obj)
            {
                CsvRecord row = (CsvRecord)obj;
                foreach (KeyValuePair<string, string> keyValuePair in criteria)
                {
                    if (string.IsNullOrWhiteSpace(keyValuePair.Value))
                    {
                        if (!Regex.IsMatch(viewModel.Csv.ConcatenateFields(row), keyValuePair.Key, RegexOptions.IgnoreCase)) { return false; }
                        continue;
                    }

                    string field = row.GetType().GetProperty(keyValuePair.Key).GetValue(row).ToString()!;
                    if (!Regex.IsMatch(field, keyValuePair.Value, RegexOptions.IgnoreCase)) { return false; }
                }
                return true;
            }

            viewModel.Preview.PreviewSource = new();
            viewModel.Logger.Debug("Filtering CSV completed");
            isFiltering = IsFiltering = false;
            viewModel.Edit.SelectedIndex = -1;
        }
    }
}
