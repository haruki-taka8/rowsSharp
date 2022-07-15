using System.IO;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace rowsSharp.ViewModel
{
    public class NewFileWindowVM : ViewModelBase
    {
        public RowsVM RowsVM { get; set; }

        private string headers = string.Empty;
        public string Headers
        {
            get { return headers; }
            set
            {
                headers = value;
                OnPropertyChanged(nameof(Headers));
            }
        }

        public ICommand CreateCommand => new CommandHandler(
            () =>
            {
                RowsVM.Logger.Info(Headers);
                string[] toWrite =
                {
                    Headers,
                    "Placeholder 1",
                    "Placeholder 2"
                };
                File.WriteAllLines(RowsVM.Config.CsvPath, toWrite);
            },
            () =>
            {
                return
                    !string.IsNullOrWhiteSpace(Headers) &&
                    !Regex.IsMatch(Headers, @"^,*$");
            }
        );

        public NewFileWindowVM(RowsVM rowsVM)
        {
            RowsVM = rowsVM;
            Headers = string.Empty;
        }
    }
}
