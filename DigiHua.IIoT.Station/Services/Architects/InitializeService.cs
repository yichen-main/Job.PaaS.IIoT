namespace IIoT.Station.Services.Architects;
public static class InitializeService
{
    internal static void AddValidator(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<Factories.FactoryInsert>, Factories.FactoryInsert.Validator>();
        services.AddSingleton<IValidator<Factories.FactoryUpdate>, Factories.FactoryUpdate.Validator>();
        services.AddSingleton<IValidator<Groups.GroupInsert>, Groups.GroupInsert.Validator>();
        services.AddSingleton<IValidator<Groups.GroupUpdate>, Groups.GroupUpdate.Validator>();
        services.AddSingleton<IValidator<Networks.NetworkInsert>, Networks.NetworkInsert.Validator>();
        services.AddSingleton<IValidator<Networks.NetworkUpdate>, Networks.NetworkUpdate.Validator>();
        services.AddSingleton<IValidator<Equipments.EquipmentInsert>, Equipments.EquipmentInsert.Validator>();
        services.AddSingleton<IValidator<Equipments.EquipmentUpdate>, Equipments.EquipmentUpdate.Validator>();
        services.AddSingleton<IValidator<Logics.MissionInsert>, Logics.MissionInsert.Validator>();
        services.AddSingleton<IValidator<Logics.MissionUpdate>, Logics.MissionUpdate.Validator>();
        services.AddSingleton<IValidator<Parameters.ProcessInsert>, Parameters.ProcessInsert.Validator>();
        services.AddSingleton<IValidator<Parameters.ProcessUpdate>, Parameters.ProcessUpdate.Validator>();
    }
    public static ValueTask AddEnvironmentAsync(this Assembly assembly)
    {
        Console.CursorVisible = default;
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        Floor.Identification = assembly.GetName().Name ?? string.Empty;
        Console.Title = string.Format("{0} v{1}", Floor.Identification,
        FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion);
        Output(string.Join(Environment.NewLine, new string[]
        {
            string.Concat("　Hostname        =>   　", Dns.GetHostName()),
            string.Concat("　Username        =>   　", Environment.UserName),
            string.Concat("　Language        =>   　", Thread.CurrentThread.CurrentCulture.IetfLanguageTag),
            string.Concat("　Internet        =>   　", NetworkInterface.GetIsNetworkAvailable()),
            string.Concat("　.NET Version    =>   　", Environment.Version),
            string.Concat("　IPv4 Physical   =>   　", NetworkInterfaceType.Ethernet.AddLocalIPv4()),
            string.Concat("　IPv4 Wireless   =>   　", NetworkInterfaceType.Wireless80211.AddLocalIPv4()),
            string.Concat("　OS Environment  =>   　", Environment.OSVersion),
            string.Concat("　File Location   =>   　", RootLocation)
        }), ConsoleColor.Yellow);
        Output(new[]
        {
            Figgle.FiggleFonts.Standard.Render(Morse.DigiHua.ToUpper().Aggregate(string.Empty.PadLeft(6, '\u00A0'),
            (first, second) => string.Concat(first, second, string.Empty.PadLeft(3, '\u00A0')))),
            new string('*', 80), Environment.NewLine
        }.Concat(), ConsoleColor.White);
        Directory.SetCurrentDirectory(RootLocation);
        DirectoryInfo kernel = new(RootLocation);
        {
            kernel.Create();
            kernel.CreateSubdirectory(Morse.EmbedsRoot);
        }
        DirectoryInfo external = new(ExternalPath);
        {
            external.Create();
            external.CreateSubdirectory(Morse.ConfigureRoot);
        }
        return new();
        static void Output(string content, ConsoleColor consoleColor = ConsoleColor.White)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(content);
        }
    }
    public static void UseRecord(this LogEventLevel level, RollingInterval rolling, string name, int quantity)
    {
        var template = "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
        .MinimumLevel.Override("Default", LogEventLevel.Error)
        .MinimumLevel.Override("System", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
        .MinimumLevel.Override("Quartz", LogEventLevel.Error)
        .MinimumLevel.Override("IdentityServer4", LogEventLevel.Error)
        .MinimumLevel.Override("Volo.Abp.Core", LogEventLevel.Error)
        .MinimumLevel.Override("Volo.Abp.Autofac", LogEventLevel.Error)
        .MinimumLevel.Override("Volo.Abp.AspNetCore", LogEventLevel.Error)
        .Enrich.FromLogContext().WriteTo.Async(item =>
        {
            item.Console(restrictedToMinimumLevel: level, outputTemplate: template, theme: SystemConsoleTheme.Literate);
            item.Logger(item => item.Filter.ByIncludingOnly(item => item.Level is LogEventLevel.Debug).WriteTo.File(
            path: Initializer(Position(), Morse.HistoryExtension, name, LogEventLevel.Debug),
            restrictedToMinimumLevel: level, outputTemplate: template, rollingInterval: rolling,
            retainedFileCountLimit: quantity, encoding: Encoding.UTF8));
            item.Logger(item => item.Filter.ByIncludingOnly(item => item.Level is LogEventLevel.Information).WriteTo.File(
            path: Initializer(Position(), Morse.HistoryExtension, name, LogEventLevel.Information),
            restrictedToMinimumLevel: level, outputTemplate: template, rollingInterval: rolling,
            retainedFileCountLimit: quantity, encoding: Encoding.UTF8));
            item.Logger(item => item.Filter.ByIncludingOnly(item => item.Level is LogEventLevel.Warning).WriteTo.File(
            path: Initializer(Position(), Morse.HistoryExtension, name, LogEventLevel.Warning),
            restrictedToMinimumLevel: level, outputTemplate: template, rollingInterval: rolling,
            retainedFileCountLimit: quantity, encoding: Encoding.UTF8));
            item.Logger(item => item.Filter.ByIncludingOnly(item => item.Level is LogEventLevel.Error).WriteTo.File(
            path: Initializer(Position(), Morse.HistoryExtension, name, LogEventLevel.Error),
            restrictedToMinimumLevel: level, outputTemplate: template, rollingInterval: rolling,
            retainedFileCountLimit: quantity, encoding: Encoding.UTF8));
            item.Logger(item => item.Filter.ByIncludingOnly(item => item.Level is LogEventLevel.Fatal).WriteTo.File(
            path: Initializer(Position(), Morse.HistoryExtension, name, LogEventLevel.Fatal),
            restrictedToMinimumLevel: level, outputTemplate: template, rollingInterval: rolling,
            retainedFileCountLimit: quantity, encoding: Encoding.UTF8));
        }, blockWhenFull: default).CreateLogger();
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, @event) =>
        Log.Fatal(Morse.HistoryDefault, nameof(UnhandledExceptionEventArgs), new
        {
            Detail = @event.ExceptionObject.ToString()
        }));
        static string Position() => new[]
        {
            string.Join("/", new string[]
            {
                ".".Joint(), Morse.HistoryRoot
            }), "/"
        }.Concat();
        static string Initializer(string root, string extension, string name, LogEventLevel level) => string.Join("/", new string[]
        {
            root, name, level.ToString(), "/"
        }).Joint(extension);
    }
    internal static async ValueTask BuildAsync(this ValueTask task)
    {
        UseMarquee();
        await task.ConfigureAwait(default);
    }
    internal static string GetName(this ControllerModel model) => model.ControllerType.Assembly.GetName().Name!.Replace(".", "/");
    internal static void UseSchedulers(this IApplicationBuilder app) => Array.ForEach(app.ApplicationServices.GetRequiredService<IEntranceTrigger[]>(), item => Task.Run(() => item.PushAsync()));
    internal static IApplicationBuilder UseSymbolizer(this IApplicationBuilder app) => app.UseMiddleware<Symbolizer>();
    public record Header
    {
        [FromHeader(Name = Head.Language)] public string? Language { get; init; }
    }
    sealed class Symbolizer
    {
        readonly RequestDelegate _request;
        public Symbolizer(RequestDelegate request) => _request = request;
        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add(Head.FrameOption, "*");
            context.Response.Headers.Add(Head.AllowOrigin, "*");
            context.Response.Headers.Add(Head.AllowHeader, "*");
            context.Response.Headers.Add(Head.AllowMethod, "*");
            context.Response.Headers.Add(Head.ExposeHeader, "*");
            context.Response.Headers.Add(Head.AllowCredential, "true");
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await _request(context);
        }
    }
    internal static string? Nameplate { get; set; }
    internal static IRunnerProfile.Text RunnerText { get; set; } = new()
    {
        Organization = new()
        {
            Debug = true,
            CompanyID = "99",
            CompanyName = Morse.DigiHua,
            SiteID = "DSCTC",
            SiteName = "DSCTC",
            LogLevel = LogEventLevel.Error,
            Language = Thread.CurrentThread.CurrentCulture.IetfLanguageTag
        },
        Platform = new()
        {
            Entrance = 17770,
            Mqbroker = 1883,
            Username = Morse.DigiHua,
            Password = Morse.DigiHua,
            Hash = Guid.NewGuid().ToString("N")
        }
    };
    internal static IManagerProfile.Text ManagerText { get; set; } = new()
    {
        Assembly = new()
        {
            PushCycle = Mark.Found.ToString(),
            ExperiBlock = $"{Uri.UriSchemeHttp}://{BrokerIp}/SMES_Test_MESws_EAI/wsEAI.asmx",
            FormalBlock = $"{Uri.UriSchemeHttp}://{BrokerIp}/SMES_Production_MESws_EAI/wsEAI.asmx",
            CollectBlock = $"{Uri.UriSchemeHttp}://{BrokerIp}:8094/manage/index.html#/manage/taskAdd"
        },
        Hangar = new()
        {
            Merchant = "5432",
            Flowmeter = "8086",
            Identifier = Morse.DigiHua,
            Pespond = $"{Morse.DigiHua}@123",
            Plaque = Morse.Title,
            Location = IPAddress.Loopback.ToString()
        }
    };
}