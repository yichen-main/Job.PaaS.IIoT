namespace IIoT.Domain.Functions.Triggers;
internal sealed class RegisterTrigger : IRegisterTrigger
{
    public bool Pass() => IsFactory == default && IsGroup == default && IsNetwork == default && IsEquipment == default && IsEstablish == default;
    public bool IsEquipmentStatus(int value) => Array.Exists(new int[]
    {
        (int)IEquipment.Status.Run,
        (int)IEquipment.Status.Idle,
        (int)IEquipment.Status.Error,
        (int)IEquipment.Status.Setup,
        (int)IEquipment.Status.Shutdown,
        (int)IEquipment.Status.Repair,
        (int)IEquipment.Status.Maintenance,
        (int)IEquipment.Status.Hold
    }, item => item == value);
    public IEquipment.Status ToEquipmentStatus(byte value) => value switch
    {
        (int)IEquipment.Status.Run => IEquipment.Status.Run,
        (int)IEquipment.Status.Idle => IEquipment.Status.Idle,
        (int)IEquipment.Status.Error => IEquipment.Status.Error,
        (int)IEquipment.Status.Setup => IEquipment.Status.Setup,
        (int)IEquipment.Status.Shutdown => IEquipment.Status.Shutdown,
        (int)IEquipment.Status.Repair => IEquipment.Status.Repair,
        (int)IEquipment.Status.Maintenance => IEquipment.Status.Maintenance,
        (int)IEquipment.Status.Hold => IEquipment.Status.Hold,
        _ => IEquipment.Status.Unused
    };
    public IEquipment.Status ToEquipmentStatus(string value) => value switch
    {
        "0" => IEquipment.Status.Run,
        "1" => IEquipment.Status.Idle,
        "2" => IEquipment.Status.Error,
        "3" => IEquipment.Status.Setup,
        "4" => IEquipment.Status.Shutdown,
        "5" => IEquipment.Status.Repair,
        "6" => IEquipment.Status.Maintenance,
        "7" => IEquipment.Status.Hold,
        _ => IEquipment.Status.Unused
    };
    public string AsConverter(in IEquipment.Status status) => status switch
    {
        IEquipment.Status.Run => "0",
        IEquipment.Status.Idle => "1",
        IEquipment.Status.Error => "2",
        IEquipment.Status.Setup => "3",
        IEquipment.Status.Shutdown => "4",
        IEquipment.Status.Repair => "5",
        IEquipment.Status.Maintenance => "6",
        IEquipment.Status.Hold => "7",
        _ => string.Empty
    };
    public IEquipment.Status AsStatus(in string status, in IEstablishInformation.StatusLabel mapper) => status switch
    {
        var item when string.Equals(item, mapper.Run) => IEquipment.Status.Run,
        var item when string.Equals(item, mapper.Idle) => IEquipment.Status.Idle,
        var item when string.Equals(item, mapper.Error) => IEquipment.Status.Error,
        var item when string.Equals(item, mapper.Setup) => IEquipment.Status.Setup,
        var item when string.Equals(item, mapper.Shutdown) => IEquipment.Status.Shutdown,
        var item when string.Equals(item, mapper.Repair) => IEquipment.Status.Repair,
        var item when string.Equals(item, mapper.Maintenance) => IEquipment.Status.Maintenance,
        var item when string.Equals(item, mapper.Hold) => IEquipment.Status.Hold,
        _ => IEquipment.Status.Unused
    };
    public IEstablishInformation.StatusLabel ChangeStatus(in string text)
    {
        var value = text.Replace("\"", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty);
        string run = string.Empty, idle = string.Empty, error = string.Empty, setup = string.Empty, shutdown = string.Empty;
        string repair = string.Empty, maintenance = string.Empty, hold = string.Empty;
        Array.ForEach(value.Split(','), item =>
        {
            var statuses = item.Split(":");
            if (statuses.Length is 2)
            {
                switch (statuses[default])
                {
                    case nameof(IEquipment.Status.Run):
                        run = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Idle):
                        idle = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Error):
                        error = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Setup):
                        setup = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Shutdown):
                        shutdown = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Repair):
                        repair = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Maintenance):
                        maintenance = statuses[1];
                        break;

                    case nameof(IEquipment.Status.Hold):
                        hold = statuses[1];
                        break;
                }
            }
        });
        return new IEstablishInformation.StatusLabel
        {
            Run = run,
            Idle = idle,
            Error = error,
            Setup = setup,
            Shutdown = shutdown,
            Repair = repair,
            Maintenance = maintenance,
            Hold = hold
        };
    }
    public Guid PutFactory(Guid factoryId, string factoryNo)
    {
        if (Factories.TryGetValue(factoryNo, out var value)) return value;
        return Factories.AddOrUpdate(factoryNo, factoryId, (key, value) => value);
    }
    public Guid PutGroup(Guid factoryId, Guid groupId, string groupNo)
    {
        if (Groups.TryGetValue(groupNo, out var value)) return value.groupId;
        return Groups.AddOrUpdate(groupNo, (factoryId, groupId), (key, value) => value).groupId;
    }
    public Guid PutNetwork(Guid networkId, string networkNo)
    {
        if (Networks.TryGetValue(networkNo, out var value)) return value;
        return Networks.AddOrUpdate(networkNo, networkId, (key, value) => value);
    }
    public Guid PutEquipment(Guid networkId, Guid groupId, Guid equipmentId, string equipmentNo)
    {
        if (Equipments.TryGetValue(equipmentNo, out var value)) return value.equipmentId;
        return Equipments.AddOrUpdate(equipmentNo, (networkId, groupId, equipmentId), (key, value) => (networkId, groupId, equipmentId)).equipmentId;
    }
    public IEstablishInformation.StatusLabel PutEstablishInformation(Guid equipmentId, Guid establishId)
    {
        EquipmentEstablishes.AddOrUpdate(equipmentId, new Dictionary<IProcessEstablish.ProcessType, Guid>
        {
            { IProcessEstablish.ProcessType.EquipmentStatus, establishId }
        }, (key, establishes) =>
        {
            ref var oldEstablishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentStatus);
            if (Unsafe.IsNullRef(ref oldEstablishId)) establishes.Add(IProcessEstablish.ProcessType.EquipmentStatus, establishId);
            return establishes;
        });
        return EstablishInformations.AddOrUpdate(establishId, new IEstablishInformation.StatusLabel
        {
            Run = nameof(IEquipment.Status.Run),
            Idle = nameof(IEquipment.Status.Idle),
            Error = nameof(IEquipment.Status.Error),
            Setup = nameof(IEquipment.Status.Setup),
            Shutdown = nameof(IEquipment.Status.Shutdown),
            Repair = nameof(IEquipment.Status.Repair),
            Maintenance = nameof(IEquipment.Status.Maintenance),
            Hold = nameof(IEquipment.Status.Hold)
        }, (key, value) => value);
    }
    public Guid PutEstablishInformation(Guid equipmentId, Guid establishId, IEstablishInformation.StatusLabel mapper)
    {
        EquipmentEstablishes.AddOrUpdate(equipmentId, new Dictionary<IProcessEstablish.ProcessType, Guid>
        {
            { IProcessEstablish.ProcessType.EquipmentStatus, establishId }
        }, (key, establishes) =>
        {
            ref var oldEstablishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentStatus);
            if (Unsafe.IsNullRef(ref oldEstablishId)) establishes.Add(IProcessEstablish.ProcessType.EquipmentStatus, establishId);
            return establishes;
        });
        EstablishInformations.AddOrUpdate(establishId, mapper, (key, values) => mapper);
        return establishId;
    }
    public (Guid establishId, Guid processId) PutEstablishProduction(Guid equipmentId, Guid establishId, Guid processId, string dispatchNo, string batchNo)
    {
        EquipmentEstablishes.AddOrUpdate(equipmentId, new Dictionary<IProcessEstablish.ProcessType, Guid>
        {
            { IProcessEstablish.ProcessType.EquipmentOutput, establishId }
        }, (key, establishes) =>
        {
            ref var oldEstablishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentOutput);
            if (Unsafe.IsNullRef(ref oldEstablishId)) establishes.Add(IProcessEstablish.ProcessType.EquipmentOutput, establishId);
            return establishes;
        });
        var order = (processId, dispatchNo, batchNo);
        var orders = EstablishProductions.AddOrUpdate(establishId, new List<(Guid processId, string dispatchNo, string batchNo)>
        {
            order
        }, (key, processes) =>
        {
            if (!processes.Any(item => item.processId == processId)) processes.Add(order);
            return processes;
        });
        return (establishId, processId);
    }
    public void PutEstablishParameter(Guid equipmentId, Guid establishId, Guid processId, string dataNo)
    {
        EquipmentEstablishes.AddOrUpdate(equipmentId, new Dictionary<IProcessEstablish.ProcessType, Guid>
        {
            { IProcessEstablish.ProcessType.EquipmentParameter, establishId }
        }, (key, establishes) =>
        {
            ref var oldEstablishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentParameter);
            if (!Unsafe.IsNullRef(ref oldEstablishId)) oldEstablishId = establishId;
            else establishes.Add(IProcessEstablish.ProcessType.EquipmentParameter, establishId);
            return establishes;
        });
        var data = (processId, dataNo);
        var datas = EstablishParameters.AddOrUpdate(establishId, new List<(Guid, string)>
        {
            data
        }, (key, processes) =>
        {
            if (!processes.Any(item => item.dataNo == dataNo)) processes.Add(data);
            return processes;
        });
    }
    public void CacheData(Guid equipmentId, Guid processId, IEquipment.Status status, DateTime eventTime)
    {
        ProcessInformations.AddOrUpdate(processId, (status, eventTime), (key, value) => (status, eventTime));
    }
    public IEquipment.Status CacheData(Guid equipmentId, string status, DateTime eventTime)
    {
        var establishId = Guid.NewGuid();
        EquipmentEstablishes.AddOrUpdate(equipmentId, new Dictionary<IProcessEstablish.ProcessType, Guid>
        {
            { IProcessEstablish.ProcessType.EquipmentStatus, establishId }
        }, (key, establishes) =>
        {
            ref var oldEstablishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentStatus);
            if (!Unsafe.IsNullRef(ref oldEstablishId)) establishId = oldEstablishId;
            else establishes.Add(IProcessEstablish.ProcessType.EquipmentStatus, establishId);
            return establishes;
        });
        var mapper = EstablishInformations.AddOrUpdate(establishId, new IEstablishInformation.StatusLabel
        {
            Run = nameof(IEquipment.Status.Run),
            Idle = nameof(IEquipment.Status.Idle),
            Error = nameof(IEquipment.Status.Error),
            Setup = nameof(IEquipment.Status.Setup),
            Shutdown = nameof(IEquipment.Status.Shutdown),
            Repair = nameof(IEquipment.Status.Repair),
            Maintenance = nameof(IEquipment.Status.Maintenance),
            Hold = nameof(IEquipment.Status.Hold)
        }, (key, value) => value);
        var equipmentStatus = AsStatus(status, mapper);
        ProcessInformations.AddOrUpdate(establishId, (equipmentStatus, eventTime), (key, value) => (equipmentStatus, eventTime));
        return equipmentStatus;
    }
    public IEquipment.Status CacheData(Guid equipmentId, Guid establishId, string status, DateTime eventTime)
    {
        var equipmentStatus = AsStatus(status, PutEstablishInformation(equipmentId, establishId));
        ProcessInformations.AddOrUpdate(establishId, (equipmentStatus, eventTime), (key, value) => (equipmentStatus, eventTime));
        return equipmentStatus;
    }
    public void CacheData(Guid equipmentId, Guid establishId, Guid processId, string dispatchNo, string batchNo, int output, DateTime eventTime)
    {
        PutEstablishProduction(equipmentId, establishId, processId, dispatchNo, batchNo);
        ProcessProductions.AddOrUpdate(processId, (output, eventTime), (key, value) => (output, eventTime));
    }
    public void CacheData(Guid equipmentId, Guid establishId, Guid processId, string dataNo, float dataValue, DateTime eventTime)
    {
        PutEstablishParameter(equipmentId, establishId, processId, dataNo);
        ProcessParameters.AddOrUpdate(processId, (dataValue, eventTime), (key, value) => (dataValue, eventTime));
    }
    public void RemoveFactory(Guid factoryId)
    {
        foreach (var factory in Factories)
        {
            if (factory.Value == factoryId)
            {
                if (Factories.Remove(factory.Key, out var value))
                {
                    foreach (var group in Groups)
                    {
                        if (value == factoryId) RemoveGroup(group.Value.groupId);
                    }
                    return;
                }
            }
        }
    }
    public void RemoveGroup(Guid groupId)
    {
        foreach (var group in Groups)
        {
            if (group.Value.groupId == groupId)
            {
                if (Groups.Remove(group.Key, out var value))
                {
                    foreach (var equipment in Equipments)
                    {
                        if (value.groupId == groupId) RemoveEquipment(equipment.Value.equipmentId);
                    }
                    return;
                }
            }
        }
    }
    public void RemoveNetwork(Guid networkId)
    {
        foreach (var network in Networks)
        {
            if (network.Value == networkId)
            {
                if (Networks.Remove(network.Key, out var value))
                {
                    foreach (var equipment in Equipments)
                    {
                        if (value == networkId) RemoveEquipment(equipment.Value.equipmentId);
                    }
                    return;
                }
            }
        }
    }
    public void RemoveEquipment(Guid equipmentId)
    {
        foreach (var equipment in Equipments)
        {
            if (equipment.Value.equipmentId == equipmentId)
            {
                if (Networks.Remove(equipment.Key, out var value))
                {
                    RemoveInformation(value);
                    RemoveProduction(value);
                    RemoveParameter(value);
                    return;
                }
            }
        }
    }
    public void RemoveInformation(Guid equipmentId) => ProcessInformations.Remove(equipmentId, out _);
    public void RemoveProduction(Guid equipmentId) => ProcessProductions.Remove(equipmentId, out _);
    public void RemoveParameter(Guid equipmentId) => ProcessParameters.Remove(equipmentId, out _);
    public void RemoveFactory(string factoryNo)
    {
        if (Factories.Remove(factoryNo, out var value))
        {
            foreach (var group in Groups)
            {
                if (group.Value.factoryId == value) RemoveGroup(group.Key);
            }
        }
    }
    public void RemoveGroup(string groupNo)
    {
        if (Groups.Remove(groupNo, out var value))
        {
            foreach (var equipment in Equipments)
            {
                if (equipment.Value.groupId == value.groupId) RemoveEquipment(equipment.Key);
            }
        }
    }
    public void RemoveNetwork(string networkNo)
    {
        if (Networks.Remove(networkNo, out var value))
        {
            foreach (var equipment in Equipments)
            {
                if (equipment.Value.networkId == value) RemoveEquipment(equipment.Key);
            }
        }
    }
    public void RemoveEquipment(string equipmentNo)
    {
        if (Equipments.Remove(equipmentNo, out var value))
        {
            RemoveInformation(value.equipmentId);
            RemoveProduction(value.equipmentId);
            RemoveParameter(value.equipmentId);
        }
    }
    public (Guid networkId, Guid groupId, Guid equipmentId) GetEquipment(string equipmentNo)
    {
        if (Equipments.TryGetValue(equipmentNo, out var value)) return value;
        return (default, default, default);
    }
    public (Guid establishInformationId, IEstablishInformation.StatusLabel mapper) GetEquipmentStatus(Guid equipmentId)
    {
        if (EquipmentEstablishes.TryGetValue(equipmentId, out var establishes))
        {
            ref var establishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentStatus);
            if (!Unsafe.IsNullRef(ref establishId))
            {
                if (EstablishInformations.TryGetValue(establishId, out var value))
                {
                    return (establishId, value);
                }
            }
        }
        return (default, default);
    }
    public (Guid establishProductionId, List<(Guid processId, string dispatchNo, string batchNo)> orders) GetEquipmentOrder(Guid equipmentId)
    {
        List<(Guid processId, string dispatchNo, string batchNo)> results = new();
        if (EquipmentEstablishes.TryGetValue(equipmentId, out var establishes))
        {
            ref var establishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentOutput);
            if (!Unsafe.IsNullRef(ref establishId))
            {
                if (EstablishProductions.TryGetValue(establishId, out var orders))
                {
                    foreach (var (processId, dispatchNo, batchNo) in orders) results.Add(new()
                    {
                        processId = processId,
                        dispatchNo = dispatchNo,
                        batchNo = batchNo
                    });
                    return (establishId, results);
                }
            }
        }
        return (default, results);
    }
    public (Guid establishParameterId, List<(Guid processId, string dataNo)> datas) GetDashboardData(Guid equipmentId)
    {
        List<(Guid processId, string dataNo)> results = new();
        if (EquipmentEstablishes.TryGetValue(equipmentId, out var establishes))
        {
            ref var establishId = ref CollectionsMarshal.GetValueRefOrNullRef(establishes, IProcessEstablish.ProcessType.EquipmentParameter);
            if (!Unsafe.IsNullRef(ref establishId))
            {
                if (EstablishParameters.TryGetValue(establishId, out var datas))
                {
                    foreach (var (processId, dataNo) in datas) results.Add(new()
                    {
                        processId = processId,
                        dataNo = dataNo
                    });
                    return (establishId, results);
                }
            }
        }
        return (default, results);
    }
    public (IEquipment.Status status, DateTime eventTime) GetInformation(Guid processId)
    {
        if (ProcessInformations.TryGetValue(processId, out var value)) return (value.status, value.eventTime.ToLocalTime());
        return (IEquipment.Status.Unused, default);
    }
    public (int output, DateTime eventTime) GetProduction(Guid processId)
    {
        if (ProcessProductions.TryGetValue(processId, out var value)) return (value.output, value.eventTime.ToLocalTime());
        return (default, default);
    }
    public (float dataValue, DateTime eventTime) GetParameter(Guid processId)
    {
        if (ProcessParameters.TryGetValue(processId, out var value)) return (value.dataValue, value.eventTime.ToLocalTime());
        return (default, default);
    }
    public IDictionary<string, Guid> ListFactory() => Factories.ToImmutableDictionary();
    public IDictionary<string, (Guid factoryId, Guid groupId)> ListGroup() => Groups.ToImmutableDictionary();
    public IDictionary<string, Guid> ListNetwork() => Networks.ToImmutableDictionary();
    public IDictionary<string, (Guid networkId, Guid groupId, Guid equipmentId)> ListEquipment() => Equipments.ToImmutableDictionary();
    public IDictionary<Guid, (IEquipment.Status status, DateTime eventTime)> ListInformation() => ProcessInformations.ToImmutableDictionary();
    public bool IsFactory { get; set; }
    public bool IsGroup { get; set; }
    public bool IsNetwork { get; set; }
    public bool IsEquipment { get; set; }
    public bool IsEstablish { get; set; }
    ConcurrentDictionary<string, Guid> Factories { get; init; } = new();
    ConcurrentDictionary<string, (Guid factoryId, Guid groupId)> Groups { get; init; } = new();
    ConcurrentDictionary<string, Guid> Networks { get; init; } = new();
    ConcurrentDictionary<string, (Guid networkId, Guid groupId, Guid equipmentId)> Equipments { get; init; } = new();
    ConcurrentDictionary<Guid, Dictionary<IProcessEstablish.ProcessType, Guid>> EquipmentEstablishes { get; init; } = new();
    ConcurrentDictionary<Guid, IEstablishInformation.StatusLabel> EstablishInformations { get; init; } = new();
    ConcurrentDictionary<Guid, (IEquipment.Status status, DateTime eventTime)> ProcessInformations { get; init; } = new();
    ConcurrentDictionary<Guid, List<(Guid processId, string dispatchNo, string batchNo)>> EstablishProductions { get; init; } = new();
    ConcurrentDictionary<Guid, (int output, DateTime eventTime)> ProcessProductions { get; init; } = new();
    ConcurrentDictionary<Guid, List<(Guid processId, string dataNo)>> EstablishParameters { get; init; } = new();
    ConcurrentDictionary<Guid, (float dataValue, DateTime eventTime)> ProcessParameters { get; init; } = new();
}