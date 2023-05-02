namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IProductionStack
{
    const string Banner = "production";
    ValueTask InstallAsync();
    Task<Entity> GetAsync(Guid id);

    [StructLayout(LayoutKind.Auto), Table(Name = $"{IProcessEstablish.Banner}_{Banner}_{Deputy.Stack}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "output")] public required int Output { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}