using static IIoT.Domain.Shared.Businesses.Roots.Equipments.IEquipmentAlarm;

namespace IIoT.Domain.Businesses.Roots.Equipments;
internal sealed class EquipmentAlarm : TacticExpert, IEquipmentAlarm
{
    readonly INpgsqlUtility _npgsqlUtility;
    public EquipmentAlarm(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.EquipmentId).FieldInfo<Entity>().Name, TableName<IEquipment.Entity>(), CurrentSign)
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>(), entity, Morse.Passer);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ExceptionCode),
        nameof(Entity.Description),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ExceptionCode),
        nameof(Entity.Description),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ExceptionCode),
        nameof(Entity.Description),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.EquipmentId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}