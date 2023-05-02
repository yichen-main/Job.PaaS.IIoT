namespace IIoT.Application.Contracts.Wrappers;
public interface IMakeMessageWrapper
{
    IEntranceTrigger<AthenaMedium.Organization, JObject> Organization { get; }
    IEntranceTrigger<(RollingInterval interval, DateTimeOffset start, DateTimeOffset end), JObject> Electricity { get; }
}