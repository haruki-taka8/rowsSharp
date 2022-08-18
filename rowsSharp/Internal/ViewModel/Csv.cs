using CsvHelper;
using CsvHelper.Configuration;
using rowsSharp.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace rowsSharp.ViewModel;

public class CsvVM
{
    public List<string> Headers = new();
    public ObservableCollection<Record> Records = new();

    public static string GetField(Record record, int column)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return string.Empty; }
        
        // We're absolutely sure that
        // record is not null && "Column" + column is a valid field
        #pragma warning disable CS8602, CS8603, IDE0055
        return record.GetType()
                     .GetProperty("Column" + column)
                     .GetValue(record).ToString();
        #pragma warning restore CS8602, CS8603, IDE0055
    }

    public static void SetField(Record record, int column, string value)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return; }
        #pragma warning disable CS8602, IDE0055
        record.GetType()
              .GetProperty("Column" + column)
              .SetValue(record, value);
        #pragma warning restore CS8602, IDE0055
    }

    public Record DeepCopy(Record record)
    {
        Record output = new();
        for (int i = 0; i < Headers.Count; i++)
        {
            SetField(output, i, GetField(record, i));
        }
        return output;
    }

    public string ConcatenateFields(Record record)
    {
        string output = string.Empty;
        for (int i = 0; i < Headers.Count; i++)
        {
            output += '"' + GetField(record, i).Replace("\"", "\"\"") + "\",";
        }
        return output.TrimEnd(',');
    }

    public CsvVM(RowsVM viewModel)
    {
        string inputPath = viewModel.Config.CsvPath;
        if (!File.Exists(inputPath)) { return; }

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            BadDataFound = null,
            HasHeaderRecord = viewModel.Config.HasHeader
        };

        viewModel.Logger.Info("Loading CSV file @ {inputPath}", inputPath);
        using StreamReader reader = new(inputPath);
        using CsvReader csv = new(reader, config);
        csv.Context.RegisterClassMap<RecordMap>();

        Records = new(csv.GetRecords<Record>());
        Headers = csv.Context.Reader.HeaderRecord?.ToList() ?? new();

        // Default headers
        if (Headers.Any() || !Records.Any()) { return; }

        for (int i = 0; i < RecordMap.MaxColumns - 1; i++)
        {
            if (GetField(Records[0], i) == string.Empty) { break; }
            Headers.Add("Column" + i);
        }
    }
}
