using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IEstablishProduction;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class EstablishProduction : TacticExpert, IEstablishProduction
{
    readonly INpgsqlUtility _npgsqlUtility;
    public EstablishProduction(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
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
                (LinkDispatchNoBatchNo, new List<string>
                {
                    nameof(Entity.EstablishId).FieldInfo<Entity>().Name,
                    nameof(Entity.DispatchNo).FieldInfo<Entity>().Name,
                    nameof(Entity.BatchNo).FieldInfo<Entity>().Name
                })
            }
        }), default, enable: true);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
   {
        nameof(Entity.Id),
        nameof(Entity.EstablishId),
        nameof(Entity.DispatchNo),
        nameof(Entity.BatchNo)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListEstablishAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EstablishId),
        nameof(Entity.DispatchNo),
        nameof(Entity.BatchNo)
    }).AddObjectFilter(nameof(Entity.EstablishId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}