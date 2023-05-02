namespace IIoT.Domain.Shared.Businesses.Roots.Networks;
public interface INetworkMqtt : ITacticExpert
{
    const string Banner = "mqtt";
    ValueTask InstallAsync();
    Task<NetworkGroup> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    enum Customer
    {
        AlibabaCloudIoT = 101
    }

    [StructLayout(LayoutKind.Auto)]
    readonly record struct NetworkGroup
    {
        public required Entity Entity { get; init; }
        public required INetwork.Entity Network { get; init; }
    }

    [Table(Name = $"{Deputy.Root}_{INetwork.Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "customer_type")] public required Customer CustomerType { get; init; }
        [Field(Name = "ip")] public required string Ip { get; init; }
        [Field(Name = "port")] public required int Port { get; init; }
        [Field(Name = "username")] public required string Username { get; init; }
        [Field(Name = "password")] public required string Password { get; init; }
    }
    string TableName { get; init; }
    static string LinkAddress => $"{Banner}_{nameof(Entity.Ip).To<Entity>()}_{nameof(Entity.Port).To<Entity>()}_{Deputy.ComboLink}";
}