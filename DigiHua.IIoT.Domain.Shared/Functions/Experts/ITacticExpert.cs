namespace IIoT.Domain.Shared.Functions.Experts;
public interface ITacticExpert
{
    const string Automatic = "robot";
    Task<bool> ExistTableAsync(string name);
    Task<bool> ExistDatabaseAsync(string name);
    Task<int> CountAsync(string content, bool enable);
    ValueTask ExecuteAsync(string content, object? @object, bool enable);
    ValueTask TransactionAsync(IEnumerable<(string content, object? @object)> values);
    Task<T> SingleQueryAsync<T>(string content, object? @object, bool enable) where T : struct;
    Task<IEnumerable<T>> QueryAsync<T>(string content, object? @object, bool enable) where T : struct;
    readonly ref struct Deputy
    {
        public const string Root = "root";
        public const string Manage = "manage";
        public const string Mission = "mission";
        public const string Workshop = "workshop";
        public const string Equipment = "equipment";
        public const string Foundation = "foundation";
        public const string Process = "process";
        public const string Stack = "stack";
        public static string ComboLink => "combos";
    }
}