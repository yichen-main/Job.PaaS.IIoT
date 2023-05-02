namespace IIoT.Domain.Functions.Promoters;
internal sealed class CollectPromoter : ICollectPromoter
{
    event EventHandler Handler;
    public CollectPromoter()
    {
        Handler ??= (sender, @event) =>
        {
            switch (@event)
            {
                case ICollectPromoter.BackgroundEventArgs background:
                    Background!.Information(Morse.HistoryTimer, background.ConsumeTime.NeatlyClock(), new
                    {
                        background.Name,
                        background.Store,
                        background.Detail,
                        background.Trace
                    });
                    break;

                case ICollectPromoter.CollectiveEventArgs collective:
                    Collective!.Information(Morse.HistoryDefault, collective.Title, new
                    {
                        collective.Burst,
                        collective.Detail,
                        collective.Trace
                    });
                    break;

                case ICollectPromoter.NativeQueueEventArgs nativeQueue:
                    NativeQueue!.Information(Morse.HistoryDefault, nativeQueue.Type, new
                    {
                        nativeQueue.Topic,
                        nativeQueue.Payload
                    });
                    break;
            }
        };
        Background ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, MissionRoot.Joint(nameof(Background)), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: SingleRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        Collective ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, MissionRoot.Joint(nameof(Collective)), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: SingleRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        NativeQueue ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, MissionRoot.Joint(nameof(NativeQueue)), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: SingleRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
    }
    public void OnLatest(in ICollectPromoter.BackgroundEventArgs @event) => Handler.Invoke(default, @event);
    public void OnLatest(in ICollectPromoter.CollectiveEventArgs @event) => Handler.Invoke(default, @event);
    public void OnLatest(in ICollectPromoter.NativeQueueEventArgs @event) => Handler.Invoke(default, @event);
    static string SingleRecord => "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
    static string MissionRoot => "Missions";
    ILogger Background { get; init; }
    ILogger Collective { get; init; }
    ILogger NativeQueue { get; init; }
}