using static IIoT.Domain.Shared.Businesses.Manages.Users.IUser;

namespace IIoT.Domain.Businesses.Manages.Users;

internal sealed class User : TacticExpert, IUser
{
    readonly INpgsqlUtility _npgsqlUtility;
    readonly IFoundationTrigger _foundationTrigger;
    public User(INpgsqlUtility npgsqlUtility, IFoundationTrigger foundationTrigger)
    {
        _npgsqlUtility = npgsqlUtility;
        _foundationTrigger = foundationTrigger;
    }
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName))
        {
            await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
            {
                Uniques = new[]
                {
                    nameof(Entity.Account).FieldInfo<Entity>().Name,
                    nameof(Entity.Username).FieldInfo<Entity>().Name
                }
            }), default, enable: true);
            await AddAsync(new()
            {
                Id = Guid.NewGuid(),
                GroupType = Group.IIoTPlatform,
                LicenseType = IUser.License.Administrator,
                OperateType = Operate.Enable,
                Account = Morse.DigiHua,
                Username = Morse.DigiHua,
                Password = _foundationTrigger.UseEncryptAES(Morse.DigiHua),
                Creator = Morse.DigiHua,
                CreateTime = DateTime.UtcNow
            }, enable: true);
        }
    }
    public async ValueTask AddAsync(Entity entity, bool enable) => await ExecuteAsync(_npgsqlUtility.MarkInsert<Entity>(), entity, enable);
    public async ValueTask UpdateAsync(Entity entity) => await ExecuteAsync(_npgsqlUtility.MarkUpdate<Entity>(new[]
    {
        nameof(Entity.GroupType),
        nameof(Entity.LicenseType),
        nameof(Entity.OperateType),
        nameof(Entity.Username),
        nameof(Entity.Password),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }), entity, Morse.Passer);
    public async Task<Entity> GetAsync(Guid id)
    {
        var result = await SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
        {
            nameof(Entity.Id),
            nameof(Entity.GroupType),
            nameof(Entity.LicenseType),
            nameof(Entity.OperateType),
            nameof(Entity.Account),
            nameof(Entity.Username),
            nameof(Entity.Password),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
        {
            id
        }, Morse.Passer);
        return new()
        {
            Id = result.Id,
            GroupType = result.GroupType,
            LicenseType = result.LicenseType,
            OperateType = result.OperateType,
            Account = result.Account,
            Username = result.Username,
            Password = _foundationTrigger.UseDecryptAES(result.Password),
            Creator = result.Creator,
            CreateTime = result.CreateTime
        };
    }
    public async Task<Entity> GetAsync(string account)
    {
        var result = await SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
        {
            nameof(Entity.Id),
            nameof(Entity.GroupType),
            nameof(Entity.LicenseType),
            nameof(Entity.OperateType),
            nameof(Entity.Account),
            nameof(Entity.Username),
            nameof(Entity.Password),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }).AddObjectFilter(nameof(Entity.Account).To<Entity>(), nameof(account)), new
        {
            account
        }, Morse.Passer);
        return new()
        {
            Id = result.Id,
            GroupType = result.GroupType,
            LicenseType = result.LicenseType,
            OperateType = result.OperateType,
            Username = result.Username,
            Account = result.Account,
            Password = _foundationTrigger.UseDecryptAES(result.Password),
            Creator = result.Creator,
            CreateTime = result.CreateTime
        };
    }
    public async Task<IEnumerable<Entity>> ListAsync()
    {
        var result = await QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
        {
            nameof(Entity.Id),
            nameof(Entity.GroupType),
            nameof(Entity.LicenseType),
            nameof(Entity.OperateType),
            nameof(Entity.Account),
            nameof(Entity.Username),
            nameof(Entity.Password),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }).ToString(), default, Morse.Passer);
        return result.Select(item => new Entity
        {
            Id = item.Id,
            GroupType = item.GroupType,
            LicenseType = item.LicenseType,
            OperateType = item.OperateType,
            Username = item.Username,
            Account = item.Account,
            Password = _foundationTrigger.UseDecryptAES(item.Password),
            Creator = item.Creator,
            CreateTime = item.CreateTime
        });
    }
    public async Task<IEnumerable<Entity>> ListAsync(Group genre)
    {
        var result = await QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
        {
            nameof(Entity.Id),
            nameof(Entity.GroupType),
            nameof(Entity.LicenseType),
            nameof(Entity.OperateType),
            nameof(Entity.Account),
            nameof(Entity.Username),
            nameof(Entity.Password),
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }).AddEqualFilter(nameof(Entity.GroupType).To<Entity>(), (int)genre), default, Morse.Passer);
        return result.Select(item => new Entity
        {
            Id = item.Id,
            GroupType = item.GroupType,
            LicenseType = item.LicenseType,
            OperateType = item.OperateType,
            Username = item.Username,
            Account = item.Account,
            Password = _foundationTrigger.UseDecryptAES(item.Password),
            Creator = item.Creator,
            CreateTime = item.CreateTime
        });
    }
    public string TableName { get; init; } = TableName<Entity>();
}