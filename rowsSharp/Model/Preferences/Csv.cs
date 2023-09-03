namespace RowsSharp.Model;

public class Csv
{
    /// <summary>
    /// Path to the CSV.
    /// </summary>
    public string Path { get; set; } = "$baseDir/CSVData/data.csv";

    /// <summary>
    /// Describes whether the provided CSV file contains headers in its first row.
    /// If not, numbered headers will be generated.
    /// </summary>
    public bool HasHeader { get; set; } = true;
}
