namespace IIoT.Retinue.Services;
internal static class InitializeService
{
    internal static async ValueTask BuildAsync(this ValueTask task)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //await new RunnerProfile().BuildAsync();
            }
        }
        catch (Exception e)
        {
            Log.Fatal(Morse.HistoryDefault, nameof(InitializeService), new
            {
                Detail = string.Join(",\u00A0", new string[]
                {
                    e.Message,
                    e.StackTrace ?? string.Empty
                })
            });
        }
        await task.ConfigureAwait(default);
    }
    internal static WebApplication UseInternetArchitecture(this WebApplication service)
    {
        service.MapGet("/book", (string repository) =>
        {
            return repository;
        });
        service.MapPost("/todoitems", ([FromBody] JToken body) =>
        {
            try
            {
                return Results.Created("/todoitems/{todoItem.Id}", default);
            }
            catch (Exception e)
            {
                return Results.NotFound(new
                {
                    e.Message
                });
            }
        });
        return service;
    }
    internal static Carrier CarrierText { get; set; } = new();
    internal static RunnerProfile.Text RunnerText { get; set; } = new();
}