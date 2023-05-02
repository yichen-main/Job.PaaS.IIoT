namespace IIoT.Domain.Shared.Functions.Triggers;
public interface IRegisterTrigger
{
    bool Pass();
    bool IsEquipmentStatus(int value);
    IEquipment.Status ToEquipmentStatus(byte value);
    IEquipment.Status ToEquipmentStatus(string value);
    string AsConverter(in IEquipment.Status status);
    IEquipment.Status AsStatus(in string status, in IEstablishInformation.StatusLabel mapper);
    IEstablishInformation.StatusLabel ChangeStatus(in string text);
    Guid PutFactory(Guid factoryId, string factoryNo);
    Guid PutGroup(Guid factoryId, Guid groupId, string groupNo);
    Guid PutNetwork(Guid networkId, string networkNo);
    Guid PutEquipment(Guid networkId, Guid groupId, Guid equipmentId, string equipmentNo);
    IEstablishInformation.StatusLabel PutEstablishInformation(Guid equipmentId, Guid establishId);
    Guid PutEstablishInformation(Guid equipmentId, Guid establishId, IEstablishInformation.StatusLabel mapper);
    (Guid establishId, Guid processId) PutEstablishProduction(Guid equipmentId, Guid establishId, Guid processId, string dispatchNo, string batchNo);
    void PutEstablishParameter(Guid equipmentId, Guid establishId, Guid processId, string dataNo);
    void CacheData(Guid equipmentId, Guid processId, IEquipment.Status status, DateTime eventTime);
    IEquipment.Status CacheData(Guid equipmentId, string status, DateTime eventTime);
    IEquipment.Status CacheData(Guid equipmentId, Guid establishId, string status, DateTime eventTime);
    void CacheData(Guid equipmentId, Guid establishId, Guid processId, string dispatchNo, string batchNo, int output, DateTime eventTime);
    void CacheData(Guid equipmentId, Guid establishId, Guid processId, string dataNo, float dataValue, DateTime eventTime);
    void RemoveFactory(Guid factoryId);
    void RemoveGroup(Guid groupId);
    void RemoveNetwork(Guid networkId);
    void RemoveEquipment(Guid equipmentId);
    void RemoveInformation(Guid equipmentId);
    void RemoveProduction(Guid equipmentId);
    void RemoveParameter(Guid equipmentId);
    void RemoveFactory(string factoryNo);
    void RemoveGroup(string groupNo);
    void RemoveNetwork(string networkNo);
    void RemoveEquipment(string equipmentNo);
    (Guid networkId, Guid groupId, Guid equipmentId) GetEquipment(string equipmentNo);
    (Guid establishInformationId, IEstablishInformation.StatusLabel mapper) GetEquipmentStatus(Guid equipmentId);
    (Guid establishProductionId, List<(Guid processId, string dispatchNo, string batchNo)> orders) GetEquipmentOrder(Guid equipmentId);
    (Guid establishParameterId, List<(Guid processId, string dataNo)> datas) GetDashboardData(Guid equipmentId);
    (IEquipment.Status status, DateTime eventTime) GetInformation(Guid processId);
    (int output, DateTime eventTime) GetProduction(Guid processId);
    (float dataValue, DateTime eventTime) GetParameter(Guid processId);
    IDictionary<string, Guid> ListFactory();
    IDictionary<string, (Guid factoryId, Guid groupId)> ListGroup();
    IDictionary<string, Guid> ListNetwork();
    IDictionary<string, (Guid networkId, Guid groupId, Guid equipmentId)> ListEquipment();
    IDictionary<Guid, (IEquipment.Status status, DateTime eventTime)> ListInformation();
    public bool IsFactory { get; set; }
    public bool IsGroup { get; set; }
    public bool IsNetwork { get; set; }
    public bool IsEquipment { get; set; }
    public bool IsEstablish { get; set; }
}