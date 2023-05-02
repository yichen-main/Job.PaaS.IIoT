namespace IIoT.Domain.Infrastructure.Sturdies;
public static class SturdyExpansion
{
    public static DateTime GetHour(this DateTime dateTime) => dateTime.ToString(Converter.EaiHour).ToDateTime();
    public static DateTime ToDateTime(this string dateTime) => DateTime.ParseExact(dateTime, new[]
    {
        Converter.DefaultSeconds, Converter.EaiHour, Converter.EaiSeconds, Converter.EaiMillisecond, Converter.DefaultMillisecond
    }, provider: CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
    public static string Concat(this string[] args)
    {
        DefaultInterpolatedStringHandler handler = new(default, args.Length);
        for (int item = default; item < args.Length; item++) handler.AppendFormatted(args[item]);
        return handler.ToStringAndClear();
    }
    public static string NeatlyClock(this long integer) => integer.ToString().PadLeft(6, '0');
    public static string HeaderName<T>(this string name) => typeof(T).GetProperty(name)?.GetCustomAttribute<FromHeaderAttribute>()?.Name ?? string.Empty;
    public static string HashAlgorithm(this string text) => BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
    public static string GetDesc(this Enum @enum) => @enum.GetType().GetRuntimeField(@enum.ToString())!.GetCustomAttribute<DescriptionAttribute>()!.Description;
    public static string Joint(this string front, string latter = "", string tag = ".") => $"{front}{tag}{latter}";
    public static string AddURL(this string ip, in int port, in string path = "") => $"{Uri.UriSchemeHttp}://{ip}:{port}{path}";
    public static string AddLocalIPv4(this NetworkInterfaceType networkInterfaceType)
    {
        var result = string.Empty;
        Array.ForEach(NetworkInterface.GetAllNetworkInterfaces(), item =>
        {
            if (item.NetworkInterfaceType == networkInterfaceType && item.OperationalStatus is OperationalStatus.Up)
            {
                foreach (var info in item.GetIPProperties().UnicastAddresses)
                {
                    if (info.Address.AddressFamily is AddressFamily.InterNetwork) result = info.Address.ToString();
                }
            }
        });
        return result;
    }
    public static async Task WaitAsync()
    {
        Floor.Executioner = true;
        if (Floor.Missions.Any()) await Task.WhenAll(Floor.Missions);
    }
    public static void UseMarquee() => Floor.Missions = new[]
    {
        Task.Run(async () =>
        {
            var result = string.Empty;
            PeriodicTimer periodic = new(TimeSpan.FromMilliseconds(100));
            while (await periodic.WaitForNextTickAsync())
            {
                Console.SetCursorPosition(default, Console.CursorTop);
                Console.Write(result switch
                {
                    "\\" => result = "|",
                    "|" => result = "/",
                    "/" => result = "-",
                    _ => result = "\\",
                });
                if (Floor.Executioner) periodic.Dispose();
            }
            Console.SetCursorPosition(default, Console.CursorTop);
            Console.Write(new string('\u00A0', Console.WindowWidth));
        })
    };
    public static string DelimitMark(this IEnumerable<string> contents, in Delimiter delimiter = Delimiter.Comma)
    {
        int count = default;
        DefaultInterpolatedStringHandler result = new(default, contents.Count());
        foreach (var content in contents)
        {
            result.AppendFormatted(content);
            if (contents.Count() - count++ is not 1) result.AppendFormatted(delimiter.GetDesc());
        }
        return result.ToStringAndClear();
    }
    public static T? Parse<T>(this string text, IFormatProvider? provider = null) where T : IParsable<T>
    {
        if (T.TryParse(text, provider, out var value)) return value;
        return default;
    }
    public enum Delimiter
    {
        [Description(",")] Comma,
        [Description(";\u00A0")] Finish
    }
    public readonly record struct Carrier
    {
        public int Influxdb { get; init; }
        public int Postgres { get; init; }
        public string Location { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
        public string Database { get; init; }
    }
    public ref struct Floor
    {
        public static bool Tester { get; set; }
        public static bool Executioner { get; set; }
        public static string SecretKey { get; set; } = Morse.DigiHua;
        public static string CompanyName { get; set; } = Morse.DigiHua;
        public static string ExperiLocation { get; set; } = string.Empty;
        public static string FormalLocation { get; set; } = string.Empty;
        public static string Identification { get; set; } = string.Empty;
        public static Task[] Missions { get; set; } = Array.Empty<Task>();
    }
    public ref struct Morse
    {
        public static bool Meter { get; set; }
        public static bool Passer { get; set; }
        public static string HistoryExtension => "log";
        public static string ProfileExtension => "yml";
        public static string HistoryTimer => "({0}ms) {1}";
        public static string HistoryDefault => "[{0}] {1}";
        public static string Title => "iiot";
        public static string DigiHua => "digihua";
        public static string HistoryRoot => "Logs";
        public static string EmbedsRoot => "Embeds";
        public static string BreakerRoot => "Breakers";
        public static string LogisticRoot => "Resources";
        public static string ConfigureRoot => "Configures";
        public static string Username { get; set; } = DigiHua;
        public static string Password { get; set; } = DigiHua;
        public static string Flowmeter { get; set; } = string.Empty;
        public static string Address { get; set; } = IPAddress.Loopback.ToString();
    }
    public ref struct Converter
    {
        public static string EaiHour => "yyyyMMddHH";
        public static string EaiSeconds => "yyyyMMddHHmmss";
        public static string EaiMillisecond => "yyyyMMddHHmmssfff";
        public static string DefaultSeconds => "yyyy/MM/dd HH:mm:ss";
        public static string DefaultMillisecond => "yyyy-MM-dd HH:mm:ss.fff";
    }
}