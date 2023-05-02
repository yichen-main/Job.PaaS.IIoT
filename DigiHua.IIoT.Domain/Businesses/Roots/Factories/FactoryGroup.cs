using static IIoT.Domain.Shared.Businesses.Roots.Factories.IFactoryGroup;

namespace IIoT.Domain.Businesses.Roots.Factories;
internal sealed class FactoryGroup : TacticExpert, IFactoryGroup
{
    readonly INpgsqlUtility _npgsqlUtility;
    public FactoryGroup(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.FactoryId).FieldInfo<Entity>().Name, TableName<IFactory.Entity>(), CurrentSign)
            },
            Uniques = new[]
            {
                nameof(Entity.GroupNo).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity, Morse.Passer);
    public async ValueTask UpdateAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkUpdate<Entity>(new[]
    {
        nameof(Entity.FactoryId),
        nameof(Entity.GroupNo),
        nameof(Entity.GroupName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }), entity, Morse.Passer);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryId),
        nameof(Entity.GroupNo),
        nameof(Entity.GroupName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<Entity> GetAsync(string groupNo) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryId),
        nameof(Entity.GroupNo),
        nameof(Entity.GroupName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.GroupNo).To<Entity>(), nameof(groupNo)), new
    {
        groupNo
    }, Morse.Passer);
    public async Task<SingleFactory> GetSingleFactoryAsync(Guid id)
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
                    nameof(Entity.FactoryId),
                    nameof(Entity.GroupNo),
                    nameof(Entity.GroupName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), id),
                _npgsqlUtility.MarkQuery<IFactory.Entity>(new[]
                {
                    nameof(IFactory.Entity.Id),
                    nameof(IFactory.Entity.FactoryNo),
                    nameof(IFactory.Entity.FactoryName),
                    nameof(IFactory.Entity.Creator),
                    nameof(IFactory.Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            var entity = await result.ReadSingleAsync<Entity>();
            var factories = await result.ReadAsync<IFactory.Entity>();
            return new()
            {
                Entity = entity,
                Factory = factories.First(item => item.Id == entity.FactoryId)
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryId),
        nameof(Entity.GroupNo),
        nameof(Entity.GroupName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListFactoryAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryId),
        nameof(Entity.GroupNo),
        nameof(Entity.GroupName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.FactoryId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public async IAsyncEnumerable<(IFactory.Entity factory, Entity group)> ListFactoryGroupAsync()
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<IFactory.Entity>(new[]
                {
                    nameof(IFactory.Entity.Id),
                    nameof(IFactory.Entity.FactoryNo),
                    nameof(IFactory.Entity.FactoryName),
                    nameof(IFactory.Entity.Creator),
                    nameof(IFactory.Entity.CreateTime)
                }).ToString(),
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.FactoryId),
                    nameof(Entity.GroupNo),
                    nameof(Entity.GroupName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            var factories = await result.ReadAsync<IFactory.Entity>();
            foreach (var entity in await result.ReadAsync<Entity>()) yield return (factories.First(item => item.Id == entity.FactoryId), entity);
        }
    }
    public async IAsyncEnumerable<(INetwork.Entity network, IEquipment.Entity equipment)> ListNetworkEquipmentAsync(Guid id)
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<INetwork.Entity>(new[]
                {
                    nameof(INetwork.Entity.Id),
                    nameof(INetwork.Entity.CategoryType),
                    nameof(INetwork.Entity.SessionNo),
                    nameof(INetwork.Entity.SessionName),
                    nameof(INetwork.Entity.Creator),
                    nameof(INetwork.Entity.CreateTime)
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
                }).AddEqualFilter(nameof(IEquipment.Entity.GroupId).To<IEquipment.Entity>(), id)
            }.DelimitMark(Delimiter.Finish));
            var networks = await result.ReadAsync<INetwork.Entity>();
            foreach (var equipment in await result.ReadAsync<IEquipment.Entity>()) yield return (networks.First(item => item.Id == equipment.NetworkId), equipment);
        }
    }
    public string TableName { get; init; } = TableName<Entity>();
}