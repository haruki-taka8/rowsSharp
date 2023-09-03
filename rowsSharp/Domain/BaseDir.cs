using System;

namespace RowsSharp.Domain;

internal static class BaseDir
{
    private const string Metavariable = "$baseDir";
    private static readonly string BasePath = Environment.CurrentDirectory + "/Userdata/";
    private static readonly string BasePathEscaped = BasePath.Replace("\\", "\\\\");

    /// <summary>
    /// Replace "$baseDir" in <paramref name="value"/> with the root directory of the program.
    /// </summary>
    internal static string Expand(string value)
    {
        return value.Replace(Metavariable, BasePath);
    }

    /// <summary>
    /// Replace "$baseDir" in <paramref name="value"/> with the root directory of the program, with the backslash path separator escaped.
    /// Useful for deserializing JSON and other data types.
    /// </summary>
    internal static string ExpandEscaped(string value)
    {
        return value.Replace(Metavariable, BasePathEscaped);
    }
}
