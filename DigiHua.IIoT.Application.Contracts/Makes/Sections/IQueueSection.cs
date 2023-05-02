namespace IIoT.Application.Contracts.Makes.Sections;
public interface IQueueSection
{
    void Clear(in Guid sessionId);
    Task OpenAsync(INetworkMqtt.Customer type, Guid sessionId, string sessionNo, Formula formula);
    readonly record struct Formula
    {
        public required string Ip { get; init; }
        public required int Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string SessionsNo { get; init; }
    }
    ConcurrentDictionary<Guid, (INetworkMqtt.Customer type, string sessionNo, Formula formula, IMqttClient entity)> Providers { get; init; }
}