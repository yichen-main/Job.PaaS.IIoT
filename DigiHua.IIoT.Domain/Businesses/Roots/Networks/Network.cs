using static IIoT.Domain.Shared.Businesses.Roots.Networks.INetwork;

namespace IIoT.Domain.Businesses.Roots.Networks;
internal sealed class Network : TacticExpert, INetwork
{
    readonly INpgsqlUtility _npgsqlUtility;
    public Network(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Uniques = new[]
            {
                nameof(Entity.SessionNo).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity, Morse.Passer);
    public async ValueTask AddAsync(Entity entity, INetworkMqtt.Entity detail) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkInsert<Entity>(), entity), (_npgsqlUtility.MarkInsert<INetworkMqtt.Entity>(), detail)
    });
    public async ValueTask AddAsync(Entity entity, INetworkOpcUa.Entity detail) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkInsert<Entity>(), entity), (_npgsqlUtility.MarkInsert<INetworkOpcUa.Entity>(), detail)
    });
    public async ValueTask UpdateAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkUpdate<Entity>(new[]
    {
        nameof(Entity.CategoryType),
        nameof(Entity.SessionName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }), entity, Morse.Passer);
    public async ValueTask UpdateAsync(Entity entity, (Guid id, string table) remove) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (remove.table.UseDelete(), new { remove.id })
    });
    public async ValueTask UpdateAsync(Entity entity, INetworkMqtt.Entity detail) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<INetworkMqtt.Entity>(new[]
        {
            nameof(INetworkMqtt.Entity.CustomerType),
            nameof(INetworkMqtt.Entity.Ip),
            nameof(INetworkMqtt.Entity.Port),
            nameof(INetworkMqtt.Entity.Username),
            nameof(INetworkMqtt.Entity.Password)
        }), detail)
    });
    public async ValueTask UpdateAsync(Entity entity, INetworkOpcUa.Entity detail) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<INetworkOpcUa.Entity>(new[]
        {
            nameof(INetworkOpcUa.Entity.Endpoint),
            nameof(INetworkOpcUa.Entity.Username),
            nameof(INetworkOpcUa.Entity.Password)
        }), detail)
    });
    public async ValueTask UpdateAsync(Entity entity, INetworkMqtt.Entity insert, (Guid id, string table) remove) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),
        (_npgsqlUtility.MarkInsert<INetworkMqtt.Entity>(), insert),
        (remove.table.UseDelete(), new { remove.id })
    });
    public async ValueTask UpdateAsync(Entity entity, INetworkOpcUa.Entity insert, (Guid id, string table) remove) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),
        (_npgsqlUtility.MarkInsert<INetworkOpcUa.Entity>(), insert),
        (remove.table.UseDelete(), new { remove.id })
    });
    public async ValueTask UpdateChangeAsync(Entity entity, INetworkMqtt.Entity insert) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),
        (_npgsqlUtility.MarkInsert<INetworkMqtt.Entity>(), insert)
    });
    public async ValueTask UpdateChangeAsync(Entity entity, INetworkOpcUa.Entity insert) => await TransactionAsync(new (string content, object? @object)[]
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.CategoryType),
            nameof(Entity.SessionName),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),
        (_npgsqlUtility.MarkInsert<INetworkOpcUa.Entity>(), insert)
    });
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.CategoryType),
        nameof(Entity.SessionNo),
        nameof(Entity.SessionName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<Entity> GetAsync(string sessionNo) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.CategoryType),
        nameof(Entity.SessionNo),
        nameof(Entity.SessionName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.SessionNo).To<Entity>(), nameof(sessionNo)), new
    {
        sessionNo
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.CategoryType),
        nameof(Entity.SessionNo),
        nameof(Entity.SessionName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync(Category category) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.CategoryType),
        nameof(Entity.SessionNo),
        nameof(Entity.SessionName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddEqualFilter(nameof(Entity.CategoryType).To<Entity>(), (int)category), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}