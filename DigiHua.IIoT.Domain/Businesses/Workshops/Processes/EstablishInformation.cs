using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IEstablishInformation;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class EstablishInformation : TacticExpert, IEstablishInformation
{
    readonly INpgsqlUtility _npgsqlUtility;
    public EstablishInformation(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()), default, enable: true);
    }
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.Run),
        nameof(Entity.Idle),
        nameof(Entity.Error),
        nameof(Entity.Setup),
        nameof(Entity.Shutdown),
        nameof(Entity.Repair),
        nameof(Entity.Maintenance),
        nameof(Entity.Hold)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.Run),
        nameof(Entity.Idle),
        nameof(Entity.Error),
        nameof(Entity.Setup),
        nameof(Entity.Shutdown),
        nameof(Entity.Repair),
        nameof(Entity.Maintenance),
        nameof(Entity.Hold)
    }).ToString(), default, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}