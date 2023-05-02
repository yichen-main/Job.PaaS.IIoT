namespace Pier.Neltron;

[DependsOn(typeof(AbpAutofacModule))]
internal sealed class AppModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHostedService<KeyenceGuard>();
    }
}