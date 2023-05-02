namespace IIoT.Domain.Shared.Businesses.Roots.Networks;
public interface INetworkOpcUa : ITacticExpert
{
    const string Banner = "opcua";
    const string Scheme = "opc.tcp";
    ValueTask InstallAsync();
    Task<NetworkGroup> GetAsync(Guid id);
    Task<Contingent> GetContingentAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();

    [StructLayout(LayoutKind.Auto)]
    readonly record struct NetworkGroup
    {
        public required Entity Entity { get; init; }
        public required INetwork.Entity Network { get; init; }
    }
    readonly record struct Contingent
    {
        public required Entity Entity { get; init; }
        public required IEnumerable<IEquipment.Entity> Equipments { get; init; }
    }

    [Table(Name = $"{Deputy.Root}_{INetwork.Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "endpoint")] public required string Endpoint { get; init; }
        [Field(Name = "username")] public required string Username { get; init; }
        [Field(Name = "password")] public required string Password { get; init; }
    }
    string TableName { get; init; }
}