namespace IIoT.Terminal.Services;
internal static class InitializeService
{
    public static async ValueTask BuildAsync(this ValueTask task)
    {
        //await new DriverService().BuildAsync();
        //await new DriverService().CreateStarterAsync();
        //await new DriverService().CreateStopperAsync();
        //await new DriverService().CreateRestarterAsync();
        await task.ConfigureAwait(default);
    }
}