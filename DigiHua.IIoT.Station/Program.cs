try
{
    await Assembly.GetExecutingAssembly().AddEnvironmentAsync().BuildAsync();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = RootLocation,
        EnvironmentName = Environments.Staging,
        ApplicationName = typeof(Program).Assembly.FullName
    });
    builder.Host.ConfigureHostOptions(item => item.ShutdownTimeout = TimeSpan.FromMinutes(10)).AddAppSettingsSecretsJson().UseAutofac().UseSystemd().UseSerilog();
    builder.WebHost.UseKestrel(item => item.ListenAnyIP(RunnerText.Platform.Entrance)).ConfigureKestrel((context, item) => item.Limits.MaxRequestBodySize = long.MaxValue);
    builder.Services.AddSwaggerGen(item =>
    {
        Array.ForEach(typeof(Domain).GetEnumNames().ToArray(), domain => item.SwaggerDoc(domain, new OpenApiInfo
        {
            Version = domain,
            Title = nameof(IIoT).Joint(nameof(Program)),
            Description = nameof(Morse.DigiHua).Joint(domain)
        }));
        item.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, Floor.Identification.Joint(nameof(Extension.Xml).ToLower())), includeControllerXmlComments: true);
    });
    builder.Services.AddValidator();
    builder.Services.AddMemoryCache();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddLogging(item => item.AddSerilog());
    await builder.AddApplicationAsync<AppModule>(item => item.PlugInSources.AddFolder(string.Join("/", new[]
    {
        RootLocation, Morse.EmbedsRoot
    })));
    var apply = builder.Build();
    apply.UseSwaggerUI(item =>
    {
        Array.ForEach(typeof(Domain).GetEnumNames().ToArray(), swagger =>
        item.SwaggerEndpoint($"/{nameof(swagger)}/{swagger}/{nameof(swagger)}".Joint(nameof(Extension.Json).ToLower()), swagger));
        item.RoutePrefix = string.Empty;
    });
    apply.UseRouting();
    apply.UseCors();
    apply.UseSwagger();
    apply.UseSymbolizer();
    apply.UseAuthentication();
    apply.UseAuthorization();
    apply.MapControllers();
    apply.UseSerilogRequestLogging();
    await apply.InitializeApplicationAsync();
    await apply.RunAsync();
}
catch (Exception e)
{
    await WaitAsync();
    Log.Fatal(Morse.HistoryDefault, nameof(Program), new
    {
        e.Message,
        e.StackTrace
    });
}
finally
{
    Log.CloseAndFlush();
}