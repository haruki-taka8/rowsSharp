using System.Windows.Input;
using System.Collections.Generic;
using rowsSharp.Model;
using System.Linq;
using System;
using System.Windows;

namespace rowsSharp.ViewModel
{
    public class HistoryVM : ViewModelBase
    {
        private Stack<Operation> undoStack;
        public Stack<Operation> UndoStack
        {
            get { return undoStack; }
            set
            {
                undoStack = value;
                OnPropertyChanged(nameof(UndoStack));
            }
        }

        private Stack<Operation> redoStack;
        public Stack<Operation> RedoStack
        {
            get { return redoStack; }
            set
            {
                redoStack = value;
                OnPropertyChanged(nameof(RedoStack));
            }
        }

        private readonly RowsVM viewModel;
        internal HistoryVM(RowsVM inViewModel)
        {
            undoStack = new();
            redoStack = new();
            viewModel = inViewModel;
        }

        private ICommand? undoCommand;
        public ICommand UndoCommand => undoCommand ??= new CommandHandler(
            () => Undo(),
            () => viewModel.Config.ReadWrite && undoStack.Any()
        );

        private ICommand? redoCommand;
        public ICommand RedoCommand => redoCommand ??= new CommandHandler(
            () => Redo(),
            () => viewModel.Config.ReadWrite && redoStack.Any()
        );

        private ICommand? undoKeyCommand;
        public ICommand UndoKeyCommand => undoKeyCommand ??= new CommandHandler(
            () => Undo(),
            () => UndoCommand.CanExecute(this) && viewModel.Edit.IsEditing
        );

        private ICommand? redoKeyCommand;
        public ICommand RedoKeyCommand => redoKeyCommand ??= new CommandHandler(
            () => Redo(),
            () => RedoCommand.CanExecute(this) && viewModel.Edit.IsEditing
        );

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
            if (next is not null && last.Parity == next.Parity) { Undo(); }
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
            if (next is not null && last.Parity == next.Parity) { Redo(); }
        }
    }
}
