using CsvHelper;
using CsvHelper.Configuration;
using rowsSharp.Model;
using System.Globalization;
using System.IO;
using System.Linq;

namespace rowsSharp.Domain.IO;

internal static class Csv
{
    internal static DataStore.Csv Import(string path, bool hasHeader)
    {
        if (!File.Exists(path)) { return new(); }
    
        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            BadDataFound = null,
            HasHeaderRecord = hasHeader
        };
    
        App.Logger.Info("Loading CSV @ {path}", path);
        using StreamReader reader = new(path);
        using CsvReader csvReader = new(reader, config);
        csvReader.Context.RegisterClassMap<RecordMap>();

        DataStore.Csv csv = new()
        {
            Records = new(csvReader.GetRecords<Record>()),
            Headers = csvReader.Context.Reader.HeaderRecord?.ToList() ?? new()
        };

        // Default headers
        if (csv.Headers.Any() || !csv.Records.Any()) { return csv; }
    
        for (int i = 0; i < RecordMap.MaxColumns - 1; i++)
        {
            if (csv.Records[0].GetField(i) == string.Empty) { break; }
            csv.Headers.Add("Column" + i);
        }
        return csv;
    }
}
