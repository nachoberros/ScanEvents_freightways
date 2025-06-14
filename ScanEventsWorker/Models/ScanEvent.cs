using ScanEventsWorker.Helpers;
using ScanEventsWorker.Models.Enum;
using System.Text.Json.Serialization;

namespace ScanEventsWorker.Models
{
    public class ScanEvent
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ParcelId { get; set; }
        [JsonConverter(typeof(SafeEnumConverter<ScanEventType>))]
        public ScanEventType Type { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public string? StatusCode { get; set; }
        public Device? Device { get; set; }
        public User? User { get; set; }
    }
}
