using System;
using System.Timers;

namespace RowsSharp.Domain;

/// <summary>
/// Encapsulates System.Timers.Timer.
/// Consumer needs not to implement IDisposable nor reference ElapsedEventArgs.
/// Intended for fire-and-forget uses.
/// </summary>
public class BackgroundTimer : IDisposable
{
    private readonly Timer timer = new();
    
    /// <summary>
    /// Describes whether autosave is enabled or not.
    /// </summary>
    public bool Enabled
    {
        get => timer.Enabled;
        set => timer.Enabled = value;
    }

    /// <summary>
    /// The frequency of autosave in milliseconds.
    /// </summary>
    public double Interval
    {
        get => timer.Interval;
        set => timer.Interval = value;
    }

    private Action? elapsedAction;
    private ElapsedEventHandler? elapsed;
    /// <summary>
    /// Action to execute every <see cref="Interval"/>.
    /// This 
    /// </summary>
    public Action? ElapsedAction
    {
        get => elapsedAction;
        set
        {
            timer.Elapsed -= elapsed;

            elapsedAction = value;
            elapsed = (object? sender, ElapsedEventArgs e) => value?.Invoke();

            timer.Elapsed += elapsed;
        }
    }

    public void Dispose()
    {
        ((IDisposable)timer).Dispose();
        GC.SuppressFinalize(this);
    }
}
