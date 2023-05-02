namespace IIoT.Domain.Shared.Businesses.Workshops.Missions;
public interface IMission : ITacticExpert
{
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity, IMissionPush.Entity mission);
    ValueTask UpdateAsync(Entity entity, IMissionPush.Entity mission);
    Task<Entity> GetAsync(Guid id);
    Task<SingleMission> GetSingleMissionAsync(Guid id);
    Task<MultipleMission> GetMultipleMissionAsync();
    Task<IEnumerable<Entity>> ListAsync();
    enum Category
    {
        PushTask = 101
    }
    readonly record struct SingleMission
    {
        public required Entity Entity { get; init; }
        public required IMissionPush.Entity Push { get; init; }
        public required IEnumerable<IEquipment.Entity> Equipments { get; init; }
    }
    readonly record struct MultipleMission
    {
        public required IEnumerable<Entity> Entities { get; init; }
        public required IEnumerable<IMissionPush.Entity> Pushs { get; init; }
        public required IEnumerable<IEquipment.Entity> Equipments { get; init; }
    }

    [Table(Name = $"{Deputy.Workshop}_{Deputy.Mission}_{Deputy.Foundation}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "equipment_id")] public required Guid EquipmentId { get; init; }
        [Field(Name = "category_type")] public required Category CategoryType { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    static string LinkCategoryType => $"{Deputy.Mission}_{nameof(Entity.CategoryType).To<Entity>()}_{Deputy.ComboLink}";
    string TableName { get; init; }
}