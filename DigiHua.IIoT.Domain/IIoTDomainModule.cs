namespace IIoT.Domain;

[DependsOn(typeof(IIoTDomainSharedModule))]
public class IIoTDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<INpgsqlUtility, NpgsqlElement>();
        context.Services.AddSingleton<IRegisterTrigger, RegisterTrigger>();
        context.Services.AddSingleton<IWorkshopRawdata, WorkshopRawdata>();
        context.Services.AddSingleton<ICollectPromoter, CollectPromoter>();
        context.Services.AddSingleton<IEaistagePromoter, EaistagePromoter>();
        context.Services.AddSingleton<IFoundationTrigger, FoundationTrigger>();
    }
}