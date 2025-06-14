using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ScanEventsWorker.Data;
using ScanEventsWorker.Models.Enum;
using ScanEventsWorker.Models;
using ScanEventsWorker.Services;
using Microsoft.Extensions.Configuration;
using ScanEvents.Tests.Helpers;

namespace ScanEvents.Tests
{
    public class ScanEventServiceTests : IDisposable
    {
        private ScanEventsContext _dbContext;
        private ScanEventService _scanEventService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ScanEventsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ScanEventsContext(options);

            var loggerMock = new Mock<ILogger<ScanEventService>>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ScanEventsApi:BaseUrl"]).Returns("http://localhost");
            configMock.Setup(c => c["ScanEventsApi:Limit"]).Returns("100");

            var httpClient = new HttpClient(new MockHttpMessageHandler("[]")); // Empty array response
            _scanEventService = new ScanEventService(httpClient, _dbContext, configMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task SaveScanEventAsync_ShouldUpdateScanEvents()
        {
            var scanEvent = new ScanEvent
            {
                EventId = 5000,
                ParcelId = 321,
                CreatedDateTimeUtc = DateTime.UtcNow,
                Type = ScanEventType.PICKUP
            };

            await _scanEventService.SaveScanEventAsync(scanEvent);

            var savedScanEvent = await _dbContext.ScanEvents.FirstOrDefaultAsync();
            Assert.That(savedScanEvent, Is.Not.Null);
            Assert.That(savedScanEvent.EventId, Is.EqualTo(scanEvent.EventId));
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
