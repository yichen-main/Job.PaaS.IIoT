namespace IIoT.Retinue.Guards;
internal sealed class WatchmanGuard : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await new PeriodicTimer(TimeSpan.FromSeconds(5)).WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                //await new RunnerProfile().BuildAsync();
                {
                    if (RunnerText.Broker.Enable)
                    {
                        //BrokerIp = RunnerText.Broker.Ip;
                        BrokerPort = RunnerText.Broker.Port;
                        BrokerUsername = RunnerText.Broker.Username;
                        BrokerPassword = RunnerText.Broker.Password;
                        BusinessEnable = !string.IsNullOrEmpty(CarrierText.Database);
                        {
                            if (Broker is null)
                            {
                                Broker = new BrokerActuator();
                                await Broker.BuildAsync();
                            }
                            else
                            {
                                //var ip = Broker.Ip != BrokerIp;
                                var port = Broker.Port != BrokerPort;
                                var username = Broker.Username != BrokerUsername;
                                var password = Broker.Password != BrokerPassword;
                                //if (ip || port || username || password)
                                //{
                                //    Broker.Dispose();
                                //    {
                                //        Broker = null;
                                //        {
                                //            Broker = new BrokerActuator();
                                //            await Broker.BuildAsync();
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
                if (Histories.Any()) Histories.Clear();
            }
            catch (Exception e)
            {
                //if (!Histories.Exists(item => item == e.Message)) new ICollectPromoter.CollectiveEventArgs()
                //{
                //    Title = nameof(WatchmanGuard).Joint(nameof(ExecuteAsync)),
                //    Burst = e.Message
                //}.OnLatest();
                Histories.Add(e.Message);
            }
        }
    }
    sealed class BrokerActuator : IDisposable
    {
        public BrokerActuator()
        {
            Ip = "BrokerIp";
            Port = BrokerPort;
            Username = BrokerUsername;
            Password = BrokerPassword;
        }
        internal async Task BuildAsync()
        {
            using var client = new MqttFactory().CreateMqttClient();
            {
                client.ConnectedAsync += @event =>
                {
                    return Task.CompletedTask;
                };
                client.DisconnectedAsync += async @event =>
                {
                    await Task.Delay(10 * 1000);
                    if (!Shutdown) await BuildAsync();
                };
                client.ApplicationMessageReceivedAsync += @event =>
                {
                    try
                    {
                        CarrierText = @event.ApplicationMessage.Payload.ToObject<Carrier>();
                        {
                            StoreIp = CarrierText.Ip;
                            StoreUsername = CarrierText.Username;
                            StorePassword = CarrierText.Password;
                            InfluxUrl = StoreIp.AddURL(CarrierText.InfluxdbPort);
                            ConnectionString = StoreIp.Nameplate(CarrierText.PostgresPort,
                            CarrierText.Database, StoreUsername, StorePassword);
                        }
                    }
                    catch (Exception)
                    {
                        //new OriginateMedium.CollectiveEventArgs()
                        //{
                        //    Title = nameof(WatchmanGuard).Joint(nameof(Client.ApplicationMessageReceivedAsync)),
                        //    Burst = @event.ClientId,
                        //    Detail = e.Message
                        //}.OnLatest();
                    }
                    return Task.CompletedTask;
                };
                var option = new MqttClientOptionsBuilder().WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(Ip, Port).WithCredentials(Username, Password).WithCleanSession().Build();
                {
                    await client.ConnectAsync(option);
                    await client.SubscribeAsync(nameof(Carrier), MqttQualityOfServiceLevel.AtLeastOnce);
                    {
                        Client = client;
                    }
                }
            }
        }
        public void Dispose()
        {
            Shutdown = true;
            Client?.Dispose();
        }
        bool Shutdown { get; set; }
        IMqttClient? Client { get; set; }
        internal int Port { get; private init; }
        internal string Ip { get; private init; }
        internal string Username { get; private init; }
        internal string Password { get; private init; }
    }
    BrokerActuator? Broker { get; set; }
    public required List<string> Histories { get; init; } = new();
}