using ScanEventsWorker.Models.Enum;

namespace ScanEventsWorker.Models
{
    public class Parcel
    {
        public int Id { get; set; }
        public ScanEventType LastEventType { get; set; }
        public DateTime? LastEventTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string? LastRunId { get; set; }
    }
}
