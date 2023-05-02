namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IEstablishProduction
{
    const string Banner = "production";
    ValueTask InstallAsync();
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListEstablishAsync(Guid id);

    [Table(Name = $"{IProcessEstablish.Type}_{IProcessEstablish.Banner}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "establish_id")] public required Guid EstablishId { get; init; }
        [Field(Name = "dispatch_no")] public required string DispatchNo { get; init; }
        [Field(Name = "batch_no")] public required string BatchNo { get; init; }
    }
    static string LinkDispatchNoBatchNo => $"{Banner}_{nameof(Entity.DispatchNo).To<Entity>()}_{nameof(Entity.BatchNo).To<Entity>()}_{Deputy.ComboLink}";
    string TableName { get; init; }
}