using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Services.Profiles;

[Dependency(ServiceLifetime.Singleton)]
internal sealed class ManagerProfile : IManagerProfile
{
    public async ValueTask BuildAsync() => ManagerText = await ReadAsync();
    public async ValueTask<IManagerProfile.Text> ReadAsync()
    {
        await FoundationTrigger.CreateFileAaync(FullPath, new IManagerProfile.Text
        {
            Assembly = new()
            {
                PushCycle = FoundationTrigger.UseEncryptAES(ManagerText.Assembly.PushCycle),
                ExperiBlock = FoundationTrigger.UseEncryptAES(ManagerText.Assembly.ExperiBlock),
                FormalBlock = FoundationTrigger.UseEncryptAES(ManagerText.Assembly.FormalBlock),
                CollectBlock = FoundationTrigger.UseEncryptAES(ManagerText.Assembly.CollectBlock)
            },
            Hangar = new()
            {
                Merchant = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Merchant),
                Flowmeter = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Flowmeter),
                Location = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Location),
                Identifier = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Identifier),
                Plaque = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Plaque),
                Pespond = FoundationTrigger.UseEncryptAES(ManagerText.Hangar.Pespond)
            }
        }, Extension.Yaml);
        Configuration = FoundationTrigger.InitialFile(FullPath, Extension.Yaml, change: false);
        var result = FoundationTrigger.RefreshFile(ManagerText, Configuration);
        return new()
        {
            Assembly = new()
            {
                PushCycle = FoundationTrigger.UseDecryptAES(result.Assembly.PushCycle),
                ExperiBlock = FoundationTrigger.UseDecryptAES(result.Assembly.ExperiBlock),
                FormalBlock = FoundationTrigger.UseDecryptAES(result.Assembly.FormalBlock),
                CollectBlock = FoundationTrigger.UseDecryptAES(result.Assembly.CollectBlock)
            },
            Hangar = new()
            {
                Merchant = FoundationTrigger.UseDecryptAES(result.Hangar.Merchant),
                Flowmeter = FoundationTrigger.UseDecryptAES(result.Hangar.Flowmeter),
                Location = FoundationTrigger.UseDecryptAES(result.Hangar.Location),
                Plaque = FoundationTrigger.UseDecryptAES(result.Hangar.Plaque),
                Identifier = FoundationTrigger.UseDecryptAES(result.Hangar.Identifier),
                Pespond = FoundationTrigger.UseDecryptAES(result.Hangar.Pespond)
            }
        };
    }
    public string FullPath => string.Join("/", new string[]
    {
        RootLocation, Morse.LogisticRoot, new[]
        {
            (nameof(IManagerProfile.Text) + Floor.SecretKey).HashAlgorithm()
        }.Concat().Joint(Morse.ProfileExtension)
    });
    IConfiguration? Configuration { get; set; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
}