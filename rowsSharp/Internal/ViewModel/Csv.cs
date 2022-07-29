using CsvHelper;
using CsvHelper.Configuration;
using rowsSharp.Model;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace rowsSharp.ViewModel
{
    public class CsvVM : Csv
    {
        public static string GetField(CsvRecord record, int column)
        {
            if (column < 0 || column > MaxColumns - 1) { return string.Empty; }

            // We're absolutely sure that
            // record is not null && "Column" + column is a valid field
            #pragma warning disable CS8602, CS8603, IDE0055
            return record.GetType()
                         .GetProperty("Column" + column)
                         .GetValue(record).ToString();
            #pragma warning restore CS8602, CS8603, IDE0055
        }

        public static void SetField(CsvRecord record, int column, string value)
        {
            if (column < 0 || column > MaxColumns - 1) { return; }
            #pragma warning disable CS8602, IDE0055
            record.GetType()
                  .GetProperty("Column" + column)
                  .SetValue(record, value);
            #pragma warning restore CS8602, IDE0055
        }

        public CsvRecord DeepCopy(CsvRecord record)
        {
            CsvRecord output = new();
            for (int i = 0; i < Headers.Count; i++)
            {
                SetField(output, i, GetField(record, i));
            }
            return output;
        }

        public string ConcatenateFields(CsvRecord record)
        {
            string output = string.Empty;
            for (int i = 0; i < Headers.Count; i++)
            {
                output += '"' + GetField(record, i).Replace("\"", "\"\"") + "\",";
            }
            return output.TrimEnd(',');
        }

        public CsvVM(RowsVM viewModel, string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                viewModel.Logger.Warn("CSV file not found. Starting creation wizard.");
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                BadDataFound = null,
                HasHeaderRecord = viewModel.Config.HasHeader
            };

            viewModel.Logger.Info("Loading CSV file @ {inputPath}", inputPath);
            using StreamReader reader = new(inputPath);
            using CsvReader csv = new(reader, config);
            csv.Context.RegisterClassMap<CsvRecordMap>();

            Records = new(csv.GetRecords<CsvRecord>());
            Headers = csv.Context.Reader.HeaderRecord is null
                ? new()
                : csv.Context.Reader.HeaderRecord.ToList();

            if (Headers.Any()) { return; }
            if (!Records.Any())
            {
                viewModel.Logger.Warn("CSV file empty. Starting creation wizard.");
                return;
            }

            // Default headers
            for (int i = 0; i < MaxColumns - 1; i++)
            {
                if (GetField(Records[0], i) == string.Empty) { break; }
                Headers.Add("Column" + i);
            }
        }
    }
}
