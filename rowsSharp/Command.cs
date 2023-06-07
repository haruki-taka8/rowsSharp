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

/* 
 * The following constructors deliberately ALLOW the use of value type,
 * despite Microsoft.Practices.Prism.Commands doing otherwise.
 * 
 * It can be safely assumed that CanExecute(null) will not cause any
 * issues.
 */

public class DelegateCommand<T> : ICommand
{
    private readonly Func<T, bool> _canExecute;
    private readonly Action<T> _execute;

    internal DelegateCommand(Action<T> execute) : this(execute, (T parameter) => true) { }
    internal DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return parameter is T type
            && _canExecute.Invoke(type);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T type)
        {
            _execute(type);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
