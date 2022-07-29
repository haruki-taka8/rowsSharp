using rowsSharp.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace rowsSharp.ViewModel
{
    public class FilterVM : ViewModelBase
    {
        private readonly RowsVM viewModel;
        public FilterVM(RowsVM inViewModel)
        {
            viewModel = inViewModel;
            useRegex = viewModel.Config.UseRegexFilter;
        }

        private bool hasFocus;
        public bool HasFocus
        {
            get => hasFocus;
            set
            {
                hasFocus = value;
                OnPropertyChanged(nameof(HasFocus));
            }
        }

        private string filterText = string.Empty;
        public string FilterText
        {
            get => filterText;
            set
            {
                filterText = value;
                OnPropertyChanged(nameof(FilterText));
            }
        }

        private DelegateCommand? focusCommand;
        public DelegateCommand FocusCommand => focusCommand ??= new(
            () => { HasFocus = false; HasFocus = true; }  // Force update view
        );

        private DelegateCommand? filterCommand;
        public DelegateCommand FilterCommand => filterCommand ??= new(
            () => Filter()
        );

        private List<KeyValuePair<string, string>> criteria = new();
        private bool useRegex;

        private static List<KeyValuePair<string, string>> ParseInput(
            string inFilterText,
            List<string> inHeaders,
            Dictionary<string, Dictionary<string, string>>? inAlias,
            bool inUseRegexFilter
        )
        {
            List<KeyValuePair<string, string>> output = new();

            string[] splittedFilterText = Regex.Split(inFilterText, "\\s+(?=(?:\"[^\"]*\"|[^\"])*$)");
            foreach (string criterion in splittedFilterText)
            {
                string[] keyvalue = Regex.Split(criterion, ":(?=(?:\"[^\"]*\"|[^\"])*$)");
                string header = keyvalue[0].Trim().Trim('"');
                string value = string.Empty;

                // Handle Header:Value
                if (keyvalue.Length == 2)
                {
                    if (!inHeaders.Contains(header)) { throw new InvalidFilterCriteriaException($"Invalid column {header}"); }

                    value = keyvalue[1].Trim().Trim('"');

                    if (inAlias is not null &&
                        inAlias.TryGetValue(header, out Dictionary<string, string>? thisAlias))
                    {
                        foreach (KeyValuePair<string, string> aliasKeyValue in thisAlias)
                        {
                            value = value.Replace(aliasKeyValue.Value, aliasKeyValue.Key);
                        }
                    }

                    // Convert user-provided header to internal ColumnX notation
                    header = inHeaders.IndexOf(header).ToString();
                }

                // Validate regular expression
                if (inUseRegexFilter)
                {
                    string regexToTest = value == string.Empty ? header : value;
                    try
                    {
                        Regex.IsMatch("", regexToTest);
                    }
                    catch
                    {
                        throw new InvalidFilterCriteriaException($"Invalid regex {regexToTest}");
                    }
                }
                output.Add(new(header, value));
            }
            return output;
        }

        private static ICollectionView OutputAlias(
            ICollectionView input,
            CsvVM inCsvVM,
            Dictionary<string, Dictionary<string, string>> inAlias
        )
        {
            List<CsvRecord> tempRecords = new();
            foreach (CsvRecord record in input)
            {
                CsvRecord thisRecord = inCsvVM.DeepCopy(record);
                for (int i = 0; i < inCsvVM.Headers.Count - 1; i++)
                {
                    inAlias.TryGetValue(inCsvVM.Headers[i], out Dictionary<string, string>? thisAlias);
                    if (thisAlias is null) { continue; }

                    foreach (KeyValuePair<string, string> aliasKeyValue in thisAlias)
                    {
                        CsvVM.SetField(
                            thisRecord,
                            i,
                            CsvVM.GetField(thisRecord, i).Replace(aliasKeyValue.Key, aliasKeyValue.Value)
                        );
                    }
                }
                tempRecords.Add(thisRecord);
            }
            return CollectionViewSource.GetDefaultView(tempRecords);
        }

        private bool RecordsViewFilter(object obj)
        {
            CsvRecord row = (CsvRecord)obj;
            foreach (KeyValuePair<string, string> criterion in criteria)
            {
                if (string.IsNullOrWhiteSpace(criterion.Value))
                {
                    string concatField = viewModel.Csv.ConcatenateFields(row);
                    if (
                        (useRegex && Regex.IsMatch(concatField, criterion.Key, RegexOptions.IgnoreCase)) ||
                        (!useRegex && concatField.ToLower().Contains(criterion.Key.ToLower()))
                    ) { continue; }
                    return false;
                }

                string field = CsvVM.GetField(row, int.Parse(criterion.Key));
                if (
                    (useRegex && Regex.IsMatch(field, criterion.Value, RegexOptions.IgnoreCase)) ||
                    (!useRegex && field.ToLower().Contains(criterion.Value.ToLower()))
                ) { continue; }
                return false;
            }
            return true;
        }

        private void Filter()
        {
            viewModel.Logger.Info("Filtering CSV, ({filter}, IOAlias: {IAlias}, {OAlias})",
                filterText,
                viewModel.Config.InputAlias,
                viewModel.Config.OutputAlias
            );

            // Parse input
            try
            {
                criteria = ParseInput(
                        filterText,
                        viewModel.Csv.Headers,
                        viewModel.Config.InputAlias ? viewModel.Config.Style.Alias : null,
                        useRegex
                );
            }
            catch (InvalidFilterCriteriaException ex)
            {
                viewModel.Logger.Warn(ex.Message);
                viewModel.RecordsView.Filter = (object obj) => false;
                return;
            }

            // Filtering
            viewModel.RecordsView = CollectionViewSource.GetDefaultView(viewModel.Csv.Records);
            viewModel.RecordsView.Filter = RecordsViewFilter;

            // Output alias
            if (viewModel.Config.OutputAlias)
            {
                viewModel.RecordsView = OutputAlias(viewModel.RecordsView, viewModel.Csv, viewModel.Config.Style.Alias);
            }

            viewModel.Preview.PreviewSource = new();
            viewModel.Logger.Debug("Filtering CSV completed");
            viewModel.Edit.SelectedIndex = -1;
        }
    }
}
