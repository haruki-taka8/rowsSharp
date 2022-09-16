using System.IO;
using System.Text.RegularExpressions;

namespace rowsSharp.Domain;

internal class InitWizard : INPC
{
    private DataStore.Config config;
    public DataStore.Config Config
    {
        get => config;
        set => SetField(ref config, value);
    }

    internal InitWizard(DataStore.Config config)
    {
        Config = this.config = config;
    }

    private string initHeaders = string.Empty;
    public string InitHeaders
    {
        get => initHeaders;
        set => SetField(ref initHeaders, value);
    }

    // private DelegateCommand? createCommand;
    public DelegateCommand CreateCommand => new(
        () =>
        {
            string[] toWrite =
            {
                InitHeaders,
                "Placeholder 1"
            };
            File.WriteAllLines(Config.CsvPath, toWrite);
        },
        () => !Regex.IsMatch(InitHeaders, @"^[,\s]*$")
    );
}
