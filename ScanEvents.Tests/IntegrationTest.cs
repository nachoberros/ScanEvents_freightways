using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using ScanEventsWorker.Data;
using ScanEventsWorker.Models;
using ScanEventsWorker.Models.Enum;
using ScanEventsWorker.Services;
using ScanEvents.Tests.Helpers;

namespace ScanEvents.Tests
{

    [TestFixture]
    public class IntegrationTest : IDisposable
    {
        private ScanEventsContext _dbContext;
        private ScanEventService _scanEventService;
        private ParcelService _parcelService;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory DB
            var options = new DbContextOptionsBuilder<ScanEventsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ScanEventsContext(options);

            // Mock logger
            var scanEventLogger = new Mock<ILogger<ScanEventService>>();
            var parcelLogger = new Mock<ILogger<ParcelService>>();

            // Mock configuration
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ScanEventsApi:BaseUrl"]).Returns("http://fakeapi.local");
            configMock.Setup(c => c["ScanEventsApi:Limit"]).Returns("100");

            // Setup HttpClient with fake response
            var fakeScanEvents = new ScanEventResponse { ScanEvents = [new() { EventId = 3001, ParcelId = 6001, Type = ScanEventType.PICKUP, CreatedDateTimeUtc = DateTime.UtcNow }] };

            var json = JsonSerializer.Serialize(fakeScanEvents);
            var handler = new MockHttpMessageHandler(json);
            var httpClient = new HttpClient(handler);

            _scanEventService = new ScanEventService(httpClient, _dbContext, configMock.Object, scanEventLogger.Object);
            _parcelService = new ParcelService(_dbContext, parcelLogger.Object);
        }

        [Test]
        public async Task FullTest_ShouldFetchProcessAndSave()
        {
            // Act
            var scanEvents = await _scanEventService.FetchScanEventsAsync();

            Assert.That(scanEvents, Is.Not.Null);

            foreach (var scanEvent in scanEvents)
            {
                await _parcelService.ProcessAndSaveParcelAsync(scanEvent);
                await _scanEventService.SaveScanEventAsync(scanEvent);
            }

            // Assert
            var parcel = await _dbContext.Parcels.FindAsync(6001);
            Assert.That(parcel, Is.Not.Null);
            Assert.That(parcel.LastEventType, Is.EqualTo(ScanEventType.PICKUP));

            var tracker = await _dbContext.ScanEvents.FirstOrDefaultAsync();
            Assert.That(tracker, Is.Not.Null);
            Assert.That(tracker.EventId, Is.EqualTo(3001));
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}