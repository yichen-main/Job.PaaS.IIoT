using static IIoT.Domain.Shared.Businesses.Workshops.Missions.IMission;

namespace IIoT.Domain.Businesses.Workshops.Missions;
internal sealed class Mission : TacticExpert, IMission
{
    readonly INpgsqlUtility _npgsqlUtility;
    public Mission(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.EquipmentId).FieldInfo<Entity>().Name, TableName<IEquipment.Entity>(), CurrentSign)
            },
            Combos = new[]
            {
                (LinkCategoryType, new List<string>
                {
                    nameof(Entity.EquipmentId).FieldInfo<Entity>().Name,
                    nameof(Entity.CategoryType).FieldInfo<Entity>().Name
                })
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity, IMissionPush.Entity mission) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkInsert<Entity>(), entity),
        (_npgsqlUtility.MarkInsert<IMissionPush.Entity>(), mission)
    });
    public async ValueTask UpdateAsync(Entity entity, IMissionPush.Entity mission) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.EquipmentId),
            nameof(Entity.CategoryType),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<IMissionPush.Entity>(new[]
        {
            nameof(IMissionPush.Entity.OperateType),
            nameof(IMissionPush.Entity.EnvironmentType)
        }), mission)
    });
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.CategoryType),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public async Task<SingleMission> GetSingleMissionAsync(Guid id)
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.EquipmentId),
                    nameof(Entity.CategoryType),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), id),
                _npgsqlUtility.MarkQuery<IMissionPush.Entity>(new[]
                {
                    nameof(IMissionPush.Entity.Id),
                    nameof(IMissionPush.Entity.OperateType),
                    nameof(IMissionPush.Entity.EnvironmentType)
                }).AddEqualFilter(nameof(IMissionPush.Entity.Id).To<Entity>(), id),
                _npgsqlUtility.MarkQuery<IEquipment.Entity>(new[]
                {
                    nameof(IEquipment.Entity.Id),
                    nameof(IEquipment.Entity.GroupId),
                    nameof(IEquipment.Entity.NetworkId),
                    nameof(IEquipment.Entity.OperateType),
                    nameof(IEquipment.Entity.EquipmentNo),
                    nameof(IEquipment.Entity.EquipmentName),
                    nameof(IEquipment.Entity.Creator),
                    nameof(IEquipment.Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Entity = await result.ReadSingleOrDefaultAsync<Entity>(),
                Push = await result.ReadSingleOrDefaultAsync<IMissionPush.Entity>(),
                Equipments = await result.ReadAsync<IEquipment.Entity>()
            };
        }
        return default;
    }
    public async Task<MultipleMission> GetMultipleMissionAsync()
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.EquipmentId),
                    nameof(Entity.CategoryType),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).ToString(),
                _npgsqlUtility.MarkQuery<IMissionPush.Entity>(new[]
                {
                    nameof(IMissionPush.Entity.Id),
                    nameof(IMissionPush.Entity.OperateType),
                    nameof(IMissionPush.Entity.EnvironmentType)
                }).ToString(),
                _npgsqlUtility.MarkQuery<IEquipment.Entity>(new[]
                {
                    nameof(IEquipment.Entity.Id),
                    nameof(IEquipment.Entity.GroupId),
                    nameof(IEquipment.Entity.NetworkId),
                    nameof(IEquipment.Entity.OperateType),
                    nameof(IEquipment.Entity.EquipmentNo),
                    nameof(IEquipment.Entity.EquipmentName),
                    nameof(IEquipment.Entity.Creator),
                    nameof(IEquipment.Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Entities = await result.ReadAsync<Entity>(),
                Pushs = await result.ReadAsync<IMissionPush.Entity>(),
                Equipments = await result.ReadAsync<IEquipment.Entity>()
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.CategoryType),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}