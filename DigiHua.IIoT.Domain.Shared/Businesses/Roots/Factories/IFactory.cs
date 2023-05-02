namespace IIoT.Domain.Shared.Businesses.Roots.Factories;
public interface IFactory : ITacticExpert
{
    const string Type = "factory";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity);
    ValueTask UpdateAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);
    Task<Entity> GetAsync(string factoryNo);
    Task<SingleGroup> GetColonyAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<SingleGroup>> ListColonyAsync();
    readonly record struct SingleGroup
    {
        public required Entity Factory { get; init; }
        public required IEnumerable<IFactoryGroup.Entity> Groups { get; init; }
    }

    [Table(Name = $"{Deputy.Root}_{Type}_{Deputy.Foundation}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "factory_no")] public required string FactoryNo { get; init; }
        [Field(Name = "factory_name")] public required string FactoryName { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}