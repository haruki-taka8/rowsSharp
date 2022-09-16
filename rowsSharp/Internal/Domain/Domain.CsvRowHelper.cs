using rowsSharp.Model;

namespace rowsSharp.Domain;
internal class CsvRowHelper
{
    private readonly DataStore.Csv csv;
    internal CsvRowHelper(DataStore.Csv inputCsvDataStore)
    {
        csv = inputCsvDataStore;
    }

    internal static string GetField(Record record, int column)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return string.Empty; }

        // We're absolutely sure that
        // record is not null && "Column" + column is a valid field
        #pragma warning disable CS8602, CS8603
        return record.GetType()
                     .GetProperty("Column" + column)
                     .GetValue(record).ToString();
        #pragma warning restore CS8602, CS8603
    }

    internal static void SetField(Record record, int column, string value)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return; }
        #pragma warning disable CS8602
        record.GetType()
              .GetProperty("Column" + column)
              .SetValue(record, value);
        #pragma warning restore CS8602
    }

    internal Record DeepCopy(Record record)
    {
        Record output = new();
        for (int i = 0; i < csv.Headers.Count; i++)
        {
            SetField(output, i, GetField(record, i));
        }
        return output;
    }

    internal string ConcatenateFields(Record record)
    {
        string output = string.Empty;
        for (int i = 0; i < csv.Headers.Count; i++)
        {
            output += '"' + GetField(record, i).Replace("\"", "\"\"") + "\",";
        }
        return output.TrimEnd(',');
    }
}
