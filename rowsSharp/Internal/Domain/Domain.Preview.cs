using rowsSharp.Model;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows;
using System;

namespace rowsSharp.Domain;

internal class Preview
{
    private readonly ViewModel.Status status;
    private readonly DataStore.Csv csv;
    private readonly string previewPath;
    private readonly string copyRowFormat;

    internal Preview(ViewModel.Status inputStatus, DataStore.Csv inputCsv, string inputPreviewPath, string inputCopyRowFormat)
    {
        status = inputStatus;
        csv = inputCsv;
        previewPath = inputPreviewPath;
        copyRowFormat = inputCopyRowFormat;
    }

    private string ExpandColumnNotation(string inString, Record activeRow)
    {
        MatchCollection matches = Regex.Matches(inString, @"(?<=<)(.+?)(?=>)");
        foreach (Match match in matches.Cast<Match>())
        {
            int columnIndex = csv.Headers.IndexOf(match.Value);
            if (columnIndex == -1) { return string.Empty; }

            string replaceFrom = string.Format("<{0}>", match.Value);
            string replaceTo = activeRow.GetField(columnIndex);

            inString = inString.Replace(replaceFrom, replaceTo);
        }
        return inString;
    }

    internal void UpdatePreview()
    {
        status.PreviewBitmap = new();
        if (!status.SelectedItems.Any()) { return; }

        string path = ExpandColumnNotation(previewPath, status.SelectedItems[0]);

        if (!File.Exists(path))
        {
            App.Logger.Warn("Failed to set preview because of non-existent file @ {path}", path);
            status.PreviewBitmap = new();
            return;
        }

        // Don't permanently lock the image
        App.Logger.Info("Setting preview to {path}", path);

        BitmapImage previewSource = new();
        previewSource.BeginInit();
        previewSource.UriSource = new Uri(path);
        previewSource.CacheOption = BitmapCacheOption.OnLoad;
        previewSource.EndInit();
        previewSource.Freeze();
        status.PreviewBitmap = previewSource;
    }

    internal void CopyImage()
    {
        App.Logger.Info("Copying preview image");
        Clipboard.SetImage(status.PreviewBitmap);
    }

    internal void CopyString()
    {
        App.Logger.Info("Copying row string");
        string toCopy = ExpandColumnNotation(copyRowFormat, status.SelectedItems[0]);
        Clipboard.SetText(toCopy);
    }
}
