namespace rowsSharp.Model;
internal static class ModelExtensions
{
    internal static string GetField(this Record record, int column)
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

    internal static void SetField(this Record record, int column, string value)
    {
        if (column < 0 || column > RecordMap.MaxColumns - 1) { return; }
        #pragma warning disable CS8602
        record.GetType()
              .GetProperty("Column" + column)
              .SetValue(record, value);
        #pragma warning restore CS8602
    }

    internal static Record DeepCopy(this Record record, int columnCount = RecordMap.MaxColumns - 1)
    {
        Record output = new();
        for (int i = 0; i < columnCount; i++)
        {
            SetField(output, i, record.GetField(i));
        }
        return output;
    }

    internal static string ConcatenateFields(this Record record, int columnCount = RecordMap.MaxColumns - 1)
    {
        string output = string.Empty;
        for (int i = 0; i < columnCount; i++)
        {
            output += '"' + record.GetField(i).Replace("\"", "\"\"") + "\",";
        }
        return output.TrimEnd(',');
    }
}
