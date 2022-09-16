using System.Collections.Generic;

namespace rowsSharp.DataStore;

internal class OperationHistory : INPC
{
    private Stack<Model.Operation> undoStack = new();
    internal Stack<Model.Operation> UndoStack
    {
        get => undoStack;
        set => SetField(ref undoStack, value);
    }

    private Stack<Model.Operation> redoStack = new();
    internal Stack<Model.Operation> RedoStack
    {
        get => redoStack;
        set => SetField(ref redoStack, value);
    }
}
