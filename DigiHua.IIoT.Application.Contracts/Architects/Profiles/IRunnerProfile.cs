namespace IIoT.Application.Contracts.Architects.Profiles;
public interface IRunnerProfile
{
    ValueTask BuildAsync();
    ValueTask<Text> ReadAsync();
    string FullPath { get; }
    sealed class Text
    {
        [YamlMember(ApplyNamingConventions = false)] public required TextOrganization Organization { get; init; }
        [YamlMember(ApplyNamingConventions = false)] public required TextPlatform Platform { get; init; }
        public sealed class TextOrganization
        {
            [YamlMember(ApplyNamingConventions = false)] public required bool Debug { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string CompanyID { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string CompanyName { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string SiteID { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string SiteName { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required LogEventLevel LogLevel { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Language { get; init; }
        }
        public sealed class TextPlatform
        {
            [YamlMember(ApplyNamingConventions = false)] public required int Entrance { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required int Mqbroker { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Username { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Password { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public required string Hash { get; init; }
        }
    }
}