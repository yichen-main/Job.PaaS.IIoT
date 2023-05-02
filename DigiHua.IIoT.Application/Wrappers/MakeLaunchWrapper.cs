using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Application.Wrappers;

[Dependency(ServiceLifetime.Singleton)]
file sealed class MakeLaunchWrapper : IMakeLaunchWrapper
{
    public IMetadataLaunch Metadata => new MetadataLaunch(RegisterTrigger, WorkshopRawdata, BusinessManufacture);
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IWorkshopRawdata WorkshopRawdata { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}