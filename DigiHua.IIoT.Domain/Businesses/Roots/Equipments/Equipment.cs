using static IIoT.Domain.Shared.Businesses.Roots.Equipments.IEquipment;

namespace IIoT.Domain.Businesses.Roots.Equipments;
internal sealed class Equipment : TacticExpert, IEquipment
{
    readonly INpgsqlUtility _npgsqlUtility;
    public Equipment(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.GroupId).FieldInfo<Entity>().Name, TableName<IFactoryGroup.Entity>(), CurrentSign),
                (nameof(Entity.NetworkId).FieldInfo<Entity>().Name, TableName<INetwork.Entity>(), CurrentSign)
            },
            Uniques = new[]
            {
                nameof(Entity.EquipmentNo).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async Task<bool> IsExistEquipmentAsync(string equipmentNo)
    {
        var result = await GetAsync(equipmentNo);
        return result.EquipmentNo == equipmentNo;
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity, Morse.Passer);
    public async ValueTask<Entity> AddAsync(string equipmentNo, Guid groupId, Guid networkId)
    {
        var entity = await GetAsync(equipmentNo);
        if (entity.Id == default)
        {
            await AddAsync(new Entity
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                NetworkId = networkId,
                OperateType = Operate.Enable,
                EquipmentNo = equipmentNo,
                EquipmentName = string.Empty,
                Creator = ITacticExpert.Automatic,
                CreateTime = DateTime.UtcNow
            });
            return await GetAsync(equipmentNo);
        }
        return entity;
    }
    public async ValueTask UpdateAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkUpdate<Entity>(new[]
    {
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }), entity, Morse.Passer);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public async Task<Entity> GetAsync(string equipmentNo) => await SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.EquipmentNo).To<Entity>(), nameof(equipmentNo)), new
    {
        equipmentNo
    }, Morse.Passer);
    public async Task<INetwork.Entity> GetNetworkAsync(Guid equipmentId)
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
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.GroupId),
                    nameof(Entity.NetworkId),
                    nameof(Entity.OperateType),
                    nameof(Entity.EquipmentNo),
                    nameof(Entity.EquipmentName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), equipmentId)
            }.DelimitMark(Delimiter.Finish));
            var networks = await result.ReadAsync<INetwork.Entity>();
            var entity = await result.ReadSingleAsync<Entity>();
            return networks.First(item => item.Id == entity.NetworkId);
        }
        return default;
    }
    public async Task<ScavengerScope> GetScavengerAsync(Guid equipmentId)
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<IMission.Entity>(new[]
                {
                    nameof(IMission.Entity.Id),
                    nameof(IMission.Entity.EquipmentId),
                    nameof(IMission.Entity.CategoryType),
                    nameof(IMission.Entity.Creator),
                    nameof(IMission.Entity.CreateTime)
                }).AddEqualFilter(nameof(IMission.Entity.EquipmentId).To<IMission.Entity>(), equipmentId),
                _npgsqlUtility.MarkQuery<IEquipmentAlarm.Entity>(new[]
                {
                    nameof(IEquipmentAlarm.Entity.Id),
                    nameof(IEquipmentAlarm.Entity.EquipmentId),
                    nameof(IEquipmentAlarm.Entity.ExceptionCode),
                    nameof(IEquipmentAlarm.Entity.Description),
                    nameof(IEquipmentAlarm.Entity.CreateTime)
                }).AddEqualFilter(nameof(IEquipmentAlarm.Entity.EquipmentId).To<IEquipmentAlarm.Entity>(), equipmentId)
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Missions = await result.ReadAsync<IMission.Entity>(),
                Alarms = await result.ReadAsync<IEquipmentAlarm.Entity>()
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListGroupAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.GroupId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListNetworkAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.GroupId),
        nameof(Entity.NetworkId),
        nameof(Entity.OperateType),
        nameof(Entity.EquipmentNo),
        nameof(Entity.EquipmentName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.NetworkId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public async IAsyncEnumerable<FactoryNetworkScope> ListFactoryNetworkAsync(Guid networkId)
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
                    nameof(Entity.GroupId),
                    nameof(Entity.NetworkId),
                    nameof(Entity.OperateType),
                    nameof(Entity.EquipmentNo),
                    nameof(Entity.EquipmentName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).ToString(),
                _npgsqlUtility.MarkQuery<IFactoryGroup.Entity>(new[]
                {
                    nameof(IFactoryGroup.Entity.Id),
                    nameof(IFactoryGroup.Entity.FactoryId),
                    nameof(IFactoryGroup.Entity.GroupNo),
                    nameof(IFactoryGroup.Entity.GroupName),
                    nameof(IFactoryGroup.Entity.Creator),
                    nameof(IFactoryGroup.Entity.CreateTime)
                }).ToString(),
                _npgsqlUtility.MarkQuery<INetwork.Entity>(new[]
                {
                    nameof(INetwork.Entity.Id),
                    nameof(INetwork.Entity.CategoryType),
                    nameof(INetwork.Entity.SessionNo),
                    nameof(INetwork.Entity.SessionName),
                    nameof(INetwork.Entity.Creator),
                    nameof(INetwork.Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            var entitiesAsync = result.ReadAsync<Entity>();
            var groupsAsync = result.ReadAsync<IFactoryGroup.Entity>();
            var networks = await result.ReadAsync<INetwork.Entity>();
            var groups = await groupsAsync;
            foreach (var entity in await entitiesAsync)
            {
                if (entity.NetworkId == networkId) yield return new()
                {
                    Entity = entity,
                    Group = groups.First(item => item.Id == entity.GroupId),
                    Network = networks.First(item => item.Id == entity.NetworkId)
                };
            }
        }
    }
    public string TableName { get; init; } = TableName<Entity>();
}