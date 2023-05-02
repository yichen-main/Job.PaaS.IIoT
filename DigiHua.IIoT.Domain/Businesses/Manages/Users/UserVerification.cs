using static IIoT.Domain.Shared.Businesses.Manages.Users.IUserVerification;

namespace IIoT.Domain.Businesses.Manages.Users;
internal sealed class UserVerification : TacticExpert, IUserVerification
{
    readonly INpgsqlUtility _npgsqlUtility;
    public UserVerification(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Uniques = new[]
            {
                nameof(Entity.Token).FieldInfo<Entity>().Name
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>(), entity, enable: true);
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.Token),
        nameof(Entity.RefreshToken),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}