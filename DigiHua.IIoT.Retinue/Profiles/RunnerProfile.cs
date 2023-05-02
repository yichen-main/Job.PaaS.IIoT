namespace IIoT.Retinue.Profiles;
internal sealed class RunnerProfile
{
    //public RunnerProfile() => FullPath = (new[]
    //{
    //    Folder, RootName.Joint(nameof(Retinue)), Yaml
    //}).Concat();
    //public async ValueTask BuildAsync() => RunnerText = await ReadAsync();
    //public async ValueTask<Text> ReadAsync()
    //{
    //    await CreateAaync(new(), Extension.YAML);
    //    {
    //        Configuration = Initialization(Extension.YAML);
    //        return Build(new());
    //    }
    //}
    public sealed class Text
    {
        [YamlMember(ApplyNamingConventions = false)] public TextNative Native { get; init; } = new();
        [YamlMember(ApplyNamingConventions = false)] public TextBroker Broker { get; init; } = new();
        public sealed class TextNative
        {
            [YamlMember(ApplyNamingConventions = false)] public int Port { get; init; } = 17780;
        }
        public sealed class TextBroker
        {
            [YamlMember(ApplyNamingConventions = false)] public bool Enable { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public string Ip { get; init; } = "BrokerIp";
            [YamlMember(ApplyNamingConventions = false)] public int Port { get; init; } = 17883;
            [YamlMember(ApplyNamingConventions = false)] public string Username { get; init; } = Morse.DigiHua;
            [YamlMember(ApplyNamingConventions = false)] public string Password { get; init; } = Morse.DigiHua;
        }
    }
}