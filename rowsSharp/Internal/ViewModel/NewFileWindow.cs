using rowsSharp.Model;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace rowsSharp.ViewModel
{
    public class NewFileWindowVM : ViewModelBase
    {
        public Config Config { get; }
        public NewFileWindowVM(Config config) => Config = config;

        private string headers = string.Empty;
        public string Headers
        {
            get => headers;
            set
            {
                headers = value;
                OnPropertyChanged(nameof(Headers));
            }
        }

        private ICommand? createCommand;
        public ICommand CreateCommand => createCommand ??= new CommandHandler(
            () =>
            {
                string[] toWrite =
                {
                    Headers,
                    "Placeholder 1"
                };
                File.WriteAllLines(Config.CsvPath, toWrite);
            },
            () => !Regex.IsMatch(Headers, @"^[,\s]*$")
        );
    }
}
