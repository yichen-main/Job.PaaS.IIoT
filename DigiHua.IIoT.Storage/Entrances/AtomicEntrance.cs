namespace IIoT.Storage.Entrances;
internal sealed class AtomicEntrance : IEntranceTrigger
{
    public async ValueTask PushAsync()
    {
        try
        {
            await FoundationTrigger.CreateFileAaync(new[]
            {
                BreakerFolder, Identification, IEntranceTrigger.Operater
            }.Concat().Joint(Morse.BatchExtension), $""""
            {IEntranceTrigger.Title}
            {IEntranceTrigger.Administrator}
            set launcher={IEntranceTrigger.Launcher}
            set title={Identification}
            set location={ResourcePath}
            set route={ResourcePath}influxd.exe
            cd \ & {char.ToLower(RootLocation.FirstOrDefault())}:
            cd %location% & %launcher% install "%title%" "%route%" & net start %title%
            {IEntranceTrigger.Timeout}
            """", Extension.Text, cover: true);
            await FoundationTrigger.CreateFileAaync(new[]
            {
                BreakerFolder, Identification, IEntranceTrigger.Shutdown
            }.Concat().Joint(Morse.BatchExtension), $""""
            {IEntranceTrigger.Title}
            {IEntranceTrigger.Administrator}
            set launcher={IEntranceTrigger.Launcher}
            set title={Identification}
            set location={ResourcePath}
            cd \ & {char.ToLower(RootLocation.FirstOrDefault())}:
            cd %location% & %launcher% stop "%title%" & net remove %title% confirm & sc delete "%title%"
            {IEntranceTrigger.Timeout}
            """", Extension.Text, cover: true);
            if (File.Exists(IEntranceTrigger.Installation)) File.Delete(IEntranceTrigger.Installation);
        }
        catch (Exception e)
        {
            Log.Fatal(Morse.HistoryDefault, nameof(AtomicEntrance), new
            {
                e.Message,
                e.StackTrace
            });
        }
    }
    public required IFoundationTrigger FoundationTrigger { get; init; }
}