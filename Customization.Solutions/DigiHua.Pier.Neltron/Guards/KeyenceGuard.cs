namespace Pier.Neltron.Guards;
internal sealed class KeyenceGuard : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Setup = await new KeyenceProfile().ReadAsync();
        var frequency = Setup.Global.Frequency;
        {
            PeriodicTimer periodic = new(TimeSpan.FromSeconds(frequency));
            while (await periodic.WaitForNextTickAsync(stoppingToken))
            {
                var watch = Stopwatch.StartNew();
                ICollectPromoter.BackgroundEventArgs background = new()
                {
                    Name = nameof(KeyenceGuard).Joint(nameof(ExecuteAsync))
                };
                try
                {
                    //Setup = await new KeyenceProfile().ReadAsync();
                    {
                        if (Setup.Global.Enable)
                        {
                            KeyenceParser parser = new(RemoteManufacture);
                            {
                                //await Parallel.ForEachAsync(parser.RealAsync(Setup.Main), async (item, _) =>
                                //{
                                //    //results.Add(Task.Run(async () =>
                                //    //await parser.PushAsync(Setup, region.BranchPath, item), stoppingToken));
                                //});
                                if (Histories.Any()) Histories.Clear();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    background.Detail = e.Message;
                    background.Trace = e.StackTrace ?? string.Empty;
                }
                finally
                {
                    watch.Stop();
                    background.ConsumeTime = watch.ElapsedMilliseconds;
                    {
                        if (background.Detail != string.Empty && !Histories.Contains(background.Detail) || background.ConsumeTime > frequency * 1000)
                        {
                            //background.OnLatest();
                            {
                                Histories.Add(background.Detail);
                            }
                        }
                        if (frequency != Setup.Global.Frequency) periodic.Dispose();
                    }
                }
            }
        }
        await RestartAsync(stoppingToken);
    }
    async Task RestartAsync(CancellationToken stoppingToken) => await ExecuteAsync(stoppingToken);
    public required List<string> Histories { get; init; } = new();
    KeyenceParser.Setup Setup { get; set; } = new();
    public required IRemoteManufactureWrapper RemoteManufacture { get; init; }
}