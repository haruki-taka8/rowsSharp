using rowsSharp.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace rowsSharp.DataStore;

internal class Csv : INPC
{
    internal List<string> Headers = new();
    internal ObservableCollection<Record> Records = new();
}
