namespace IIoT.Station.Services.Runners;
internal sealed class InitializerGuard : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ReduxService.InitialAsync();
        await WaitAsync();
        _ = Task.Run(async () =>
        {
            SpinWait.SpinUntil(() => Morse.Passer);
            PeriodicTimer periodic = new(TimeSpan.FromSeconds(2));
            while (await periodic.WaitForNextTickAsync())
            {
                try
                {
                    await WorkshopRawdata.BuildAsync();
                    DateTime startTime = DateTime.UtcNow.AddHours(Timeout.Infinite), endTime = DateTime.UtcNow;
                    var informationDatas = WorkshopRawdata.Read<IWorkshopRawdata.Information>(IProcessEstablish.ProcessType.EquipmentStatus, startTime, endTime);
                    var productionDatas = WorkshopRawdata.Read<IWorkshopRawdata.Production>(IProcessEstablish.ProcessType.EquipmentOutput, startTime, endTime);
                    var parameterDatas = WorkshopRawdata.Read<IWorkshopRawdata.Parameter>(IProcessEstablish.ProcessType.EquipmentParameter, startTime, endTime);
                    foreach (var factory in await BusinessManufacture.Factory.ListAsync()) RegisterTrigger.PutFactory(factory.Id, factory.FactoryNo);
                    foreach (var group in await BusinessManufacture.FactoryGroup.ListAsync()) RegisterTrigger.PutGroup(group.FactoryId, group.Id, group.GroupNo);
                    foreach (var network in await BusinessManufacture.Network.ListAsync()) RegisterTrigger.PutNetwork(network.Id, network.SessionNo);
                    await Parallel.ForEachAsync(await BusinessManufacture.Equipment.ListAsync(), new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10
                    }, async (equipment, stoppingToken) =>
                    {
                        var network = await BusinessManufacture.Equipment.GetNetworkAsync(equipment.Id);
                        RegisterTrigger.PutEquipment(network.Id, equipment.GroupId, equipment.Id, equipment.EquipmentNo);
                        foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipment.Id))
                        {
                            switch (establish.ProcessType)
                            {
                                case IProcessEstablish.ProcessType.EquipmentStatus:
                                    {
                                        var information = await BusinessManufacture.EstablishInformation.GetAsync(establish.Id);
                                        RegisterTrigger.PutEstablishInformation(equipment.Id, establish.Id, new()
                                        {
                                            Run = information.Run,
                                            Idle = information.Idle,
                                            Error = information.Error,
                                            Setup = information.Setup,
                                            Shutdown = information.Shutdown,
                                            Maintenance = information.Maintenance,
                                            Repair = information.Repair,
                                            Hold = information.Hold
                                        });
                                        if (informationDatas.TryGetValue(equipment.EquipmentNo, out var memories))
                                        {
                                            var memory = memories.FirstOrDefault();
                                            if (information.Id != default && memory is not null)
                                            {
                                                var equipmentStatus = RegisterTrigger.ToEquipmentStatus(memory.Status);
                                                RegisterTrigger.CacheData(equipment.Id, information.Id, equipmentStatus, memory.Timestamp.ToLocalTime());
                                            }
                                        }
                                    }
                                    break;

                                case IProcessEstablish.ProcessType.EquipmentOutput:
                                    {
                                        foreach (var production in await BusinessManufacture.EstablishProduction.ListEstablishAsync(establish.Id))
                                        {
                                            RegisterTrigger.PutEstablishProduction(equipment.Id, establish.Id, production.Id, production.DispatchNo, production.BatchNo);
                                            if (productionDatas.TryGetValue(equipment.EquipmentNo, out var memories))
                                            {
                                                var memory = memories.FirstOrDefault(item => item.DispatchNo == production.DispatchNo && item.BatchNo == production.BatchNo);
                                                if (memory is not null)
                                                {
                                                    RegisterTrigger.CacheData(equipment.Id, establish.Id, production.Id, memory.DispatchNo, memory.BatchNo, memory.Output, memory.Timestamp.ToLocalTime());
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case IProcessEstablish.ProcessType.EquipmentParameter:
                                    {
                                        foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                                        {
                                            RegisterTrigger.PutEstablishParameter(equipment.Id, establish.Id, parameter.Id, parameter.DataNo);
                                            if (parameterDatas.TryGetValue(equipment.EquipmentNo, out var memories))
                                            {
                                                var memory = memories.FirstOrDefault(item => item.DataNo == parameter.DataNo);
                                                if (memory is not null)
                                                {
                                                    RegisterTrigger.CacheData(equipment.Id, establish.Id, parameter.Id, memory.DataNo, memory.DataValue, memory.Timestamp.ToLocalTime());
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    });
                    periodic.Dispose();
                }
                catch (Exception e)
                {
                    if (!Histories.Contains(e.Message)) Log.Error(Morse.HistoryDefault, nameof(InitializerGuard), new
                    {
                        e.Message
                    });
                    Histories.Add(e.Message);
                }
            }
            Morse.Meter = true;
            if (Histories.Any()) Histories.Clear();
        }, stoppingToken);
        ReduxService.Transport.ValidatingConnectionAsync += @event => Task.Run(() =>
        {
            if (@event.UserName != RunnerText.Platform.Username || @event.Password != RunnerText.Platform.Password)
            {
                @event.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            }
            else @event.ReasonCode = @event.ClientId.Length switch
            {
                <= 0 => MqttConnectReasonCode.ClientIdentifierNotValid,
                _ => MqttConnectReasonCode.Success
            };
        });
        ReduxService.Transport.InterceptingPublishAsync += @event => Task.Run(async () =>
        {
            try
            {
                var paths = @event.ApplicationMessage.Topic.Split('/');
                var text = Encoding.UTF8.GetString(@event.ApplicationMessage.Payload);
                switch (paths.Length)
                {
                    case 6:
                        ReduxService.CheckProhibitSign(paths[0], nameof(IWorkshopRawdata.Title.SourceNo));
                        ReduxService.CheckProhibitSign(paths[1], nameof(IWorkshopRawdata.Title.FactoryNo));
                        ReduxService.CheckProhibitSign(paths[2], nameof(IWorkshopRawdata.Title.GroupNo));
                        ReduxService.CheckProhibitSign(paths[3], nameof(IWorkshopRawdata.Title.EquipmentNo));
                        IWorkshopRawdata.Title title = new()
                        {
                            SourceNo = paths[0],
                            FactoryNo = paths[1],
                            GroupNo = paths[2],
                            EquipmentNo = paths[3]
                        };
                        switch (paths[4])
                        {
                            case "equipments":
                                switch (paths[5])
                                {
                                    case "informations":
                                        {
                                            var information = text.ToObject<IWorkshopRawdata.Information.Meta>();
                                            if (!string.IsNullOrWhiteSpace(information.Status)) await MakeLaunch.Metadata.PushAsync(title, new IWorkshopRawdata.Information.Meta
                                            {
                                                Status = information.Status
                                            });
                                        }
                                        break;

                                    case "productions":
                                        switch (paths[6])
                                        {
                                            case "templets":
                                                {
                                                    var productions = text.ToObject<IWorkshopRawdata.Production.Meta[]>();
                                                    if (productions is not null && productions.Any()) await MakeLaunch.Metadata.PushAsync(title, productions);
                                                }
                                                break;
                                        }
                                        break;

                                    case "parameters":
                                        {
                                            var parameters = text.ToObject<IWorkshopRawdata.Parameter.Universal[]>();
                                            if (parameters is not null && parameters.Any()) await MakeLaunch.Metadata.PushAsync(title, parameters);
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                {
                    Title = nameof(InitializerGuard),
                    Burst = nameof(ReduxService.Transport.InterceptingPublishAsync),
                    Detail = e.Message,
                    Trace = e.StackTrace
                });
            }
        });
        await ReduxService.Transport.StartAsync();
        while (await new PeriodicTimer(TimeSpan.FromSeconds(2)).WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReduxService.InitialAsync();
                if (!Morse.Passer)
                {
                    if (await BusinessFoundation.Atom.IsExistAsync(ManagerText.Hangar.Plaque))
                    {
                        await ReduxService.AddCommercialAsync(BusinessFoundation);
                        await ReduxService.AddCommercialAsync(BusinessManufacture);
                        Morse.Passer = true;
                    }
                }
                if (ReduxService.Transport is not null && ReduxService.Transport.IsStarted)
                {
                    Nameplate ??= new object().ToJson();
                    var carrier = new Carrier
                    {
                        Location = ManagerText.Hangar.Location,
                        Postgres = int.Parse(ManagerText.Hangar.Merchant),
                        Influxdb = int.Parse(ManagerText.Hangar.Flowmeter),
                        Database = ManagerText.Hangar.Plaque,
                        Username = ManagerText.Hangar.Identifier,
                        Password = ManagerText.Hangar.Pespond
                    }.ToJson();
                    if (Nameplate != carrier)
                    {
                        await ReduxService.Transport.InjectApplicationMessage(
                        new(new()
                        {
                            Retain = true,
                            Topic = nameof(Carrier),
                            Payload = Encoding.UTF8.GetBytes(carrier),
                        })
                        {
                            SenderClientId = Guid.NewGuid().ToString()
                        }, stoppingToken);
                        Nameplate = carrier;
                    }
                }
            }
            catch (NpgsqlException e)
            {
                using (CultureHelper.Use(Language ?? string.Empty))
                {
                    if (!Histories.Contains(e.Message)) Log.Fatal(Morse.HistoryDefault, nameof(InitializerGuard), new
                    {
                        Title = Fielder["db.build.exception"],
                        ManagerText.Hangar.Location,
                        ManagerText.Hangar.Merchant,
                        ManagerText.Hangar.Plaque,
                        ManagerText.Hangar.Identifier,
                        ManagerText.Hangar.Pespond,
                        e.Message
                    });
                }
                Histories.Add(e.Message);
            }
            catch (Exception e)
            {
                if (Array.IndexOf(new[] { "bucket with name workshop_processes already exists" }, e.Message) is Timeout.Infinite)
                {
                    if (!Histories.Contains(e.Message)) Log.Fatal(Morse.HistoryDefault, nameof(InitializerGuard), new
                    {
                        e.Message,
                        e.StackTrace
                    });
                    Histories.Add(e.Message);
                }
            }
        }
    }
    public required List<string> Histories { get; init; } = new();
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IMakeLaunchWrapper MakeLaunch { get; init; }
    public required ICollectPromoter CollectPromoter { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IWorkshopRawdata WorkshopRawdata { get; init; }
    public required IBusinessFoundationWrapper BusinessFoundation { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}