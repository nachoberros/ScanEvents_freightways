using ScanEventsWorker.Models;

namespace ScanEventsWorker.Interfaces
{
    public interface IScanEventService
    {
        Task<List<ScanEvent>?> FetchScanEventsAsync();
        Task SaveScanEventAsync(ScanEvent scanEvent);
    }
}
