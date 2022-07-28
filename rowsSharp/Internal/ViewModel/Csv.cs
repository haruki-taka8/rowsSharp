using CsvHelper;
using CsvHelper.Configuration;
using rowsSharp.Model;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
            List<string> fields = new();
            for (int i = 0; i < Headers.Count; i++)
            {
                fields.Add(GetField(record, i));
            }

            return string.Join(
                ",",
                fields.Select(m => "\"" + m.Replace("\"", "\"\"") + "\"")
            );
        }

        public CsvVM(RowsVM viewModel, string inputPath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                BadDataFound = null,
                HasHeaderRecord = viewModel.Config.HasHeader
            };

            if (!File.Exists(inputPath))
            {
                viewModel.Logger.Warn("CSV file not found. Starting creation wizard.");
                return;
            }

            viewModel.Logger.Info("Loading CSV file @ {inputPath}", inputPath);
            using var reader = new StreamReader(inputPath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<CsvRecordMap>();
            Records = new(csv.GetRecords<CsvRecord>());

            if (viewModel.Config.HasHeader)
            {
                Headers = csv.Context.Reader.HeaderRecord.ToList();
                return;
            }

            if (!Records.Any())
            {
                Records = new();
                viewModel.Logger.Warn("CSV file not found (HasHeader is FALSE). Starting creation wizard.");
                return;
            }

            Headers = new();
            for (int i = 0; i < 31; i++)
            {
                if (GetField(Records[0], i) == string.Empty) { break; }
                Headers.Add("Column" + i);
            }
        }
    }
}
