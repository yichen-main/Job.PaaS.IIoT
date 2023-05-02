namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IEstablishParameter : ITacticExpert
{
    const string Banner = "parameter";
    ValueTask InstallAsync();
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListEstablishAsync(Guid id);

    [Table(Name = $"{IProcessEstablish.Type}_{IProcessEstablish.Banner}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "establish_id")] public required Guid EstablishId { get; init; }
        [Field(Name = "data_no")] public required string DataNo { get; init; }
    }
    static string LinkDataNo => $"{Banner}_{nameof(Entity.DataNo).To<Entity>()}_{Deputy.ComboLink}";
    string TableName { get; init; }
}