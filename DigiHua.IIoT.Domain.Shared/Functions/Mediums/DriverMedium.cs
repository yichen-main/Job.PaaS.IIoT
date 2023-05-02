namespace IIoT.Domain.Shared.Functions.Mediums;
public abstract class DriverMedium
{
    public const string Extension = "cmd";
    public const string Officer = "service";
    public const string RootPath = "cd %~dp0";
    public const string Boot = "\u00A0[START]";
    public const string Shutdown = "\u00A0[STOP]";
    public const string Reboot = "\u00A0[RESTART]";
    public const string BatchTitle = "@echo off<nul 3>nul";
    public static bool IsEnable(string name)
    {
        bool status = default;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                if (!new ServiceController(name).Status.Equals(ServiceControllerStatus.Stopped))
                {
                    status = true;
                }
            }
            catch (Exception)
            {
                return status;
            }
        }
        return status;
    }
    public static async Task CreateAsync(string fullPath, string text) => await text.WriteLineAsync(fullPath);
    public static string Execute(string fullPath, string fileName)
    {
        using Process process = new();
        try
        {
            process.StartInfo.Verb = "runas";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = fullPath + fileName;
            {
                process.Start();
                process.WaitForExit();
                var result = process.StandardError.ReadLine();
                if (process.ExitCode is not 0 && !string.IsNullOrEmpty(result))
                {
                    result = result.Replace(Environment.NewLine, string.Empty);
                    result = result.Replace("n", string.Empty);
                    throw new Exception(result);
                }
                process.Close();
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            process.Kill();
            return e.Message;
        }
    }
    protected string Header { get; } = new[]
    {
        BatchTitle, Environment.NewLine
    }.Concat();
    protected static string FilePath
    {
        get
        {
            DefaultInterpolatedStringHandler handler = new(1, 2);
            {
                handler.AppendFormatted(ExternalPath);
                handler.AppendFormatted(Morse.BreakerRoot);
                handler.AppendFormatted("/");
                return handler.ToStringAndClear();
            }
        }
    }
    protected StringBuilder Builder { get; set; } = new();
    public required string FileName { get; init; }
    public required string ServiceName { get; init; }
}