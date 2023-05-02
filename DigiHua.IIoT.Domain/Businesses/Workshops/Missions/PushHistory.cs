using static IIoT.Domain.Shared.Businesses.Workshops.Missions.IPushHistory;

namespace IIoT.Domain.Businesses.Workshops.Missions;
internal sealed class PushHistory : TacticExpert, IPushHistory
{
    readonly INpgsqlUtility _npgsqlUtility;
    public PushHistory(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public Task<int> GaugeAsync(string filter) => CountAsync(nameof(Entity.Id).To<Entity>().AddTotalCount<Entity>().AddTitleFilter(filter), Morse.Passer);
    public async ValueTask AddAsync(Entity entity, IEnumerable<IInformationStack.Entity> stacks)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>(), entity)
        };
        foreach (var stack in stacks) results.Add((_npgsqlUtility.MarkUpdate<IInformationStack.Entity>(new[]
        {
            nameof(IInformationStack.Entity.Status),
            nameof(IInformationStack.Entity.CreateTime)
        }), stack));
        await TransactionAsync(results);
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<IParameterStack.Entity> stacks)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>(), entity)
        };
        foreach (var stack in stacks) results.Add((_npgsqlUtility.MarkUpdate<IParameterStack.Entity>(new[]
        {
            nameof(IParameterStack.Entity.DataValue),
            nameof(IParameterStack.Entity.CreateTime)
        }), stack));
        await TransactionAsync(results);
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<IProductionStack.Entity> stacks)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>(), entity)
        };
        foreach (var stack in stacks) results.Add((_npgsqlUtility.MarkUpdate<IProductionStack.Entity>(new[]
        {
            nameof(IProductionStack.Entity.Output),
            nameof(IProductionStack.Entity.CreateTime)
        }), stack));
        await TransactionAsync(results);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EaiType),
        nameof(Entity.EnvironmentType),
        nameof(Entity.EaiType),
        nameof(Entity.ContentRecord),
        nameof(Entity.ResultRecord),
        nameof(Entity.ConsumeMS),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync(string format, IEnumerable<(string field, string value)> filter) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EaiType),
        nameof(Entity.EnvironmentType),
        nameof(Entity.EaiType),
        nameof(Entity.ContentRecord),
        nameof(Entity.ResultRecord),
        nameof(Entity.ConsumeMS),
        nameof(Entity.CreateTime)
    }).AddIntervalFilter(format, filter.ToArray()), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}