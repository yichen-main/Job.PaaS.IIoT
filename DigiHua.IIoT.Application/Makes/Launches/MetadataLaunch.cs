namespace IIoT.Application.Makes.Launches;
internal sealed class MetadataLaunch : IMetadataLaunch
{
    readonly IRegisterTrigger _registerTrigger;
    readonly IWorkshopRawdata _workshopRawdata;
    readonly IBusinessManufactureWrapper _businessManufacture;
    public MetadataLaunch(
        IRegisterTrigger registerTrigger,
        IWorkshopRawdata workshopRawdata,
        IBusinessManufactureWrapper businessManufacture)
    {
        _registerTrigger = registerTrigger;
        _workshopRawdata = workshopRawdata;
        _businessManufacture = businessManufacture;
    }
    public async Task PushAsync(IWorkshopRawdata.Title title, IWorkshopRawdata.Information.Meta content)
    {
        var networkId = _registerTrigger.PutNetwork(Guid.NewGuid(), title.SourceNo);
        var factoryId = _registerTrigger.PutFactory(Guid.NewGuid(), title.FactoryNo);
        var groupId = _registerTrigger.PutGroup(factoryId, Guid.NewGuid(), title.GroupNo);
        var equipmentId = _registerTrigger.PutEquipment(networkId, groupId, Guid.NewGuid(), title.EquipmentNo);
        var equipmentStatus = _registerTrigger.CacheData(equipmentId, content.Status, DateTime.UtcNow);
        if (equipmentStatus is not IEquipment.Status.Unused) await _workshopRawdata.InsertAsync(title, equipmentStatus);
    }
    public async Task PushAsync(IWorkshopRawdata.Title title, IEnumerable<IWorkshopRawdata.Production.Meta> contents)
    {
        var networkId = _registerTrigger.PutNetwork(Guid.NewGuid(), title.SourceNo);
        var factoryId = _registerTrigger.PutFactory(Guid.NewGuid(), title.FactoryNo);
        var groupId = _registerTrigger.PutGroup(factoryId, Guid.NewGuid(), title.GroupNo);
        var equipmentId = _registerTrigger.PutEquipment(networkId, groupId, Guid.NewGuid(), title.EquipmentNo);
        var (establishProductionId, orders) = _registerTrigger.GetEquipmentOrder(equipmentId);
        if (establishProductionId != default)
        {
            foreach (var content in contents)
            {
                var (processId, dispatchNo, batchNo) = orders.Find(item => item.dispatchNo == content.DispatchNo && item.batchNo == content.BatchNo);
                if (processId != default)
                {
                    _registerTrigger.CacheData(equipmentId, establishProductionId, processId, content.DispatchNo, content.BatchNo, content.Output, DateTime.UtcNow);
                }
                else _registerTrigger.CacheData(equipmentId, establishProductionId, Guid.NewGuid(), content.DispatchNo, content.BatchNo, content.Output, DateTime.UtcNow);
            }
        }
        else
        {
            establishProductionId = Guid.NewGuid();
            foreach (var content in contents)
            {
                _registerTrigger.CacheData(equipmentId, establishProductionId, Guid.NewGuid(), content.DispatchNo, content.BatchNo, content.Output, DateTime.UtcNow);
            }
        }
        await _workshopRawdata.InsertAsync(title, contents);
    }
    public async Task PushAsync(IWorkshopRawdata.Title title, IEnumerable<IWorkshopRawdata.Parameter.Universal> contents)
    {
        var networkId = _registerTrigger.PutNetwork(Guid.NewGuid(), title.SourceNo);
        var factoryId = _registerTrigger.PutFactory(Guid.NewGuid(), title.FactoryNo);
        var groupId = _registerTrigger.PutGroup(factoryId, Guid.NewGuid(), title.GroupNo);
        var equipmentId = _registerTrigger.PutEquipment(networkId, groupId, Guid.NewGuid(), title.EquipmentNo);
        List<IWorkshopRawdata.Parameter.Meta> dashboards = new();
        List<IParameterFormula.Entity> formulas = new();
        var (establishParameterId, datas) = _registerTrigger.GetDashboardData(equipmentId);
        if (establishParameterId != default)
        {
            foreach (var content in contents)
            {
                var metadata = Convert.ToString(content.DataValue) ?? string.Empty;
                if (float.TryParse(metadata, out var dataValue))
                {
                    var (processId, dataNo) = datas.Find(item => item.dataNo == content.DataNo);
                    if (processId != default)
                    {
                        _registerTrigger.CacheData(equipmentId, establishParameterId, processId, content.DataNo, dataValue, DateTime.UtcNow);
                    }
                    else _registerTrigger.CacheData(equipmentId, establishParameterId, Guid.NewGuid(), content.DataNo, dataValue, DateTime.UtcNow);
                    dashboards.Add(new()
                    {
                        DataNo = content.DataNo,
                        DataValue = dataValue
                    });
                }
                else formulas.Add(new()
                {
                    Id = Guid.NewGuid(),
                    EquipmentNo = title.EquipmentNo,
                    DataNo = content.DataNo,
                    DataValue = metadata,
                    CreateTime = DateTime.UtcNow
                });
            }
        }
        else
        {
            establishParameterId = Guid.NewGuid();
            foreach (var content in contents)
            {
                var metadata = Convert.ToString(content.DataValue) ?? string.Empty;
                if (float.TryParse(metadata, out var dataValue))
                {
                    _registerTrigger.CacheData(equipmentId, establishParameterId, Guid.NewGuid(), content.DataNo, dataValue, DateTime.UtcNow);
                }
                else formulas.Add(new()
                {
                    Id = Guid.NewGuid(),
                    EquipmentNo = title.EquipmentNo,
                    DataNo = content.DataNo,
                    DataValue = metadata,
                    CreateTime = DateTime.UtcNow
                });
            }
        }
        if (dashboards.Any()) await _workshopRawdata.InsertAsync(title, dashboards);
        if (formulas.Any()) await _businessManufacture.ParameterFormula.AddAsync(formulas);
    }
}