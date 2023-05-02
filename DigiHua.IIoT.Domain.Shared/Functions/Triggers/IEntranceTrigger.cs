namespace IIoT.Domain.Shared.Functions.Triggers;
public interface IEntranceTrigger
{
    ValueTask PushAsync();
    static string Launcher => "service";
    static string Operater => "\u00A0[OPEN]";
    static string Shutdown => "\u00A0[SHUT]";
    static string Timeout => "timeout /t 5";
    static string Title => "@echo off<nul 3>nul";
    static string Installation => $"{ResourcePath}dotnet.exe";
    static string Administrator => @"%1 Mshta vbscript:CreateObject(""Shell.Application"").ShellExecute(""Cmd.exe"",""/C """"%~0"""" ::"","""",""runas"",1)(window.close)&&exit";
}
public interface IEntranceTrigger<TEntity, TResult>
{
    ValueTask<TResult> PushAsync(TEntity entity);
}