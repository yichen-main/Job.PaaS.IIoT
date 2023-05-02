namespace IIoT.Storage.Services;
internal static class InitializeService
{
    internal static async ValueTask BuildAsync(this ValueTask task)
    {
        DirectoryInfo kernel = new(ExternalPath);
        {
            kernel.Create();
            kernel.CreateSubdirectory(Morse.BreakerRoot);
        }
        await task.ConfigureAwait(default);
    }
    internal static async ValueTask BuildAsync(this IAbpApplicationWithInternalServiceProvider provider)
    {
        foreach (var item in provider.ServiceProvider.GetServices<IEntranceTrigger>()) await item.PushAsync();
    }
}