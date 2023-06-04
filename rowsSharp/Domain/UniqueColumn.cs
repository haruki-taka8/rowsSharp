using ObservableTable.Core;
using System.Collections.Generic;

namespace rowsSharp.Domain;

internal class UniqueColumn
{
    private int index;

	private static Column<string> GetNumberedColumn(int index)
	{
		return new(index.ToString());
	}

	internal IEnumerable<Column<string>> Next(int count = 1)
	{
		for (int i = 0; i < count; i++)
		{
			yield return GetNumberedColumn(index++);
		}
	}
}
