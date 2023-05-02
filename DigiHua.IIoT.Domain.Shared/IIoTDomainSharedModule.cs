namespace IIoT.Domain.Shared;

[DependsOn(typeof(AbpLocalizationModule), typeof(AbpLocalizationModule))]
public class IIoTDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(item => item.FileSets.AddEmbedded<IIoTDomainSharedModule>(Assembly.GetExecutingAssembly().GetName().Name?.Replace(nameof(Morse.DigiHua).Joint(), string.Empty)));
        Configure<AbpLocalizationOptions>(item =>
        {
            item.Resources.Add<Fielder>(Language).AddVirtualJson($"/{string.Join("/", new string[]
            {
                nameof(Languages), nameof(Languages.Fielders)
            })}");
            item.Resources.Add<Search>(Language).AddVirtualJson($"/{string.Join("/", new string[]
            {
                nameof(Languages), nameof(Languages.Searches)
            })}");
            item.Resources.Add<Terminology>(Language).AddVirtualJson($"/{string.Join("/", new string[]
            {
                nameof(Languages), nameof(Languages.Terminologies)
            })}");
        });
    }
}