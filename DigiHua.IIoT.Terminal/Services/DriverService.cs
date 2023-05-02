namespace IIoT.Terminal.Services;
internal sealed class DriverService : DriverExpert
{
    public DriverService() : base(GetlocalName()[2].ToMd5().Joint(Extension), "Slave", new[]
    {
        RootLocation, "/", Morse.LogisticRoot, "/"
    }.Concat())
    { }
    public bool IsServiceStart() => IsEnable(ServiceName);
    public string Install() => Execute(FilePath, new[]
    {
        ServiceName, Boot
    }.Concat().Joint(Extension));
    public string Remove() => Execute(FilePath, new[]
    {
        ServiceName, Shutdown
    }.Concat().Joint(Extension));
    public string Restart() => Execute(FilePath, new[]
    {
        ServiceName, Reboot
    }.Concat().Joint(Extension));
    public async ValueTask BuildAsync() => await $"""
    {BatchTitle}
    {RootPath}/{".".Joint()}
    Taskkill /im {Identification}.exe /F
    cls & {Identification}.exe {Identification.ToMd5()} -app
    """.WriteLineAsync(new[]
    {
        ResourcePath, FileName
    }.Concat());
}