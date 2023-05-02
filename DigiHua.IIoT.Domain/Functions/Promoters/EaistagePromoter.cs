namespace IIoT.Domain.Functions.Promoters;
internal sealed class EaistagePromoter : IEaistagePromoter
{
    event EventHandler EventHandler;
    public EaistagePromoter()
    {
        EventHandler = (sender, @event) =>
        {
            switch (@event)
            {
                case IEaistagePromoter.HostEventArgs host:
                    if (host.Message != string.Empty)
                    {
                        if (!Histories.Contains(host.Message))
                        {
                            Customer!.Information(Morse.HistoryDefault, host.Environment, new
                            {
                                host.Url,
                                host.Message
                            });
                            Histories.Add(host.Message);
                        }
                    }
                    else
                    {
                        if (Histories.Any()) Histories.Clear();
                    }
                    break;

                case IEaistagePromoter.MessageEventArgs message:
                    switch (message.EaiType)
                    {
                        case IWorkshopRawdata.EaiType.Information:
                            Equipment!.Information(Morse.HistoryTimer, message.ConsumeMS.NeatlyClock(), new
                            {
                                message.Eendpoint,
                                message.Request,
                                message.Response
                            });
                            break;

                        case IWorkshopRawdata.EaiType.Parameter:
                            Parameter!.Information(Morse.HistoryTimer, message.ConsumeMS.NeatlyClock(), new
                            {
                                message.Eendpoint,
                                message.Request,
                                message.Response
                            });
                            break;

                        case IWorkshopRawdata.EaiType.Production:
                            Production!.Information(Morse.HistoryTimer, message.ConsumeMS.NeatlyClock(), new
                            {
                                message.Eendpoint,
                                message.Request,
                                message.Response
                            });
                            break;
                    }
                    break;
            }
        };
        Customer ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, IntegrationRoot.Joint(Eaiservice), "smes.host.information", "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: DispersionRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        Equipment ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, IntegrationRoot.Joint(Eaiservice), IWorkshopRawdata.EaiType.Information.GetDesc(), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: DispersionRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        Parameter ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, IntegrationRoot.Joint(Eaiservice), IWorkshopRawdata.EaiType.Parameter.GetDesc(), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: DispersionRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        Production ??= new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Logger(item => item.Filter.ByIncludingOnly(item =>
            item.Level is LogEventLevel.Information).WriteTo.File(string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot, IntegrationRoot.Joint(Eaiservice), IWorkshopRawdata.EaiType.Production.GetDesc(), "/"
            }).Joint(Morse.HistoryExtension),
            restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour,
            outputTemplate: DispersionRecord, retainedFileCountLimit: 100, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
    }
    public void OnLatest(in IEaistagePromoter.HostEventArgs @event) => EventHandler.Invoke(default, @event);
    public void OnLatest(in IEaistagePromoter.MessageEventArgs @event) => EventHandler.Invoke(default, @event);
    public required List<string> Histories { get; init; } = new();
    static string DispersionRecord => "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}{NewLine}{Exception}";
    static string IntegrationRoot => "Integrations";
    static string Eaiservice => "EAI";
    ILogger Customer { get; init; }
    ILogger Equipment { get; init; }
    ILogger Parameter { get; init; }
    ILogger Production { get; init; }
}