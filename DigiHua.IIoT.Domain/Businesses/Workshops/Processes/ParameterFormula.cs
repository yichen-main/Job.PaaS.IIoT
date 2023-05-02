using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IParameterFormula;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class ParameterFormula : TacticExpert, IParameterFormula
{
    readonly INpgsqlUtility _npgsqlUtility;
    public ParameterFormula(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public async ValueTask AddAsync(IEnumerable<Entity> entities)
    {
        List<(string content, object? @object)> results = new();
        foreach (var entity in entities) results.Add((_npgsqlUtility.MarkInsert<Entity>(), entity));
        await TransactionAsync(results);
    }
    public string TableName { get; init; } = TableName<Entity>();
}