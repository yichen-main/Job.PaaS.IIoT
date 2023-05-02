namespace IIoT.Application;

[DependsOn(typeof(IIoTDomainModule))]
public sealed class IIoTApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IQueueSection, QueueSection>();
        context.Services.AddSingleton<IClearerEvent, ClearerErrand>();
        context.Services.AddSingleton<IAlibabaService, AlibabaMessage>();
        context.Services.AddSingleton<IDigitalSection, DigitalSection>();
        context.Services.AddSingleton<IPlatformerService, PlatformerMessage>();
    }
}