using System;

namespace rowsSharp.Domain;

internal static class BaseDir
{
    private const string Metavariable = "$baseDir";
    private static readonly string BasePath = Environment.CurrentDirectory + "/Userdata/";

    internal static string Expand(string path)
    {
        return path.Replace(Metavariable, BasePath);
    }
}
