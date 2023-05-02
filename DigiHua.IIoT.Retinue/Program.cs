try
{
    await Assembly.GetExecutingAssembly().AddEnvironmentAsync().BuildAsync();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = RootLocation,
        EnvironmentName = Environments.Staging,
        ApplicationName = typeof(Program).Assembly.FullName
    });
    builder.Host.AddAppSettingsSecretsJson().UseAutofac().UseSystemd().UseSerilog();
    builder.WebHost.UseKestrel(item =>
    {
        item.ListenAnyIP(RunnerText.Native.Port);
    }).ConfigureKestrel((context, item) => item.Limits.MaxRequestBodySize = long.MaxValue);
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddLogging(item => item.AddSerilog());
        builder.Services.AddSoapCore();
        builder.Services.AddSoapExceptionTransformer(item => item.Message);
    }
    await builder.AddApplicationAsync<AppModule>(item => item.PlugInSources.AddFolder(string.Join("/", new[]
    {
        RootLocation, Morse.EmbedsRoot
    })));
    var service = builder.Build();
    {
        service.UseInternetArchitecture();
        service.UseSerilogRequestLogging();
        service.UseRouting();
        service.UseCors();
        service.UseAuthentication();
        service.UseAuthorization();
        await service.RunAsync();
    }
}
catch (Exception e)
{
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