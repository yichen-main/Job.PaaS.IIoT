using static IIoT.Application.Contracts.Makes.Sections.IQueueSection;

namespace IIoT.Application.Makes.Sections;
internal sealed class QueueSection : IQueueSection
{
    public void Clear(in Guid sessionId)
    {
        if (Providers.Remove(sessionId, out var session)) session.entity.Dispose();
    }
    public async Task OpenAsync(INetworkMqtt.Customer type, Guid sessionId, string sessionNo, Formula formula)
    {
        try
        {
            Clear(sessionId);
            var entity = new MqttFactory().CreateMqttClient();
            entity.ConnectedAsync += (@event) => Task.CompletedTask;
            entity.DisconnectedAsync += (@event) =>
            {
                Clear(sessionId);
                return Task.CompletedTask;
            };
            switch (type)
            {
                case INetworkMqtt.Customer.AlibabaCloudIoT:
                    await AlibabaAttach.PullAsync(sessionNo, formula, entity);
                    break;
            }
            Providers.TryAdd(sessionId, (type, sessionNo, formula, entity));
        }
        catch (Exception e)
        {
            CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
            {
                Title = nameof(QueueSection).Joint(nameof(OpenAsync)),
                Burst = type.ToString().Joint(formula.SessionsNo),
                Detail = formula.Ip.Joint(formula.Port.ToString(), ":"),
                Trace = e.Message
            });
        }
    }
    public required IAlibabaService AlibabaAttach { get; init; }
    public required ICollectPromoter CollectPromoter { get; init; }
    public required ConcurrentDictionary<Guid, (INetworkMqtt.Customer type, string sessionNo, Formula formula, IMqttClient entity)> Providers { get; init; } = new();
}