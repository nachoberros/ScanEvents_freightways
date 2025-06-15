using Microsoft.EntityFrameworkCore;
using ScanEventsWorker.Data;
using ScanEventsWorker.Interfaces;
using ScanEventsWorker.Models;
using ScanEventsWorker.Models.Enum;
using System.Net.Http.Json;

namespace ScanEventsWorker.Services
{
    public class ScanEventService(HttpClient httpClient, ScanEventsContext context, IConfiguration configuration, ILogger<ScanEventService> logger) : IScanEventService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ScanEventsContext _context = context;
        private readonly ILogger<ScanEventService> _logger = logger;
        private readonly string _baseUrl = configuration["ScanEventsApi:BaseUrl"] ?? throw new ArgumentNullException("ScanEventsApi:BaseUrl");
        private readonly int _limit = int.Parse(configuration["ScanEventsApi:Limit"] ?? "100");

        #region Public Methods

        public async Task<List<ScanEvent>?> FetchScanEventsAsync()
        {
            int? lastEventId = await GetLastEventIdAsync();

            _logger.LogInformation("Fetching scan events from remote API...");

            var eventsResult = await FetchEventsAsync(lastEventId);
            if (eventsResult == null || eventsResult.ScanEvents == null || eventsResult.ScanEvents.Count == 0)
            {
                _logger.LogInformation("No scan events returned from the API.");
                return null;
            }

            if (eventsResult.ScanEvents.Any(e => e.Type == ScanEventType.UNKOWN))
            {
                _logger.LogError("One or more events are of an unknown type. This batch must be reprocessed.");
                return null;
            }

            _logger.LogInformation("Fetched {Count} scan events.", eventsResult.ScanEvents.Count);

            return eventsResult.ScanEvents;
        }

        public async Task SaveScanEventAsync(ScanEvent scanEvent)
        {
            var existingEvent = await _context.ScanEvents.FirstOrDefaultAsync(e => e.EventId == scanEvent.EventId);

            if (existingEvent != null)
            {
                _logger.LogWarning("One or more events have already been processed and should not have been received.");
                return;
            }
            else
            {
                await _context.ScanEvents.AddAsync(scanEvent);
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Private Methods

        private async Task<int?> GetLastEventIdAsync()
        {
            var scanEvent = await _context.ScanEvents
                .OrderByDescending(e => e.EventId)
                .FirstOrDefaultAsync();

            return scanEvent?.EventId;
        }

        private async Task<ScanEventResponse?> FetchEventsAsync(int? lastEventId)
        {
            var fromEventId = lastEventId?.ToString() ?? "0";
            string url = $"{_baseUrl}?FromEventId={fromEventId}&Limit={_limit}";

            try
            {
                return await _httpClient.GetFromJsonAsync<ScanEventResponse>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch scan events from API.");
                return null;
            }
        }

        #endregion
    }
}