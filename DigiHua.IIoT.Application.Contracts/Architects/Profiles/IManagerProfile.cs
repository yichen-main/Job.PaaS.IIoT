namespace IIoT.Application.Contracts.Architects.Profiles;
public interface IManagerProfile
{
    ValueTask BuildAsync();
    ValueTask<Text> ReadAsync();
    string FullPath { get; }
    sealed class Text
    {
        [YamlMember(ApplyNamingConventions = false)] public required TextAssembly Assembly { get; init; }
        [YamlMember(ApplyNamingConventions = false)] public required TextHangar Hangar { get; init; }
        public sealed class TextAssembly
        {
            [YamlMember(ApplyNamingConventions = false)] public required string PushCycle { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string ExperiBlock { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string FormalBlock { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string CollectBlock { get; init; }
        }
        public sealed class TextHangar
        {
            [YamlMember(ApplyNamingConventions = false)] public required string Merchant { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Flowmeter { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Location { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Identifier { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Plaque { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Pespond { get; init; }
        }
    }
}