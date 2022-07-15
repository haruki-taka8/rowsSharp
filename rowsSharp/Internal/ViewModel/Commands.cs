using System;
using System.Windows.Input;

namespace rowsSharp.ViewModel
{
    /// <summary>
    ///                [Command Handler](https://stackoverflow.com/a/12423962)
    /// by             [yo chauhan](https://stackoverflow.com/users/1217882/yo-chauhan)
    /// licensed under [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/)
    /// </summary>

    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Creates instance of the command handler
        /// </summary>
        /// <param name="action">Action to be executed by the command</param>
        /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Forces checking if execute is allowed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object? parameter)
        {
            _action();
        }
    }

    public class DelegateCommand<T> : ICommand where T : class
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public DelegateCommand(Action<T> execute) : this(execute, null) {}

        public DelegateCommand (Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute is null) { return true; }
            if (parameter is null) { return false; }

            return _canExecute((T)parameter);
        }

        public void Execute(object? parameter)
        {
            if (parameter is null) { return; }
            _execute((T)parameter);
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged is null) { return; }
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
