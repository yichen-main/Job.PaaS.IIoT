namespace IIoT.Domain.Shared.Businesses.Manages.Users;
public interface IUserVerification : ITacticExpert
{
    const string Banner = "verification";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);

    [Table(Name = $"{Deputy.Manage}_{IUser.Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "token")] public required string Token { get; init; }
        [Field(Name = "refresh_token")] public required string RefreshToken { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
}