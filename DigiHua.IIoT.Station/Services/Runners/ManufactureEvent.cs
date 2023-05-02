using static IIoT.Application.Contracts.Architects.Events.IExecutorEvent;
using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Services.Runners;

[Dependency(ServiceLifetime.Singleton)]
internal sealed class ManufactureEvent : IManufactureEvent
{
    public async Task QueueBrokerAsync()
    {
        try
        {
            var protocols = await BusinessManufacture.NetworkMqtt.ListAsync();
            await Parallel.ForEachAsync(await BusinessManufacture.Network.ListAsync(INetwork.Category.MessageQueuingTelemetryTransport), new ParallelOptions
            {
                MaxDegreeOfParallelism = 3
            }, async (root, stoppingToken) =>
            {
                var protocol = protocols.First(item => item.Id == root.Id);
                string name = Terminology[Enum.GetName(typeof(INetworkMqtt.Customer), protocol.CustomerType) ?? string.Empty];
                try
                {
                    if (QueueSection.Providers.TryGetValue(root.Id, out var provider))
                    {
                        var ip = provider.formula.Ip != protocol.Ip;
                        var port = provider.formula.Port != protocol.Port;
                        var username = provider.formula.Username != protocol.Username;
                        var password = provider.formula.Password != protocol.Password;
                        var customer = provider.type != protocol.CustomerType;
                        if (ip || port || username || password || customer) await QueueSection.OpenAsync(protocol.CustomerType, root.Id, root.SessionNo, new()
                        {
                            Ip = protocol.Ip,
                            Port = protocol.Port,
                            SessionsNo = root.SessionNo,
                            Username = protocol.Username,
                            Password = protocol.Password
                        });
                    }
                    else await QueueSection.OpenAsync(protocol.CustomerType, root.Id, root.SessionNo, new()
                    {
                        Ip = protocol.Ip,
                        Port = protocol.Port,
                        SessionsNo = root.SessionNo,
                        Username = protocol.Username,
                        Password = protocol.Password
                    });
                }
                catch (Exception e)
                {
                    if (!QueueBrokers.Contains(e.Message)) CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                    {
                        Title = nameof(ManufactureEvent),
                        Burst = name.Joint(protocol.Ip).Joint(protocol.Port.ToString(), ":"),
                        Trace = $"{nameof(root.SessionNo)}:{root.SessionNo}",
                        Detail = e.Message
                    });
                    QueueBrokers.Add(e.Message);
                }
            });
            if (QueueBrokers.Any()) DigitalTwins.Clear();
        }
        catch (Exception e)
        {
            if (!QueueBrokers.Contains(e.Message)) CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
            {
                Title = nameof(ManufactureEvent),
                Burst = nameof(QueueBrokerAsync),
                Detail = e.Message,
                Trace = e.StackTrace ?? string.Empty
            });
            QueueBrokers.Add(e.Message);
        }
    }
    public async Task DigitalTwinAsync()
    {
        try
        {
            await Parallel.ForEachAsync(await BusinessManufacture.Network.ListAsync(INetwork.Category.OPCUnifiedArchitecture), new ParallelOptions
            {
                MaxDegreeOfParallelism = 3
            }, async (network, stoppingToken) =>
            {
                var contingent = await BusinessManufacture.NetworkOpcUa.GetContingentAsync(network.Id);
                if (DigitalSection.Mains.TryGetValue(network.Id, out var oldTitle))
                {
                    var varySessionNo = network.SessionNo != oldTitle.SessionNo;
                    var varyEndpoint = contingent.Entity.Endpoint != oldTitle.Endpoint;
                    var varyUsername = contingent.Entity.Username != oldTitle.Username;
                    var varyPassword = contingent.Entity.Password != oldTitle.Password;
                    if (varySessionNo || varyEndpoint || varyUsername || varyPassword)
                    {
                        DigitalSection.Clear(network.Id);
                        var entity = await DigitalSection.OpenAsync(network.Id, network.SessionNo, contingent.Entity);
                        if (entity is not null) DigitalSection.Mains.TryAdd(network.Id, new()
                        {
                            SessionNo = network.SessionNo,
                            Endpoint = contingent.Entity.Endpoint,
                            Username = contingent.Entity.Username,
                            Password = contingent.Entity.Password,
                            Entity = entity
                        });
                    }
                }
                else
                {
                    var entity = await DigitalSection.OpenAsync(network.Id, network.SessionNo, contingent.Entity);
                    if (entity is not null) DigitalSection.Mains.TryAdd(network.Id, new()
                    {
                        SessionNo = network.SessionNo,
                        Endpoint = contingent.Entity.Endpoint,
                        Username = contingent.Entity.Username,
                        Password = contingent.Entity.Password,
                        Entity = entity
                    });
                }
                var main = DigitalSection.Mains.FirstOrDefault(item => item.Key == network.Id);
                if (main.Key != default)
                {
                    foreach (var equipment in contingent.Equipments)
                    {
                        List<IDigitalSection.Formula> formulas = new();
                        var singleFactoryAsync = BusinessManufacture.FactoryGroup.GetSingleFactoryAsync(equipment.GroupId);
                        foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipment.Id))
                        {
                            switch (establish.ProcessType)
                            {
                                case IProcessEstablish.ProcessType.EquipmentStatus:
                                    {
                                        var information = await BusinessManufacture.EstablishInformation.GetAsync(establish.Id);
                                        var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(information.Id);
                                        formulas.Add(new()
                                        {
                                            EquipmentId = equipment.Id,
                                            EstablishId = establish.Id,
                                            ProcessId = information.Id,
                                            DataNo = new IEstablishInformation.StatusLabel
                                            {
                                                Run = information.Run,
                                                Idle = information.Idle,
                                                Error = information.Error,
                                                Setup = information.Setup,
                                                Shutdown = information.Shutdown,
                                                Maintenance = information.Maintenance,
                                                Repair = information.Repair,
                                                Hold = information.Hold
                                            }.ToJson(),
                                            NodePath = opcUa.NodePath,
                                            EaiType = IWorkshopRawdata.EaiType.Information
                                        });
                                    }
                                    break;

                                case IProcessEstablish.ProcessType.EquipmentOutput:
                                    {
                                        foreach (var production in await BusinessManufacture.EstablishProduction.ListEstablishAsync(establish.Id))
                                        {
                                            var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(production.Id);
                                            formulas.Add(new()
                                            {
                                                EquipmentId = equipment.Id,
                                                EstablishId = establish.Id,
                                                ProcessId = production.Id,
                                                DataNo = nameof(IProcessEstablish.ProcessType.EquipmentOutput),
                                                NodePath = opcUa.NodePath,
                                                EaiType = IWorkshopRawdata.EaiType.Production
                                            });
                                        }
                                    }
                                    break;

                                case IProcessEstablish.ProcessType.EquipmentParameter:
                                    {
                                        foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                                        {
                                            var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(parameter.Id);
                                            formulas.Add(new()
                                            {
                                                EquipmentId = equipment.Id,
                                                EstablishId = establish.Id,
                                                ProcessId = parameter.Id,
                                                DataNo = parameter.DataNo,
                                                NodePath = opcUa.NodePath,
                                                EaiType = IWorkshopRawdata.EaiType.Parameter
                                            });
                                        }
                                    }
                                    break;
                            }
                        }
                        var singleFactory = await singleFactoryAsync;
                        if (DigitalSection.Links.TryGetValue(equipment.Id, out _))
                        {
                            if (DigitalSection.Providers.TryGetValue((network.Id, equipment.Id), out var oldFormulas))
                            {
                                if (!FoundationTrigger.CheckParity(formulas, oldFormulas))
                                {
                                    await DigitalSection.RemoveLinkAsync(network.Id, equipment.Id);
                                    await DigitalSection.BuildAsync(equipment, DigitalSection.AddItem(DigitalSection.AddLink(main.Value.Entity, new()
                                    {
                                        SourceNo = network.SessionNo,
                                        FactoryNo = singleFactory.Factory.FactoryNo,
                                        GroupNo = singleFactory.Entity.GroupNo,
                                        EquipmentNo = equipment.EquipmentNo
                                    }), formulas), formulas, stoppingToken);
                                }
                            }
                        }
                        else await DigitalSection.BuildAsync(equipment, DigitalSection.AddItem(DigitalSection.AddLink(main.Value.Entity, new()
                        {
                            SourceNo = network.SessionNo,
                            FactoryNo = singleFactory.Factory.FactoryNo,
                            GroupNo = singleFactory.Entity.GroupNo,
                            EquipmentNo = equipment.EquipmentNo
                        }), formulas), formulas, stoppingToken);
                    }
                }
            });
            if (DigitalTwins.Any()) DigitalTwins.Clear();
        }
        catch (Exception e)
        {
            using (CultureHelper.Use(Language))
            {
                if (!DigitalTwins.Contains(e.Message)) CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                {
                    Title = nameof(DigitalTwinAsync),
                    Burst = e.Message,
                    Detail = e.Message switch
                    {
                        var item when item.Contains(nameof(Opc.Ua.StatusCodes.BadNotConnected)) => Fielder["wrong.server.connection"],
                        var item when item.Contains(nameof(Opc.Ua.StatusCodes.BadTooManySubscriptions)) => Fielder["wrong.server.max.subscription"],
                        _ => e.StackTrace ?? string.Empty
                    }
                });
                DigitalTwins.Add(e.Message);
            }
        }
    }
    public async Task ConfidentialAsync(IMissionPush.EnvironmentType type)
    {
        try
        {
            var entities = await ExecutorEvent.SendAsync(FoundationTrigger.UseSerializerXml(new EaiRequest
            {
                Host = new()
                {
                    Prod = IManufactureClient.Label.ProdName
                },
                Payload = new()
                {
                    Param = new()
                    {
                        Key = IManufactureClient.Label.StandardData,
                        Type = IManufactureClient.Label.TextType,
                        DataRequest = new()
                        {
                            Datainfo = new()
                            {
                                DataParameter = new()
                                {
                                    Key = MESState.ParameterKey,
                                    Type = IManufactureClient.Label.ParamType,
                                    Data = new()
                                    {
                                        Name = MESState.ParameterDataName,
                                        ParameterRows = new()
                                        {
                                            new()
                                            {
                                                Seq = "1",
                                                Fields = new()
                                                {
                                                    new()
                                                    {
                                                        Name = "machine_no",
                                                        Type = "string"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Service = new()
                {
                    Name = "workshop.machine.status"
                }
            }), type);
            if (entities.Any()) await ReduxService.Transport.InjectApplicationMessage(
            new(new()
            {
                Retain = true,
                Topic = type switch
                {
                    IMissionPush.EnvironmentType.Production => "smes/formal-environment/workshops/productions/states",
                    _ => "smes/test-environment/workshops/productions/states"
                },
                Payload = Encoding.UTF8.GetBytes(entities.ToJson()),
            })
            {
                SenderClientId = Guid.NewGuid().ToString()
            });
        }
        catch (Exception) { }
    }
    public required List<string> QueueBrokers { get; init; } = new();
    public required List<string> DigitalTwins { get; init; } = new();
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IExecutorEvent ExecutorEvent { get; init; }
    public required IQueueSection QueueSection { get; init; }
    public required IDigitalSection DigitalSection { get; init; }
    public required ICollectPromoter CollectPromoter { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}