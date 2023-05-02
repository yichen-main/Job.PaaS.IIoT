namespace IIoT.Application.Contracts.Architects.Services;

[ServiceContract(ConfigurationName = $"Website.{nameof(IIoT)}", Namespace = "http://entry.serviceengine.cross.digihua.com")]
public interface IPlatformerService
{
    [OperationContract(AsyncPattern = true, Name = "invokeSrv", ReplyAction = "*")] Task<string> BuildAsync(string text);
}