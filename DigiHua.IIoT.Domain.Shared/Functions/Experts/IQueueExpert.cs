namespace IIoT.Domain.Shared.Functions.Experts;
public interface IQueueExpert
{
    Task PushAsync<T>(T entity);
    int Port { get; set; }
    string Ip { get; set; }
    string Topic { get; set; }
    string ClientId { get; set; }
    string Username { get; set; }
    string Password { get; set; }
}