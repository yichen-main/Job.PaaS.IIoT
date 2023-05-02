namespace IIoT.Station.Services.Runners;
internal sealed class ManufactureGuard : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await new PeriodicTimer(TimeSpan.FromSeconds(5)).WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (Morse.Passer && Morse.Meter) await Task.WhenAll(new[]
                {
                    ManufactureEvent.QueueBrokerAsync(),
                    ManufactureEvent.DigitalTwinAsync(),
                    ManufactureEvent.ConfidentialAsync(IMissionPush.EnvironmentType.Experiment),
                    ManufactureEvent.ConfidentialAsync(IMissionPush.EnvironmentType.Production)
                });
            }
            catch (Exception e)
            {
                Log.Fatal(Morse.HistoryDefault, nameof(ManufactureGuard), new
                {
                    e.Message,
                    e.StackTrace
                });
            }
        }
    }
    public required IManufactureEvent ManufactureEvent { get; init; }
}