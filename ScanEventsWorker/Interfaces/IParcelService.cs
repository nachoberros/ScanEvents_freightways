using ScanEventsWorker.Models;

namespace ScanEventsWorker.Interfaces
{
    public interface IParcelService
    {
        Task ProcessAndSaveParcelAsync(ScanEvent scanEvent);
    }
}
