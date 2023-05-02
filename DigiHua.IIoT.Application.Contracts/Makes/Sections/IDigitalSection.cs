namespace IIoT.Application.Contracts.Makes.Sections;
public interface IDigitalSection
{
    Task<Session?> OpenAsync(Guid sessionId, string sessionNo, INetworkOpcUa.Entity entity);
    Subscription AddLink(in Session entity, in IWorkshopRawdata.Title title);
    Subscription AddItem(in Subscription entity, in IEnumerable<Formula> formulas);
    ValueTask BuildAsync(IEquipment.Entity entity, Subscription subscription, List<Formula> formulas, CancellationToken stoppingToken);
    ValueTask RemoveLinkAsync(Guid sessionId, Guid equipmentId);
    void RemoveItem(in Guid sessionId, in Guid equipmentId);
    void Clear(in Guid sessionId);
    readonly record struct Title
    {
        public required string SessionNo { get; init; }
        public required string Endpoint { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required Session Entity { get; init; }
    }
    readonly record struct Formula
    {
        public required Guid EquipmentId { get; init; }
        public required Guid EstablishId { get; init; }
        public required Guid ProcessId { get; init; }
        public required string DataNo { get; init; }
        public required string NodePath { get; init; }
        public required IWorkshopRawdata.EaiType EaiType { get; init; }
    }
    List<Guid> Scavengers { get; init; }
    ConcurrentDictionary<Guid, Title> Mains { get; init; }
    ConcurrentDictionary<Guid, (string equipmentNo, Subscription entity)> Links { get; init; }
    ConcurrentDictionary<(Guid sessionId, Guid equipmentId), List<Formula>> Providers { get; init; }
}