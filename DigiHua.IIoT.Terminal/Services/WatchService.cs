namespace IIoT.Terminal.Services;
internal sealed class WatchService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await new PeriodicTimer(TimeSpan.FromSeconds(5)).WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                //var setup = await new ProfileService().ReadAsync();
                {
                    //LoadMount = setup.LoadMount;
                    //StoreUsername = setup.Architecture.Username;
                    //StorePassword = setup.Architecture.Password;
                    //BrokerIp = setup.Architecture.BrokerIp;
                }
            }
            catch (Exception e)
            {
                Log.Error(Morse.HistoryDefault, nameof(WatchService).Joint(nameof(ExecuteAsync)), new
                {
                    Detail = e.Message
                });
            }
        }
    }
}