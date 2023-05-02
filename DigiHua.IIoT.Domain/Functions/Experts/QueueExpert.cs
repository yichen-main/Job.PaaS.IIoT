namespace IIoT.Domain.Functions.Experts;
public abstract class QueueExpert : IQueueExpert
{
    public Task PushAsync<T>(T entity) => Task.Run(async () =>
    {
        using var client = new MqttFactory().CreateMqttClient();
        var option = new MqttClientOptionsBuilder().WithTcpServer(Ip, Port).WithClientId(ClientId)
        .WithCredentials(Username, Password).WithCleanSession().Build();
        await client.ConnectAsync(option);
        await client.PublishAsync(new MqttApplicationMessageBuilder().WithTopic(Topic).WithPayload(entity.ToJson())
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build());
    });
    public int Port { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}