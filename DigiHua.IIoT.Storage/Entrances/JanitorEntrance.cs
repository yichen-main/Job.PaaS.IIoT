namespace IIoT.Storage.Entrances;
internal sealed class JanitorEntrance : IEntranceTrigger
{
    public async ValueTask PushAsync()
    {
        try
        {
            await Cli.Wrap(new[]
            {
                BreakerFolder, Identification, IEntranceTrigger.Operater
            }.Concat().Joint(Morse.BatchExtension)).ExecuteBufferedAsync();
        }
        catch (Exception e)
        {
            Log.Fatal(Morse.HistoryDefault, nameof(JanitorEntrance), new
            {
                e.Message,
                e.StackTrace
            });
        }
    }
}