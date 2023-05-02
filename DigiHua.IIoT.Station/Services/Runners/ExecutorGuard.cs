namespace IIoT.Station.Services.Runners;
internal sealed class ExecutorGuard : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var frequency = int.TryParse(ManagerText.Assembly.PushCycle, out var number) ? number : Mark.Found;
        PeriodicTimer periodic = new(TimeSpan.FromSeconds(frequency));
        while (await periodic.WaitForNextTickAsync(stoppingToken))
        {
            if (!Morse.Passer || !Morse.Meter) continue;
            ICollectPromoter.BackgroundEventArgs history = new()
            {
                Name = nameof(ExecutorGuard)
            };
            var watch = Stopwatch.GetTimestamp();
            try
            {
                await Task.WhenAll(new[]
                {
                    Task.Run(async ()=>
                    {
                        if(RegisterTrigger.Pass())
                        {
                            foreach (var network in RegisterTrigger.ListNetwork())
                            {
                                if(RegisterTrigger.Pass()) await BusinessManufacture.Network.AddAsync(new()
                                {
                                    Id = network.Value,
                                    SessionNo = network.Key,
                                    SessionName = string.Empty,
                                    CategoryType = INetwork.Category.PassiveReception,
                                    Creator = ITacticExpert.Automatic,
                                    CreateTime = DateTime.UtcNow
                                });
                            }
                            foreach (var factory in RegisterTrigger.ListFactory())
                            {
                                if(RegisterTrigger.Pass()) await BusinessManufacture.Factory.AddAsync(new()
                                {
                                    Id = factory.Value,
                                    FactoryNo = factory.Key,
                                    FactoryName = string.Empty,
                                    Creator = ITacticExpert.Automatic,
                                    CreateTime = DateTime.UtcNow
                                });
                            }
                            foreach (var group in RegisterTrigger.ListGroup())
                            {
                                if(RegisterTrigger.Pass()) await BusinessManufacture.FactoryGroup.AddAsync(new()
                                {
                                    Id = group.Value.groupId,
                                    FactoryId = group.Value.factoryId,
                                    GroupNo = group.Key,
                                    GroupName = string.Empty,
                                    Creator = ITacticExpert.Automatic,
                                    CreateTime = DateTime.UtcNow
                                });
                            }
                            foreach (var equipment in RegisterTrigger.ListEquipment())
                            {
                                if(RegisterTrigger.Pass()) await BusinessManufacture.Equipment.AddAsync(new()
                                {
                                    Id = equipment.Value.equipmentId,
                                    GroupId = equipment.Value.groupId,
                                    NetworkId = equipment.Value.networkId,
                                    OperateType = Operate.Enable,
                                    EquipmentNo = equipment.Key,
                                    EquipmentName = string.Empty,
                                    Creator = ITacticExpert.Automatic,
                                    CreateTime = DateTime.UtcNow
                                });
                                var (establishInformationId, mapper) = RegisterTrigger.GetEquipmentStatus(equipment.Value.equipmentId);
                                if(RegisterTrigger.Pass() && establishInformationId != default && mapper != default)
                                {
                                    await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                    {
                                        Id = establishInformationId,
                                        EquipmentId = equipment.Value.equipmentId,
                                        ProcessType= IProcessEstablish.ProcessType.EquipmentStatus,
                                        Creator = ITacticExpert.Automatic,
                                        CreateTime = DateTime.UtcNow
                                    }, new IEstablishInformation.Entity
                                    {
                                        Id = establishInformationId,
                                        Run = mapper.Run,
                                        Idle = mapper.Idle,
                                        Error = mapper.Error,
                                        Setup = mapper.Setup,
                                        Shutdown = mapper.Shutdown,
                                        Maintenance = mapper.Maintenance,
                                        Repair = mapper.Repair,
                                        Hold = mapper.Hold
                                    });
                                }
                                var (establishProductionId, orders) = RegisterTrigger.GetEquipmentOrder(equipment.Value.equipmentId);
                                if(establishProductionId != default)
                                {
                                    List<IEstablishProduction.Entity> productions = new();
                                    foreach (var (processId, dispatchNo, batchNo) in orders) productions.Add(new()
                                    {
                                        Id = processId,
                                        EstablishId = establishProductionId,
                                        DispatchNo = dispatchNo,
                                        BatchNo = batchNo
                                    });
                                    if(RegisterTrigger.Pass() && productions.Any()) await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                    {
                                        Id = establishProductionId,
                                        EquipmentId = equipment.Value.equipmentId,
                                        ProcessType= IProcessEstablish.ProcessType.EquipmentOutput,
                                        Creator = ITacticExpert.Automatic,
                                        CreateTime = DateTime.UtcNow
                                    }, productions);
                                }
                                var (establishParameterId, datas) = RegisterTrigger.GetDashboardData(equipment.Value.equipmentId);
                                if(establishParameterId != default)
                                {
                                    List<IEstablishParameter.Entity> parameters = new();
                                    foreach (var (processId, dataNo) in datas) parameters.Add(new()
                                    {
                                        Id = processId,
                                        EstablishId = establishParameterId,
                                        DataNo = dataNo
                                    });
                                    if(RegisterTrigger.Pass() && parameters.Any()) await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                    {
                                        Id = establishParameterId,
                                        EquipmentId = equipment.Value.equipmentId,
                                        ProcessType= IProcessEstablish.ProcessType.EquipmentParameter,
                                        Creator = ITacticExpert.Automatic,
                                        CreateTime = DateTime.UtcNow
                                    }, parameters);
                                }
                            }
                        }
                    }, stoppingToken),
                    Task.Run(async () =>
                    {
                        List<IExecutorEvent.InformationMission> examInformations = new();
                        List<IExecutorEvent.InformationMission> formalInformations = new();
                        List<IExecutorEvent.ProductionMission> examProductions = new();
                        List<IExecutorEvent.ProductionMission> formalProductions = new();
                        List<IExecutorEvent.ParameterMission> examParameters = new();
                        List<IExecutorEvent.ParameterMission> formalParameters = new();
                        foreach (var missionPush in await BusinessManufacture.MissionPush.ListAsync())
                        {
                            if (missionPush.OperateType is Operate.Disable) continue;
                            var mission = await BusinessManufacture.Mission.GetAsync(missionPush.Id);
                            var equipment = await BusinessManufacture.Equipment.GetAsync(mission.EquipmentId);
                            foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(mission.EquipmentId))
                            {
                                var workshopData = await BusinessManufacture.ProcessEstablish.GetWorkshopDataAsync(establish.Id);
                                if (workshopData.Information.Id != default)
                                {
                                    if (missionPush.EnvironmentType is IMissionPush.EnvironmentType.Experiment) examInformations.Add(new()
                                    {
                                        Equipment = equipment,
                                        Push = missionPush,
                                        Stack = await BusinessManufacture.InformationStack.GetAsync(establish.Id)
                                    });
                                    formalInformations.Add(new()
                                    {
                                        Equipment = equipment,
                                        Push = missionPush,
                                        Stack = await BusinessManufacture.InformationStack.GetAsync(establish.Id)
                                    });
                                }
                                List<(IEstablishProduction.Entity entity, IProductionStack.Entity stack)> productionDetails = new();
                                foreach (var production in workshopData.Productions)
                                {
                                    productionDetails.Add((production, await BusinessManufacture.ProductionStack.GetAsync(production.Id)));
                                }
                                if (missionPush.EnvironmentType is IMissionPush.EnvironmentType.Experiment) examProductions.Add(new()
                                {
                                    Equipment = equipment,
                                    Push = missionPush,
                                    Details = productionDetails
                                });
                                else formalProductions.Add(new()
                                {
                                    Equipment = equipment,
                                    Push = missionPush,
                                    Details = productionDetails
                                });
                                List<(IEstablishParameter.Entity entity, IParameterStack.Entity stack)> parameterDetails = new();
                                foreach (var parameter in workshopData.Parameters)
                                {
                                    parameterDetails.Add((parameter, await BusinessManufacture.ParameterStack.GetAsync(parameter.Id)));
                                }
                                if (missionPush.EnvironmentType is IMissionPush.EnvironmentType.Experiment) examParameters.Add(new()
                                {
                                    Equipment = equipment,
                                    Push = missionPush,
                                    Details = parameterDetails
                                });
                                else formalParameters.Add(new()
                                {
                                    Equipment = equipment,
                                    Push = missionPush,
                                    Details = parameterDetails
                                });
                            }
                        }
                        List<Task> results = new();
                        if (examInformations.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Experiment, examInformations));
                        if (formalInformations.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Production, formalInformations));
                        if (examProductions.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Experiment, examProductions));
                        if (formalProductions.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Production, formalProductions));
                        if (examParameters.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Experiment, examParameters));
                        if (formalParameters.Any()) results.Add(ExecutorEvent.BeginAsync(IMissionPush.EnvironmentType.Production, formalParameters));
                        await Task.WhenAll(results);
                    }, stoppingToken)
                });
                if (Histories.Any()) Histories.Clear();
            }
            catch (NpgsqlException e)
            {
                history.Store = e.Message;
            }
            catch (Exception e)
            {
                history.Detail = e.Message;
                history.Trace = e.StackTrace ?? string.Empty;
            }
            finally
            {
                var issued = false;
                history.ConsumeTime = (long)Stopwatch.GetElapsedTime(watch).TotalMilliseconds;
                if (!string.IsNullOrEmpty(history.Store) && !Histories.Contains(history.Store))
                {
                    issued = true;
                    Histories.Add(history.Store);
                }
                if (!string.IsNullOrEmpty(history.Detail) && !Histories.Contains(history.Detail))
                {
                    issued = true;
                    Histories.Add(history.Detail);
                }
                if (issued) CollectPromoter.OnLatest(history);
                if (int.TryParse(ManagerText.Assembly.PushCycle, out var pushCycle))
                {
                    if (frequency != pushCycle) periodic.Dispose();
                }
            }
        }
        await RestartAsync(stoppingToken);
    }
    async Task RestartAsync(CancellationToken stoppingToken) => await ExecuteAsync(stoppingToken);
    public required List<string> Histories { get; init; } = new();
    public required IExecutorEvent ExecutorEvent { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required ICollectPromoter CollectPromoter { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}