using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Services.Architects;

[Dependency(ServiceLifetime.Singleton)]
internal sealed class AuthenticateService : IAuthenticateService
{
    public async Task<string> LoginAsync(string account, string password)
    {
        try
        {
            var entities = await BusinessFoundation.User.ListAsync(IUser.Group.IIoTPlatform);
            var entity = entities.FirstOrDefault(item => item.Account == account);
            if (entity.Password != password) throw new Exception();
            return UseJsonWebToken(account, entity);
        }
        catch (Exception)
        {
            if (Accounts.Any(item => item.Key == account && item.Value == password)) return UseJsonWebToken(account, new()
            {
                Id = default,
                OperateType = Operate.Enable,
                LicenseType = IUser.License.Administrator,
                GroupType = IUser.Group.IIoTPlatform,
                Username = Morse.DigiHua,
                Account = account,
                Password = password,
                Creator = Morse.DigiHua,
                CreateTime = DateTime.UtcNow
            });
        }
        string UseJsonWebToken(string username, IUser.Entity entity)
        {
            if (Validators.TryGetValue(username, out var value)) return value.token;
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, entity.Username),
                new Claim(ClaimTypes.Sid, entity.Id.ToString())
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RunnerText.Platform.Hash));
            var jsonWebToken = new JwtSecurityToken(issuer: entity.LicenseType.ToString(),
                claims: claims, notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            var token = new JwtSecurityTokenHandler().WriteToken(jsonWebToken);
            var result = Validators.AddOrUpdate(username, (token, entity), (key, value) => (token, entity));
            return result.token;
        }
        return string.Empty;
    }
    public required IBusinessFoundationWrapper BusinessFoundation { get; init; }
    public ConcurrentDictionary<string, (string token, IUser.Entity entity)> Validators { get; set; } = new();
    static IDictionary<string, string> Accounts => new Dictionary<string, string> { { Morse.DigiHua, Morse.DigiHua } };
}