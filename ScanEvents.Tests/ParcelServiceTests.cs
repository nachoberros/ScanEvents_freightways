using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ScanEventsWorker.Data;
using ScanEventsWorker.Models.Enum;
using ScanEventsWorker.Models;
using ScanEventsWorker.Services;

namespace ScanEvents.Tests
{
    public class ParcelServiceTests : IDisposable
    {
        private ScanEventsContext _dbContext;
        private ParcelService _parcelService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ScanEventsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ScanEventsContext(options);

            var loggerMock = new Mock<ILogger<ParcelService>>();
            _parcelService = new ParcelService(_dbContext, loggerMock.Object);
        }

        [Test]
        public async Task ProcessAndSaveParcelAsync_ShouldCreateParcel()
        {
            var scanEvent = new ScanEvent
            {
                ParcelId = 101,
                EventId = 1,
                CreatedDateTimeUtc = DateTime.UtcNow,
                Type = ScanEventType.PICKUP
            };

            await _parcelService.ProcessAndSaveParcelAsync(scanEvent);

            var parcel = await _dbContext.Parcels.FindAsync(101);
            Assert.That(parcel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(parcel.PickupTime, Is.EqualTo(scanEvent.CreatedDateTimeUtc));
                Assert.That(parcel.LastEventType, Is.EqualTo(ScanEventType.PICKUP));
            });
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
