namespace IIoT.Domain.Shared.Functions.Triggers;
public interface IFoundationTrigger
{
    const char ProhibitSign = '@';
    string UseEncryptAES(in string text);
    string UseDecryptAES(in string text);
    string UseFormatXml(in string text);
    T? UseDeserializeXml<T>(in string text);
    string UseSerializerXml(in object entity, in bool indent = default);
    IDictionary<string, object> UseJsonToDictionary(in string text);
    IConfiguration InitialFile(in string path, in Extension type = Extension.Json, in bool change = true);
    T RefreshFile<T>(in T entity, in IConfiguration configuration);
    ValueTask CreateFileAaync<T>(string path, T entity, Extension type = Extension.Json, bool cover = default);
    void MonitorFile(in Action<object, FileSystemEventArgs> action, in string path, in string name);
    IEnumerable<TSource> Concat<TSource>(IEnumerable<TSource> originals, IEnumerable<TSource> merges);
    IEnumerable<T> FindRepeat<T>(in T[] entities);
    bool CheckParity<T>(in IEnumerable<T> fronts, in IEnumerable<T> backs) where T : notnull;
    void Clear(EventHandler? @event);
    enum Extension
    {
        Json,
        Text,
        Yaml,
        Xml
    }
    enum Operate
    {
        Enable = 101,
        Disable = 102
    }
    public static TimeSpan UnifiedTimer => new(default, default, 30);
    static string Location { get; set; } = string.Empty;
    static string BrokerIp
    {
        get
        {
            if (string.IsNullOrEmpty(Location))
            {
                Location = NetworkInterfaceType.Ethernet.AddLocalIPv4();
                {
                    if (string.IsNullOrEmpty(Location)) Location = NetworkInterfaceType.Wireless80211.AddLocalIPv4();
                    if (string.IsNullOrEmpty(Location)) Location = IPAddress.Loopback.ToString();
                }
            }
            return Location;
        }
        set
        {
            Location = value;
        }
    }
    static string Language { get; set; } = Thread.CurrentThread.CurrentCulture.IetfLanguageTag;
    static string RootLocation => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
    static string BreakerFolder
    {
        get => $"{ExternalPath}{Morse.BreakerRoot}\\";
    }
    static string ConfigurationFolder
    {
        get => $"{ExternalPath}{Morse.ConfigureRoot}\\";
    }
    static string ExternalPath
    {
        get => $"{RootLocation}\\{".".Joint()}\\";
    }
    static string ResourcePath
    {
        get => $"{RootLocation}\\{Morse.LogisticRoot}\\";
    }
}