namespace IIoT.Domain.Shared.Businesses.Manages.Users;
public interface IUser : ITacticExpert
{
    const string Type = "user";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity, bool enable);
    ValueTask UpdateAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);
    Task<Entity> GetAsync(string account);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListAsync(Group genre);
    enum Group
    {
        IIoTPlatform = 101
    }
    enum License
    {
        Operator = 101,
        Manager = 102,
        Administrator = 103
    }

    [Table(Name = $"{Deputy.Manage}_{Type}_{Deputy.Foundation}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "group_type")] public required Group GroupType { get; init; }
        [Field(Name = "license_type")] public required License LicenseType { get; init; }
        [Field(Name = "operate_type")] public required Operate OperateType { get; init; }
        [Field(Name = "account")] public required string Account { get; init; }
        [Field(Name = "username")] public required string Username { get; init; }
        [Field(Name = "password")] public required string Password { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}