namespace IIoT.Terminal;

[DependsOn(typeof(AbpAutofacModule))]
internal sealed class AppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        LogEventLevel.Information.UseRecord(RollingInterval.Day, Identification, 10);
        {
            context.Services.AddHostedService<WatchService>();
        }
    }
}