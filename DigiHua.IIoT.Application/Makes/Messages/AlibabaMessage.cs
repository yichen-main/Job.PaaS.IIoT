using static IIoT.Application.Contracts.Architects.Services.IAlibabaService;

namespace IIoT.Application.Makes.Messages;
internal sealed class AlibabaMessage : IAlibabaService
{
    readonly IMakeLaunchWrapper _makeLaunch;
    readonly ICollectPromoter _collectPromoter;
    public AlibabaMessage(IMakeLaunchWrapper makeLaunch, ICollectPromoter collectPromoter)
    {
        _makeLaunch = makeLaunch;
        _collectPromoter = collectPromoter;
    }
    public async Task PullAsync(string connectionNo, IQueueSection.Formula formula, IMqttClient entity)
    {
        entity.ApplicationMessageReceivedAsync += (@event) => Task.Run(async () =>
        {
            try
            {
                switch (@event.ApplicationMessage.Topic.Split('/')[1])
                {
                    case var tag when tag.Equals("custom", StringComparison.OrdinalIgnoreCase):
                        {
                            var information = @event.ApplicationMessage.Payload.ToObject<Information>();
                            await _makeLaunch.Metadata.PushAsync(new IWorkshopRawdata.Title
                            {
                                SourceNo = connectionNo,
                                FactoryNo = FactoryNo,
                                GroupNo = GroupNo,
                                EquipmentNo = information.AssetCode.Replace('_', '-')
                            }, new IWorkshopRawdata.Information.Meta
                            {
                                Status = information.Value switch
                                {
                                    (int)Status.Run => nameof(IEquipment.Status.Run),
                                    (int)Status.Idle => nameof(IEquipment.Status.Idle),
                                    (int)Status.Error => nameof(IEquipment.Status.Error),
                                    (int)Status.Shutdown => nameof(IEquipment.Status.Shutdown),
                                    _ => information.Value.ToString()
                                }
                            });
                        }
                        break;

                    case var tag when tag.Equals("module", StringComparison.OrdinalIgnoreCase):
                        {
                            List<Task> tasks = new();
                            foreach (var equipment in @event.ApplicationMessage.Payload.ToObject<Parameter>().Data.GroupBy(item =>
                            item.AssetCode).ToDictionary(item => item.Key, item => item.Select(item => new
                            {
                                item.AttributeCode,
                                item.Value
                            })).ToImmutableDictionary())
                            {
                                try
                                {
                                    IWorkshopRawdata.Title title = new()
                                    {
                                        SourceNo = connectionNo,
                                        FactoryNo = FactoryNo,
                                        GroupNo = GroupNo,
                                        EquipmentNo = equipment.Key.Replace('_', '-')
                                    };
                                    List<IWorkshopRawdata.Parameter.Universal> parameters = new();
                                    foreach (var metadata in equipment.Value)
                                    {
                                        switch (metadata.AttributeCode)
                                        {
                                            case var item when item.Equals(Label.TotalQuantity, StringComparison.OrdinalIgnoreCase):
                                                if (int.TryParse(Convert.ToString(metadata.Value), out var count))
                                                {
                                                    tasks.Add(_makeLaunch.Metadata.PushAsync(title, new[]
                                                    {
                                                        new IWorkshopRawdata.Production.Meta
                                                        {
                                                            DispatchNo = string.Empty,
                                                            BatchNo = string.Empty,
                                                            Output = count
                                                        }
                                                    }));
                                                }
                                                break;

                                            default:
                                                parameters.Add(new()
                                                {
                                                    DataNo = metadata.AttributeCode,
                                                    DataValue = metadata.Value
                                                });
                                                break;
                                        }
                                    }
                                    if (parameters.Any()) tasks.Add(_makeLaunch.Metadata.PushAsync(title, parameters));
                                }
                                catch (Exception e)
                                {
                                    _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                                    {
                                        Title = nameof(AlibabaMessage).Joint(nameof(PullAsync)),
                                        Burst = $"{nameof(Parameter.Datum.AssetCode)}:{equipment.Key}",
                                        Detail = e.Message
                                    });
                                }
                            };
                            await Task.WhenAll(tasks);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                {
                    Title = nameof(QueueSection).Joint(nameof(entity.ApplicationMessageReceivedAsync)),
                    Burst = nameof(INetworkMqtt.Customer.AlibabaCloudIoT).Joint(connectionNo),
                    Detail = @event.ClientId,
                    Trace = e.Message
                });
            }
            finally
            {
                if (Floor.Tester) _collectPromoter.OnLatest(new ICollectPromoter.NativeQueueEventArgs
                {
                    Type = INetworkMqtt.Customer.AlibabaCloudIoT,
                    Topic = @event.ApplicationMessage.Topic,
                    Payload = Encoding.UTF8.GetString(@event.ApplicationMessage.Payload)
                });
            }
        });
        var option = new MqttClientOptionsBuilder().WithClientId(Guid.NewGuid().ToString())
        .WithTcpServer(formula.Ip, formula.Port).WithCredentials(formula.Username, formula.Password).WithCleanSession().Build();
        {
            await entity.ConnectAsync(option);
            await entity.SubscribeAsync(Topic.Information, MqttQualityOfServiceLevel.AtLeastOnce);
            await entity.SubscribeAsync(Topic.Parameter, MqttQualityOfServiceLevel.AtLeastOnce);
        }
    }
}