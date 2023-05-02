namespace IIoT.Application.Contracts.Architects.Events;
public interface IManufactureEvent
{
    Task QueueBrokerAsync();
    Task DigitalTwinAsync();
    Task ConfidentialAsync(IMissionPush.EnvironmentType type);
}