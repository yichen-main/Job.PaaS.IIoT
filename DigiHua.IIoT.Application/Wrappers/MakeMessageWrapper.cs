using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Application.Wrappers;

[Dependency(ServiceLifetime.Singleton)]
file sealed class MakeMessageWrapper : IMakeMessageWrapper
{
    public IEntranceTrigger<AthenaMedium.Organization, JObject> Organization => new OrganizationMessage();
    public IEntranceTrigger<(RollingInterval interval, DateTimeOffset start, DateTimeOffset end), JObject> Electricity => new ElectricityMessage(Fielder);
    public required IStringLocalizer<Fielder> Fielder { get; init; }
}