using System.Collections.Generic;
using System.Linq;

namespace rowsSharp.Domain;

internal static class ColumnNotation
{
    internal static string Expand(string path, IEnumerable<string> headers, IEnumerable<string?> row)
    {
        foreach (var (header, field) in headers.Zip(row))
        {
            path = path.Replace("<" + header + ">", field);
        }
        return path;
    }
}
