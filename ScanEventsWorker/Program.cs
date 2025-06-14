using Microsoft.EntityFrameworkCore;
using ScanEventsWorker.Data;
using ScanEventsWorker.Interfaces;
using ScanEventsWorker.Services;
using ScanEventsWorker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ScanEventsContext>(options =>
    options.UseSqlite("Data Source=parcels.db"));

builder.Services.AddScoped<IScanEventService, ScanEventService>();
builder.Services.AddScoped<IParcelService, ParcelService>();
builder.Services.AddHttpClient<ScanEventService>();
builder.Services.AddHostedService<ScanEventWorker>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScanEventsContext>();
    db.Database.EnsureCreated();
}
app.Run();

