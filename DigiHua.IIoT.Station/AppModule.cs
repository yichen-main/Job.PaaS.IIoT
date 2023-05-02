namespace IIoT.Station;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpAspNetCoreModule), typeof(IIoTApplicationModule))]
internal sealed class AppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RunnerText.Organization.LogLevel.UseRecord(RollingInterval.Day, Floor.Identification, 10);
        context.Services.AddControllers(item =>
        {
            item.ReturnHttpNotAcceptable = true;
            item.Filters.Add<ExceptionFilter>();
        }).ConfigureApiBehaviorOptions(item =>
        {
            item.SuppressModelStateInvalidFilter = default;
            item.InvalidModelStateResponseFactory = context =>
            {
                List<string> results = new();
                results.AddRange(Refresher());
                ProblemResult result = new()
                {
                    Message = string.Join(",\u00A0", results)
                };
                return new UnprocessableEntityObjectResult(result)
                {
                    ContentTypes = { MediaTypeNames.Application.Json }
                };
                IEnumerable<string> Refresher()
                {
                    foreach (var entry in context.ModelState.Root.Children ?? Enumerable.Empty<ModelStateEntry>())
                    {
                        for (int i = default; i < entry.Errors.Count; i++) yield return entry.Errors[i].ErrorMessage;
                    }
                }
            };
        }).AddNewtonsoftJson(item =>
        {
            item.SerializerSettings.DateFormatString = Converter.DefaultSeconds;
            item.SerializerSettings.NullValueHandling = NullValueHandling.Include;
        }).AddMvcOptions(item => item.Conventions.Add(new ModelConvention())).AddControllersAsServices();

        context.Services.AddAuthentication(Morse.DigiHua).AddScheme<AuthenticateOption, AuthenticateHandler>(Morse.DigiHua, configureOptions: default);
        context.Services.AddCors(item => item.AddDefaultPolicy(item => item.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*")));
     
        context.Services.AddHostedService<ExecutorGuard>();
        context.Services.AddHostedService<InitializerGuard>();
        context.Services.AddHostedService<ManufactureGuard>();
        context.Services.Configure<FormOptions>(item =>
        {
            item.ValueLengthLimit = int.MaxValue;
            item.MemoryBufferThreshold = int.MaxValue;
            item.MultipartBodyLengthLimit = long.MaxValue;
        });
    }
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var service = context.GetApplicationBuilder();
        service.UseSchedulers();
        service.UseEndpoints(item =>
        {
            item.UseSoapEndpoint<IPlatformerService>(SturdyExpansion.Concat(new[]
            {
                "/", string.Join("/", new[] { Morse.Title, IManufactureClient.Label.Name }), ".svc"
            }), new[]
            {
                new SoapEncoderOptions
                {
                    WriteEncoding = Encoding.UTF8,
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxStringContentLength = int.MaxValue
                    }
                }
            }, SoapSerializer.DataContractSerializer);
            item.UseSoapEndpoint<IPlatformerService>(SturdyExpansion.Concat(new[]
            {
                "/", string.Join("/", new[] { Morse.Title, IManufactureClient.Label.Name }), ".asmx"
            }), new[]
            {
                new SoapEncoderOptions
                {
                    WriteEncoding = Encoding.UTF8,
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxStringContentLength = int.MaxValue
                    }
                }
            }, SoapSerializer.XmlSerializer);
        });
    }
}