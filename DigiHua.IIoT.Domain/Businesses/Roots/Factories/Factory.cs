using static IIoT.Domain.Shared.Businesses.Roots.Factories.IFactory;

namespace IIoT.Domain.Businesses.Roots.Factories;
internal sealed class Factory : TacticExpert, IFactory
{
    readonly INpgsqlUtility _npgsqlUtility;
    public Factory(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Uniques = new[]
            {
                nameof(Entity.FactoryNo).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity, Morse.Passer);
    public async ValueTask UpdateAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkUpdate<Entity>(new[]
    {
        nameof(Entity.FactoryNo),
        nameof(Entity.FactoryName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }), entity, Morse.Passer);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryNo),
        nameof(Entity.FactoryName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<Entity> GetAsync(string factoryNo) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryNo),
        nameof(Entity.FactoryName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.FactoryNo).To<Entity>(), nameof(factoryNo)), new
    {
        factoryNo
    }, Morse.Passer);
    public async Task<SingleGroup> GetColonyAsync(Guid id)
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
                    nameof(Entity.FactoryNo),
                    nameof(Entity.FactoryName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), id),
                _npgsqlUtility.MarkQuery<IFactoryGroup.Entity>(new[]
                {
                    nameof(IFactoryGroup.Entity.Id),
                    nameof(IFactoryGroup.Entity.FactoryId),
                    nameof(IFactoryGroup.Entity.GroupNo),
                    nameof(IFactoryGroup.Entity.GroupName),
                    nameof(IFactoryGroup.Entity.Creator),
                    nameof(IFactoryGroup.Entity.CreateTime)
                }).AddEqualFilter(nameof(IFactoryGroup.Entity.FactoryId).To<IFactoryGroup.Entity>(), id)
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Factory = await result.ReadSingleOrDefaultAsync<Entity>(),
                Groups = await result.ReadAsync<IFactoryGroup.Entity>()
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.FactoryNo),
        nameof(Entity.FactoryName),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public async Task<IEnumerable<SingleGroup>> ListColonyAsync()
    {
        List<SingleGroup> results = new();
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<IFactoryGroup.Entity>(new[]
                {
                    nameof(IFactoryGroup.Entity.Id),
                    nameof(IFactoryGroup.Entity.FactoryId),
                    nameof(IFactoryGroup.Entity.GroupNo),
                    nameof(IFactoryGroup.Entity.GroupName),
                    nameof(IFactoryGroup.Entity.Creator),
                    nameof(IFactoryGroup.Entity.CreateTime)
                }).ToString(),
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.FactoryNo),
                    nameof(Entity.FactoryName),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).ToString()
            }.DelimitMark(Delimiter.Finish));
            var groups = await result.ReadAsync<IFactoryGroup.Entity>();
            foreach (var entity in await result.ReadAsync<Entity>()) results.Add(new()
            {
                Factory = entity,
                Groups = groups.Where(item => item.FactoryId == entity.Id)
            });
        }
        return results;
    }
    public string TableName { get; init; } = TableName<Entity>();
}