using System.IO;
using System.Windows.Media.Imaging;

namespace rowsSharp.Domain;

internal static class Preview
{
    internal static BitmapImage? FromPath(string path)
    {
        if (!File.Exists(path)) { return null; }

        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new(path);
        image.EndInit();
        return image;
    }
}
