namespace Timingz;

public interface IServerTimingCallback
{
    Task OnServerTiming(ServerTimingEvent serverTimingEvent);
}