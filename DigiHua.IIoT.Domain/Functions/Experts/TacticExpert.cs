namespace IIoT.Domain.Functions.Experts;
public abstract class TacticExpert : ITacticExpert
{
    protected TacticExpert() => DefaultTypeMap.MatchNamesWithUnderscores = true;
    public async Task<bool> ExistDatabaseAsync(string name)
    {
        await using NpgsqlConnection npgsql = new(ConnectionString);
        await npgsql.OpenAsync();
        var results = await npgsql.QueryAsync($"SELECT datname FROM pg_catalog.pg_database WHERE datname = '{name}'");
        return results.Count() is not 0;
    }
    public async Task<bool> ExistTableAsync(string name)
    {
        await using NpgsqlConnection npgsql = new(ConnectionString);
        await npgsql.OpenAsync();
        return await npgsql.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM pg_class WHERE relname = @name", new
        {
            name
        });
    }
    public async Task<int> CountAsync(string content, bool enable)
    {
        if (enable && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            return await npgsql.QueryFirstAsync<int>(content);
        }
        return default;
    }
    public async Task<IEnumerable<T>> QueryAsync<T>(string content, object? @object, bool enable) where T : struct
    {
        if (enable && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            return await npgsql.QueryAsync<T>(content, @object);
        }
        return await ValueTask.FromResult(Array.Empty<T>());
    }
    public async Task<T> SingleQueryAsync<T>(string content, object? @object, bool enable) where T : struct
    {
        if (enable && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            return await npgsql.QuerySingleOrDefaultAsync<T>(content, @object);
        }
        return default;
    }
    public async ValueTask ExecuteAsync(string content, object? @object, bool enable)
    {
        if (enable && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            await npgsql.ExecuteAsync(content, @object);
            await npgsql.CloseAsync();
        }
    }
    public async ValueTask TransactionAsync(IEnumerable<(string content, object? @object)> values)
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            await using var result = await npgsql.BeginTransactionAsync();
            try
            {
                foreach (var (content, @object) in values)
                {
                    await npgsql.ExecuteAsync(content, @object, transaction: result);
                }
                await result.CommitAsync();
            }
            catch (Exception)
            {
                await result.RollbackAsync();
                throw;
            }
            finally
            {
                await npgsql.CloseAsync();
            }
        }
    }
}