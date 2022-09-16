using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;
using rowsSharp.Model;

namespace rowsSharp.ViewModel;

public class Status : INPC
{
    public Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version!;

    private bool isDirtyEditor;
    public bool IsDirtyEditor
    {
        get => isDirtyEditor;
        set => SetField(ref isDirtyEditor, value);
    }

    private bool isEditing;
    public bool IsEditing
    {
        get => isEditing;
        set => SetField(ref isEditing, value);
    }

    private bool isInsertExpanded;
    public bool IsInsertExpanded
    {
        get => isInsertExpanded;
        set => SetField(ref isInsertExpanded, value);
    }

    private List<Record> selectedItems = new();
    public List<Record> SelectedItems
    {
        get => selectedItems;
        set => SetField(ref selectedItems, value);
    }

    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get => selectedIndex;
        set => SetField(ref selectedIndex, value);
    }

    private bool isFilterFocused;
    public bool IsFilterFocused
    {
        get => isFilterFocused;
        set => SetField(ref isFilterFocused, value);
    }

    private string filterText = string.Empty;
    public string FilterText
    {
        get => filterText;
        set => SetField(ref filterText, value);
    }

    private BitmapImage previewBitmap = new();
    public BitmapImage PreviewBitmap
    {
        get => previewBitmap;
        set => SetField(ref previewBitmap, value);
    }
}