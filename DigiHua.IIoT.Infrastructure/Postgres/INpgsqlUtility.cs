namespace IIoT.Domain.Infrastructure.Postgres;
public interface INpgsqlUtility
{
    string MarkTable<T>(in TableInfo info) where T : struct;
    string MarkInsert<T>() where T : struct;
    string MarkUpdate<T>(in IEnumerable<string> names) where T : struct;
    string MarkDelete<T>(Guid primaryKey) where T : struct;
    StringBuilder MarkQuery<T>(in IEnumerable<string> names) where T : struct;
}