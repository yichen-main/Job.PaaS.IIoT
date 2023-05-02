namespace IIoT.Domain.Functions.Experts;
public abstract class DriverExpert : DriverMedium
{
    protected DriverExpert(string file, string service, string path)
    {
        FileName = file;
        ServiceName = nameof(Morse.DigiHua).Joint(nameof(IIoT)).Joint(service);
        Builder.AppendLine($""" 
        %1 Mshta vbscript:CreateObject("Shell.Application").ShellExecute("Cmd.exe","/C ""%~0"" ::","","runas",1)(window.close)&&exit
        cd\
        {char.ToLower(RootLocation.FirstOrDefault())}:
        cd {path}
        set environment=%~dp0
        set path=%environment%;%path%
        set execute={Officer}
        set filename={FileName}
        set service={ServiceName}
        set basePath=%cd%
        """);
    }
    public async ValueTask CreateStarterAsync()
    {
        Builder.AppendLine("""%basePath%\%execute% install "%service%" "%basePath%\%filename%" & net start "%service%" """);
        Builder.Append("timeout /t 1");
        Builder.Insert(default, Header);
        await CreateAsync(new[]
        {
            FilePath, ServiceName, Boot
        }.Concat().Joint(Extension), Builder.ToString());
    }
    public async ValueTask CreateStopperAsync()
    {
        Builder.AppendLine(@"net stop %service% & %basePath%\%execute% remove %service% confirm");
        Builder.Append("timeout /t 1");
        Builder.Insert(default, Header);
        await CreateAsync(new[]
        {
            FilePath, ServiceName, Shutdown
        }.Concat().Joint(Extension), Builder.ToString());
    }
    public async ValueTask CreateRestarterAsync()
    {
        Builder.AppendLine(@"%basePath%\%execute% restart %service%");
        Builder.Append("timeout /t 1");
        Builder.Insert(default, Header);
        await CreateAsync(new[]
        {
            FilePath, ServiceName, Reboot
        }.Concat().Joint(Extension), Builder.ToString());
    }
}