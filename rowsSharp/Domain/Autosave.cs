using System.Timers;

namespace rowsSharp.Domain;

public class Autosave
{
    private readonly Timer timer;
    
    /// <summary>
    /// Describes whether autosave is enabled or not.
    /// </summary>
    public bool Enabled
    {
        get => timer.Enabled;
        set => timer.Enabled = value;
    }

    /// <summary>
    /// The frequency of autosave in milliseconds
    /// </summary>
    public double Interval
    {
        get => timer.Interval;
        set => timer.Interval = value;
    }

    /// <summary>
    /// Event handler triggered every <see cref="Interval"/>
    /// </summary>
    private ElapsedEventHandler elapsed;
    public ElapsedEventHandler Elapsed
    {
        get => elapsed;
        set
        {
            timer.Elapsed -= elapsed;
            timer.Elapsed += value;
            elapsed = value;
        }
    }

    public Autosave(ElapsedEventHandler elapsed)
    {
        timer = new();
        this.elapsed = elapsed;
        Elapsed = elapsed;
    }
}
