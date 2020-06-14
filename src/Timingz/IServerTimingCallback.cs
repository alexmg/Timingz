using System.Threading.Tasks;

namespace Timingz
{
    public interface IServerTimingCallback
    {
        Task OnServerTiming(ServerTimingEvent serverTimingEvent);
    }
}