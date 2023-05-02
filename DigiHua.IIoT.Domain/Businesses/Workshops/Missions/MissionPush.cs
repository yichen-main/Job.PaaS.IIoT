using static IIoT.Domain.Shared.Businesses.Workshops.Missions.IMissionPush;

namespace IIoT.Domain.Businesses.Workshops.Missions;
internal sealed class MissionPush : TacticExpert, IMissionPush
{
    readonly INpgsqlUtility _npgsqlUtility;
    public MissionPush(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public Task<int> GaugeAsync() => CountAsync(nameof(Entity.Id).To<Entity>().AddTotalCount<Entity>().ToString(), Morse.Passer);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.OperateType),
        nameof(Entity.EnvironmentType)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.OperateType),
        nameof(Entity.EnvironmentType)
    }).ToString(), default, Morse.Passer);
    public async Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id)
    {
        List<Entity> results = new();
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
                }).AddEqualFilter(nameof(IMission.Entity.EquipmentId).To<IMission.Entity>(), id),
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.OperateType),
                    nameof(Entity.EnvironmentType)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            var missions = await result.ReadAsync<IMission.Entity>();
            foreach (var entity in await result.ReadAsync<Entity>())
            {
                if (missions.Any(item => item.Id == entity.Id)) results.Add(new()
                {
                    Id = entity.Id,
                    OperateType = entity.OperateType,
                    EnvironmentType = entity.EnvironmentType
                });
            }
        }
        return results;
    }
    public string TableName { get; init; } = TableName<Entity>();
}