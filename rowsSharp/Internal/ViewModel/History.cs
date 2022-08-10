using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace rowsSharp.ViewModel;

public class HistoryVM : ViewModelBase
{
    private readonly RowsVM viewModel;
    internal HistoryVM(RowsVM inViewModel)
    {
        viewModel = inViewModel;
    }

    private Stack<Operation> undoStack = new();
    public Stack<Operation> UndoStack
    {
        get => undoStack;
        set
        {
            undoStack = value;
            OnPropertyChanged(nameof(UndoStack));
        }
    }

    private Stack<Operation> redoStack = new();
    public Stack<Operation> RedoStack
    {
        get => redoStack;
        set
        {
            redoStack = value;
            OnPropertyChanged(nameof(RedoStack));
        }
    }

    private DelegateCommand? undoCommand;
    public DelegateCommand UndoCommand => undoCommand ??= new(
        () => Undo(),
        () => viewModel.Config.CanEdit && undoStack.Any()
    );

    private DelegateCommand? redoCommand;
    public DelegateCommand RedoCommand => redoCommand ??= new(
        () => Redo(),
        () => viewModel.Config.CanEdit && redoStack.Any()
    );

    private bool parity;
    public void AddOperation(OperationEnum operationEnum, int at, Record oldRow)
    {
        UndoStack.Push(
            new Operation()
            {
                OperationEnum = operationEnum,
                At = at,
                OldRow = oldRow,
                Parity = parity
            }
        );
    }

    public void CommitOperation()
    {
        RedoStack.Clear();
        viewModel.Edit.IsDirtyEditor = true;
        parity = !parity;
    }

    private static void DispatcherInvoke(Action action)
    {
        Application.Current.Dispatcher.BeginInvoke(action);
    }

    private void CommonOperation (bool isUndo, Operation last)
    {
        OperationEnum action = last.OperationEnum;
        if (action == OperationEnum.Inline)
        {
            DispatcherInvoke(() => viewModel.Csv.Records[last.At] = last.OldRow);
            return;
        }

        if ((isUndo && action == OperationEnum.Remove) ||
           (!isUndo && action == OperationEnum.Insert)    
        )
        {
            DispatcherInvoke(() => viewModel.Csv.Records.Insert(last.At, last.OldRow));
            return;
        }

        DispatcherInvoke(() => viewModel.Csv.Records.RemoveAt(last.At));
    }

    private void Undo()
    {
        Operation last = UndoStack.Pop();
        viewModel.Logger.Info("Undo {Action} @ {At}, {Parity}", last.OperationEnum, last.At, last.Parity);

        Operation operation = Operation.DeepCopy(last);
        operation.OldRow = last.OperationEnum == OperationEnum.Inline ?
            viewModel.Csv.Records[last.At] :
            last.OldRow;
        RedoStack.Push(operation);
        CommonOperation(true, last);

        // Group insert/remove edits
        UndoStack.TryPeek(out Operation? next);
        if (last.Parity == next?.Parity) { Undo(); }
    }

    private void Redo()
    {
        Operation last = RedoStack.Pop();
        viewModel.Logger.Info("Redo {Action} @ {At}, {Parity}", last.OperationEnum, last.At, last.Parity);

        Operation operation = Operation.DeepCopy(last);
        operation.OldRow = last.OperationEnum == OperationEnum.Inline ?
            viewModel.Csv.Records[last.At] :
            last.OldRow;
        UndoStack.Push(operation);
        CommonOperation(false, last);

        RedoStack.TryPeek(out Operation? next);
        if (last.Parity == next?.Parity) { Redo(); }
    }
}
