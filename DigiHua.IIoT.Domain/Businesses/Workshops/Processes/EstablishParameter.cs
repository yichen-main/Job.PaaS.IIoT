using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IEstablishParameter;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class EstablishParameter : TacticExpert, IEstablishParameter
{
    readonly INpgsqlUtility _npgsqlUtility;
    public EstablishParameter(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.EstablishId).FieldInfo<Entity>().Name, TableName<IProcessEstablish.Entity>(), CurrentSign)
            },
            Combos = new[]
            {
                (LinkDataNo, new List<string>
                {
                    nameof(Entity.EstablishId).FieldInfo<Entity>().Name,
                    nameof(Entity.DataNo).FieldInfo<Entity>().Name
                })
            }
        }), default, enable: true);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EstablishId),
        nameof(Entity.DataNo)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EstablishId),
        nameof(Entity.DataNo)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListEstablishAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EstablishId),
        nameof(Entity.DataNo)
    }).AddObjectFilter(nameof(Entity.EstablishId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}