using System.Collections.Generic;
using System.Linq;
using rowsSharp.Model;
using CsvHelper;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;
using System.Reflection;

namespace rowsSharp.ViewModel
{
    public class CsvVM : Csv
    {
        public CsvRecord DeepCopy(CsvRecord Record)
            {
                CsvRecord output = new();
                for (int i = 0; i < Headers.Count; i++)
                {
                    PropertyInfo propertyInfo = Record.GetType().GetProperty("Column" + i)!;
                    if (propertyInfo is not null)
                    {
                        propertyInfo.SetValue(output, propertyInfo.GetValue(Record).ToString());
                    }
                }
                return output;
            }

        public string ConcatenateFields(CsvRecord Record)
        {
            string output = string.Empty;
            List<string> records = new();
            for (int i = 0; i < Headers.Count; i++)
            {
                records.Add(Record.GetType().GetProperty("Column" + i).GetValue(Record).ToString()!);
            }

            output = string.Join(
                ",",
                records.Select(m => "\"" + m + "\"")
            );

            return output;
        }

        public string ColumnToInternalName(string ColumnName)
        {
            return "Column" + Headers.IndexOf(ColumnName);
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
                if (Records[0].GetType().GetProperty("Column" + i).GetValue(Records[0]).ToString() == string.Empty) { break; }
                Headers.Add("Column" + i);
            }
        }
    }
}
