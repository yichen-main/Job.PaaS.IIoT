namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IEstablishInformation : ITacticExpert
{
    const string Banner = "information";
    ValueTask InstallAsync();
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    readonly record struct StatusLabel
    {
        public required string Run { get; init; }
        public required string Idle { get; init; }
        public required string Error { get; init; }
        public required string Setup { get; init; }
        public required string Shutdown { get; init; }
        public required string Repair { get; init; }
        public required string Maintenance { get; init; }
        public required string Hold { get; init; }
    }

    [Table(Name = $"{IProcessEstablish.Type}_{IProcessEstablish.Banner}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "run")] public required string Run { get; init; }
        [Field(Name = "idle")] public required string Idle { get; init; }
        [Field(Name = "error")] public required string Error { get; init; }
        [Field(Name = "setup")] public required string Setup { get; init; }
        [Field(Name = "shutdown")] public required string Shutdown { get; init; }
        [Field(Name = "repair")] public required string Repair { get; init; }
        [Field(Name = "maintenance")] public required string Maintenance { get; init; }
        [Field(Name = "hold")] public required string Hold { get; init; }
    }
    string TableName { get; init; }
}