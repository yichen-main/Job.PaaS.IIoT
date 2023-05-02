using static IIoT.Domain.Shared.Businesses.Roots.Networks.INetworkOpcUa;

namespace IIoT.Domain.Businesses.Roots.Networks;
internal sealed class NetworkOpcUa : TacticExpert, INetworkOpcUa
{
    readonly INpgsqlUtility _npgsqlUtility;
    public NetworkOpcUa(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Uniques = new[]
            {
                nameof(Entity.Endpoint).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async Task<NetworkGroup> GetAsync(Guid id)
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
                    nameof(Entity.Endpoint),
                    nameof(Entity.Username),
                    nameof(Entity.Password)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), id),
                _npgsqlUtility.MarkQuery<INetwork.Entity>(new[]
                {
                    nameof(INetwork.Entity.Id),
                    nameof(INetwork.Entity.CategoryType),
                    nameof(INetwork.Entity.SessionNo),
                    nameof(INetwork.Entity.SessionName),
                    nameof(INetwork.Entity.Creator),
                    nameof(INetwork.Entity.CreateTime)
                }).AddEqualFilter(nameof(INetwork.Entity.Id).To<INetwork.Entity>(), id)
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Entity = await result.ReadSingleOrDefaultAsync<Entity>(),
                Network = await result.ReadSingleOrDefaultAsync<INetwork.Entity>()
            };
        }
        return default;
    }
    public async Task<Contingent> GetContingentAsync(Guid id)
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
                    nameof(Entity.Endpoint),
                    nameof(Entity.Username),
                    nameof(Entity.Password)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), id),
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
                }).AddEqualFilter(nameof(IEquipment.Entity.NetworkId).To<IEquipment.Entity>(), id)
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Entity = await result.ReadSingleAsync<Entity>(),
                Equipments = await result.ReadAsync<IEquipment.Entity>()
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.Endpoint),
        nameof(Entity.Username),
        nameof(Entity.Password)
    }).ToString(), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}