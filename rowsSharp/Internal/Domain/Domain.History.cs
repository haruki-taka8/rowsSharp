using rowsSharp.Model;
using System;
using System.Windows;

namespace rowsSharp.Domain;
internal class History
{
    private readonly ViewModel.Status status;
    private readonly DataStore.OperationHistory operation;
    private readonly DataStore.Csv csv;
    internal History(ViewModel.Status status, DataStore.OperationHistory operation, DataStore.Csv csv)
    {
        this.status = status;
        this.operation = operation;
        this.csv = csv;
    }

    private bool parity;
    public void AddOperation(OperationType operationType, int at, Record oldRow)
    {
        operation.UndoStack.Push(new Operation(operationType, at, oldRow, parity));
    }

    public void CommitOperation()
    {
        operation.RedoStack.Clear();
        status.IsDirtyEditor = true;
        parity = !parity;
    }

    private static void DispatcherInvoke(Action action)
    {
        Application.Current.Dispatcher.BeginInvoke(action);
    }

    private void CommonOperation(bool isUndo, Operation last)
    {
        OperationType action = last.OperationType;
        if (action == OperationType.Inline)
        {
            DispatcherInvoke(() => csv.Records[last.At] = last.OldRow);
        }

        else if ((isUndo && action == OperationType.Remove) ||
           (!isUndo && action == OperationType.Insert)
        )
        {
            DispatcherInvoke(() => csv.Records.Insert(last.At, last.OldRow));
        }
        else
        { 
            DispatcherInvoke(() => csv.Records.RemoveAt(last.At));
        }
    }

    internal void Undo()
    {
        Operation last = operation.UndoStack.Pop();
        App.Logger.Info("Undo {Action} @ {At}, {Parity}", last.OperationType, last.At, last.Parity);

        operation.RedoStack.Push(new Operation(
            last.OperationType,
            last.At,
            last.OperationType == OperationType.Inline ? csv.Records[last.At] : last.OldRow,
            last.Parity
        ));
        CommonOperation(true, last);

        // Group insert/remove edits
        operation.UndoStack.TryPeek(out Operation? next);
        if (last.Parity == next?.Parity) { Undo(); }
    }

    internal void Redo()
    {
        Operation last = operation.RedoStack.Pop();
        App.Logger.Info("Redo {Action} @ {At}, {Parity}", last.OperationType, last.At, last.Parity);

        operation.UndoStack.Push(new Operation(
            last.OperationType,
            last.At,
            last.OperationType == OperationType.Inline ? csv.Records[last.At] : last.OldRow,
            last.Parity
        ));
        CommonOperation(false, last);

        operation.RedoStack.TryPeek(out Operation? next);
        if (last.Parity == next?.Parity) { Redo(); }
    }
}
