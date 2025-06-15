using Microsoft.EntityFrameworkCore;
using ScanEventsWorker.Data;
using ScanEventsWorker.Interfaces;
using ScanEventsWorker.Models.Enum;
using ScanEventsWorker.Models;

namespace ScanEventsWorker.Services
{
    public class ParcelService(ScanEventsContext context, ILogger<ParcelService> logger) : IParcelService
    {
        private readonly ScanEventsContext _context = context;
        private readonly ILogger<ParcelService> _logger = logger;

        #region Public Methods

        public async Task ProcessAndSaveParcelAsync(ScanEvent scanEvent)
        {
            _logger.LogDebug("Processing EventId {EventId} for ParcelId {ParcelId}, Type: {Type}", scanEvent.EventId, scanEvent.ParcelId, scanEvent.Type);

            var parcel = await _context.Parcels.FindAsync(scanEvent.ParcelId) ?? new Parcel { Id = scanEvent.ParcelId };

            if (parcel.LastEventTime == null || scanEvent.CreatedDateTimeUtc > parcel.LastEventTime)
            {
                parcel.LastEventType = scanEvent.Type;
                parcel.LastEventTime = scanEvent.CreatedDateTimeUtc;
                _logger.LogDebug("Updated last event for ParcelId {ParcelId}: Type = {Type}, Time = {Time}", parcel.Id, parcel.LastEventType, parcel.LastEventTime);
            }

            if (scanEvent.Type == ScanEventType.PICKUP)
            {
                parcel.PickupTime = scanEvent.CreatedDateTimeUtc;
                _logger.LogInformation("Set PickupTime for ParcelId {ParcelId} to {PickupTime}", parcel.Id, parcel.PickupTime);
            }

            if (scanEvent.Type == ScanEventType.DELIVERY)
            {
                parcel.DeliveryTime = scanEvent.CreatedDateTimeUtc;
                _logger.LogInformation("Set DeliveryTime for ParcelId {ParcelId} to {DeliveryTime}", parcel.Id, parcel.DeliveryTime);
            }

            if (!string.IsNullOrEmpty(scanEvent.User?.RunId))
            {
                parcel.LastRunId = scanEvent.User.RunId;
            }

            if (_context.Entry(parcel).State == EntityState.Detached)
            {
                _context.Parcels.Add(parcel);
                _logger.LogInformation("Added new parcel record for ParcelId {ParcelId}.", parcel.Id);
            }
            else
            {
                _context.Update(parcel);
                _logger.LogDebug("Updated existing parcel record for ParcelId {ParcelId}.", parcel.Id);
            }
        }

        #endregion
    }
}