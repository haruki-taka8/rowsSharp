using System;
using System.Windows.Input;

namespace rowsSharp;

public class DelegateCommand : ICommand
{
    private readonly Func<bool> _canExecute;
    private readonly Action _execute;

    internal DelegateCommand(Action execute) : this(execute, () => true) { }
    internal DelegateCommand(Action execute, Func<bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute.Invoke();
    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class DelegateCommand<T> : ICommand where T : class
{
    private readonly Predicate<T> _canExecute;
    private readonly Action<T> _execute;

    internal DelegateCommand(Action<T> execute) : this(execute, (T obj) => true) { }

    internal DelegateCommand(Action<T> execute, Predicate<T> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) =>
        _canExecute is not null
        && parameter is not null
        && _canExecute((T)parameter);

    public void Execute(object? parameter)
    {
        if (parameter is not null) { _execute((T)parameter); }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged()
    {
        if (CanExecuteChanged is not null) { CanExecuteChanged(this, EventArgs.Empty); }
    }
}
