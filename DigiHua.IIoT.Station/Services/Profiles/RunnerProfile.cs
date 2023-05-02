using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Services.Profiles;

[Dependency(ServiceLifetime.Singleton)]
internal sealed class RunnerProfile : IRunnerProfile
{
    public async ValueTask BuildAsync() => RunnerText = await ReadAsync();
    public async ValueTask<IRunnerProfile.Text> ReadAsync()
    {
        await FoundationTrigger.CreateFileAaync(FullPath, RunnerText, Extension.Yaml);
        Configuration ??= FoundationTrigger.InitialFile(FullPath, Extension.Yaml);
        return FoundationTrigger.RefreshFile(RunnerText, Configuration);
    }
    public string FullPath => (new[]
    {
        ConfigurationFolder, nameof(IIoT).Joint(nameof(Station))
    }).Concat().Joint(Morse.ProfileExtension);
    IConfiguration? Configuration { get; set; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
}