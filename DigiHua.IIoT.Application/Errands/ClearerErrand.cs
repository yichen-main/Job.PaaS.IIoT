namespace IIoT.Application.Makes.Errands;
internal sealed class ClearerErrand : TacticExpert, IClearerEvent
{
    public async Task UserAsync(IEnumerable<Guid> ids)
    {
        List<(string content, object? @object)> results = new();
        foreach (var id in ids) results.Add((NpgsqlUtility.MarkDelete<IUser.Entity>(id), default));
        await TransactionAsync(results);
    }
    public async Task FactoryAsync(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            RegisterTrigger.RemoveFactory(id);
            var groups = await BusinessManufacture.FactoryGroup.ListFactoryAsync(id);
            await FactoryGroupAsync(groups.Select(item => item.Id));
            await ExecuteAsync(NpgsqlUtility.MarkDelete<IFactory.Entity>(id), default, Morse.Passer);
        }
    }
    public async Task FactoryGroupAsync(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            RegisterTrigger.RemoveGroup(id);
            var equipments = await BusinessManufacture.Equipment.ListGroupAsync(id);
            await EquipmentAsync(equipments.Select(item => item.Id));
            await ExecuteAsync(NpgsqlUtility.MarkDelete<IFactoryGroup.Entity>(id), default, Morse.Passer);
        }
    }
    public async Task EquipmentAsync(IEnumerable<Guid> ids)
    {
        List<(string content, object? @object)> results = new();
        foreach (var id in ids)
        {
            var equipment = await BusinessManufacture.Equipment.GetAsync(id);
            if (equipment.Id != default)
            {
                RegisterTrigger.RemoveEquipment(equipment.EquipmentNo);
                var relation = await BusinessManufacture.Equipment.GetScavengerAsync(equipment.Id);
                await Task.WhenAll(new Task[]
                {
                    MissionAsync(relation.Missions),
                    EquipmentAlarmAsync(relation.Alarms.Select(item => item.Id)),
                    ProcessEstablishAsync(await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipment.Id))
                });
                await DigitalSection.RemoveLinkAsync(equipment.NetworkId, equipment.Id);
                results.Add((NpgsqlUtility.MarkDelete<IEquipment.Entity>(id), default));
            }
        }
        await TransactionAsync(results);
    }
    public async Task EquipmentAlarmAsync(IEnumerable<Guid> ids)
    {
        List<(string content, object? @object)> results = new();
        foreach (var id in ids) results.Add((NpgsqlUtility.MarkDelete<IEquipmentAlarm.Entity>(id), default));
        await TransactionAsync(results);
    }
    public async Task NetworkAsync(IEnumerable<INetwork.Entity> entities)
    {
        await Parallel.ForEachAsync(entities, new ParallelOptions
        {
            MaxDegreeOfParallelism = 2
        }, async (entity, _) =>
        {
            var equipments = await BusinessManufacture.Equipment.ListNetworkAsync(entity.Id);
            await EquipmentAsync(equipments.Select(item => item.Id));
            List<(string content, object? @object)> results = new()
            {
                (NpgsqlUtility.MarkDelete<INetwork.Entity>(entity.Id), default)
            };
            switch (entity.CategoryType)
            {
                case INetwork.Category.MessageQueuingTelemetryTransport:
                    results.Add((NpgsqlUtility.MarkDelete<INetworkMqtt.Entity>(entity.Id), default));
                    break;

                case INetwork.Category.OPCUnifiedArchitecture:
                    results.Add((NpgsqlUtility.MarkDelete<INetworkOpcUa.Entity>(entity.Id), default));
                    break;
            }
            RegisterTrigger.RemoveNetwork(entity.SessionNo);
            await TransactionAsync(results);
            switch (entity.CategoryType)
            {
                case INetwork.Category.MessageQueuingTelemetryTransport:
                    QueueSection.Clear(entity.Id);
                    break;

                case INetwork.Category.OPCUnifiedArchitecture:
                    DigitalSection.Clear(entity.Id);
                    break;
            }
        });
    }
    public async Task MissionAsync(IEnumerable<IMission.Entity> entities)
    {
        List<(string content, object? @object)> results = new();
        foreach (var entity in entities)
        {
            results.Add((NpgsqlUtility.MarkDelete<IMission.Entity>(entity.Id), default));
            results.Add((NpgsqlUtility.MarkDelete<IMissionPush.Entity>(entity.Id), default));
        }
        await TransactionAsync(results);
    }
    public async Task ProcessEstablishAsync(IEnumerable<IProcessEstablish.Entity> entities)
    {
        await Parallel.ForEachAsync(entities, new ParallelOptions
        {
            MaxDegreeOfParallelism = 10
        }, async (establish, _) =>
        {
            switch (establish.ProcessType)
            {
                case IProcessEstablish.ProcessType.EquipmentStatus:
                    var information = await BusinessManufacture.EstablishInformation.GetAsync(establish.Id);
                    await EstablishInformationAsync(information);
                    RegisterTrigger.RemoveInformation(establish.EquipmentId);
                    break;

                case IProcessEstablish.ProcessType.EquipmentOutput:
                    foreach (var production in await BusinessManufacture.EstablishProduction.ListEstablishAsync(establish.Id))
                    {
                        await EstablishProductionAsync(production);
                    }
                    RegisterTrigger.RemoveProduction(establish.EquipmentId);
                    break;

                case IProcessEstablish.ProcessType.EquipmentParameter:
                    foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                    {
                        await EstablishParameterAsync(parameter);
                    }
                    RegisterTrigger.RemoveParameter(establish.EquipmentId);
                    break;
            }
        });
    }
    public async Task EstablishInformationAsync(IEstablishInformation.Entity entity)
    {
        List<(string content, object? @object)> results = new()
        {
            (NpgsqlUtility.MarkDelete<IEstablishInformation.Entity>(entity.Id), default),
            (NpgsqlUtility.MarkDelete<IInformationStack.Entity>(entity.Id), default)
        };
        var establish = await BusinessManufacture.ProcessEstablish.GetAsync(entity.Id);
        var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
        var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(entity.Id);
        if (opcUa.Id != default)
        {
            var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
            DigitalSection.RemoveItem(network.Id, equipment.Id);
            results.Add((NpgsqlUtility.MarkDelete<IOpcUaProcess.Entity>(entity.Id), default));
        }
        await TransactionAsync(results);
        await ExecuteAsync(NpgsqlUtility.MarkDelete<IProcessEstablish.Entity>(establish.Id), default, Morse.Passer);
    }
    public async Task EstablishProductionAsync(IEstablishProduction.Entity entity)
    {
        List<(string content, object? @object)> results = new()
        {
            (NpgsqlUtility.MarkDelete<IEstablishProduction.Entity>(entity.Id), default),
            (NpgsqlUtility.MarkDelete<IProductionStack.Entity>(entity.Id), default)
        };
        var establish = await BusinessManufacture.ProcessEstablish.GetAsync(entity.EstablishId);
        var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
        var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(entity.Id);
        if (opcUa.Id != default)
        {
            var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
            DigitalSection.RemoveItem(network.Id, equipment.Id);
            results.Add((NpgsqlUtility.MarkDelete<IOpcUaProcess.Entity>(entity.Id), default));
        }
        await TransactionAsync(results);
        await ExecuteAsync(NpgsqlUtility.MarkDelete<IProcessEstablish.Entity>(establish.Id), default, Morse.Passer);
    }
    public async Task EstablishParameterAsync(IEstablishParameter.Entity entity)
    {
        List<(string content, object? @object)> results = new()
        {
            (NpgsqlUtility.MarkDelete<IEstablishParameter.Entity>(entity.Id), default),
            (NpgsqlUtility.MarkDelete<IParameterStack.Entity>(entity.Id), default)
        };
        var establish = await BusinessManufacture.ProcessEstablish.GetAsync(entity.EstablishId);
        var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
        var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(entity.Id);
        if (opcUa.Id != default)
        {
            var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
            DigitalSection.RemoveItem(network.Id, equipment.Id);
            results.Add((NpgsqlUtility.MarkDelete<IOpcUaProcess.Entity>(entity.Id), default));
        }
        await TransactionAsync(results);
        await ExecuteAsync(NpgsqlUtility.MarkDelete<IProcessEstablish.Entity>(establish.Id), default, Morse.Passer);
    }
    public required IQueueSection QueueSection { get; init; }
    public required INpgsqlUtility NpgsqlUtility { get; init; }
    public required IDigitalSection DigitalSection { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}