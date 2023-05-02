try
{
    await Assembly.GetExecutingAssembly().AddEnvironmentAsync().BuildAsync();
    if (Identification.HashAlgorithm().Challenge()) await Host.CreateDefaultBuilder(args).ConfigureServices(async item =>
    {
        await item.AddApplicationAsync<AppModule>(item => item.PlugInSources.AddFolder(string.Join("/", new[]
        {
            RootLocation, Morse.EmbedsRoot
        })));
    }).UseSystemd().UseAutofac().UseSerilog().Build().RunAsync();
}
catch (Exception e)
{
    Log.Fatal(Morse.HistoryDefault, nameof(Program), new
    {
        e.Message,
        e.StackTrace
    });
}
finally
{
    Log.CloseAndFlush();
}