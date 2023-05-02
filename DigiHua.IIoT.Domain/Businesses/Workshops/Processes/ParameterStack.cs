using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IParameterStack;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class ParameterStack : TacticExpert, IParameterStack
{
    readonly INpgsqlUtility _npgsqlUtility;
    public ParameterStack(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.DataValue),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}