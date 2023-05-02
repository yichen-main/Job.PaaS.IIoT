using static IIoT.Domain.Shared.Businesses.Roots.Equipments.IProduceState;

namespace IIoT.Domain.Businesses.Roots.Equipments;
internal sealed class ProduceState : TacticExpert, IProduceState
{
    readonly INpgsqlUtility _npgsqlUtility;
    public ProduceState(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public async ValueTask UpsertAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.EquipmentNo).To<Entity>(), new[]
    {
        nameof(Entity.EquipmentName).AddExcluded<Entity>(),
        nameof(Entity.EquipmentStatus).AddExcluded<Entity>(),
        nameof(Entity.EnvironmentType).AddExcluded<Entity>(),
        nameof(Entity.Description).AddExcluded<Entity>(),
        nameof(Entity.CreateTime).AddExcluded<Entity>()
    }.DelimitMark()), entity, Morse.Passer);
    public async ValueTask UpsertAsync(IEnumerable<Entity> entities)
    {
        List<(string content, object? @object)> results = new();
        foreach (var entity in entities) results.Add((_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.EquipmentNo).To<Entity>(), new[]
        {
            nameof(Entity.EquipmentName).AddExcluded<Entity>(),
            nameof(Entity.EquipmentStatus).AddExcluded<Entity>(),
            nameof(Entity.EnvironmentType).AddExcluded<Entity>(),
            nameof(Entity.Description).AddExcluded<Entity>(),
            nameof(Entity.CreateTime).AddExcluded<Entity>()
        }.DelimitMark()), entity));
        await TransactionAsync(results);
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.EquipmentStatus),
        nameof(Entity.EnvironmentType),
        nameof(Entity.Description),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}