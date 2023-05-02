try
{
    await Assembly.GetExecutingAssembly().AddEnvironmentAsync().BuildAsync();
    if (Identification.HashAlgorithm().Clearance())
    {
        var provider = await AbpApplicationFactory.CreateAsync<AppModule>(item =>
        {
            item.UseAutofac();
        });
        await provider.InitializeAsync();
        await provider.BuildAsync();
    }
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