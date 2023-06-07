using NLog;
using NLog.Targets;
using System.Threading;
using System.Threading.Tasks;

namespace rowsSharp.ViewModel;

public class SplashVM : NotifyPropertyChanged
{
    private const int RefreshLogDelay = 500; // ms

    private string log = "";
    public string Log
    {
        get => log;
        set => SetField(ref log, value);
    }

    public SplashVM(CancellationToken token)
    {
        Task.Run(() => BackgroundTask(token), token);
    }

    private void BackgroundTask(CancellationToken token)
    {
        var target = LogManager.Configuration.FindTargetByName<MemoryTarget>("Memory");
        while (true)
        {
            Task.Delay(RefreshLogDelay, token).Wait(token);
            Log = target.Logs[0];
            if (token.IsCancellationRequested) { return; }
        }
    }
}
