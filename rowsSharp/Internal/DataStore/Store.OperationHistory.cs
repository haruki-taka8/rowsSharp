using rowsSharp.Model;
using System.Collections.Generic;

namespace rowsSharp.DataStore;

internal class OperationHistory
{
    internal Stack<Operation> UndoStack = new();
    internal Stack<Operation> RedoStack = new();
}
