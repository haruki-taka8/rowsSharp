using System.IO;
using System.Windows.Media.Imaging;

namespace RowsSharp.Domain;

internal class PreviewHelper
{
    internal static BitmapImage? FromPath(string path)
    {
        if (!File.Exists(path)) { return null; }

        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new(path);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
