namespace IIoT.Storage;

[DependsOn(typeof(AbpAutofacModule), typeof(IIoTDomainModule))]
internal sealed class AppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        LogEventLevel.Information.UseRecord(RollingInterval.Month, Identification, 10);
        context.Services.AddSingleton<IEntranceTrigger, AtomicEntrance>();
        context.Services.AddSingleton<IEntranceTrigger, JanitorEntrance>();
    }
}