using static IIoT.Domain.Shared.Businesses.Roots.Equipments.IOpcUaProcess;

namespace IIoT.Domain.Businesses.Roots.Equipments;
internal sealed class OpcUaProcess : TacticExpert, IOpcUaProcess
{
    readonly INpgsqlUtility _npgsqlUtility;
    public OpcUaProcess(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Combos = new[]
            {
                (LinkNode, new List<string>
                {
                    nameof(Entity.EquipmentId).FieldInfo<Entity>().Name,
                    nameof(Entity.NodePath).FieldInfo<Entity>().Name
                })
            }
        }), default, enable: true);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.NodePath)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.NodePath)
    }).AddObjectFilter(nameof(Entity.EquipmentId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}