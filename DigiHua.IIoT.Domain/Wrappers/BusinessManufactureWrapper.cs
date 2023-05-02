using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Domain.Wrappers;

[Dependency(ServiceLifetime.Singleton)]
file sealed class BusinessManufactureWrapper : IBusinessManufactureWrapper
{
    public IFactory Factory => new Factory(NpgsqlUtility);
    public IFactoryGroup FactoryGroup => new FactoryGroup(NpgsqlUtility);
    public INetwork Network => new Network(NpgsqlUtility);
    public INetworkMqtt NetworkMqtt => new NetworkMqtt(NpgsqlUtility);
    public INetworkOpcUa NetworkOpcUa => new NetworkOpcUa(NpgsqlUtility);
    public IEquipment Equipment => new Equipment(NpgsqlUtility);
    public IEquipmentAlarm EquipmentAlarm => new EquipmentAlarm(NpgsqlUtility);
    public IOpcUaProcess OpcUaProcess => new OpcUaProcess(NpgsqlUtility);
    public IProduceState ProduceState => new ProduceState(NpgsqlUtility);
    public IMission Mission => new Mission(NpgsqlUtility);
    public IMissionPush MissionPush => new MissionPush(NpgsqlUtility);
    public IPushHistory PushHistory => new PushHistory(NpgsqlUtility);
    public IProcessEstablish ProcessEstablish => new ProcessEstablish(NpgsqlUtility);
    public IEstablishInformation EstablishInformation => new EstablishInformation(NpgsqlUtility);
    public IInformationStack InformationStack => new InformationStack(NpgsqlUtility);
    public IEstablishProduction EstablishProduction => new EstablishProduction(NpgsqlUtility);
    public IProductionStack ProductionStack => new ProductionStack(NpgsqlUtility);
    public IEstablishParameter EstablishParameter => new EstablishParameter(NpgsqlUtility);
    public IParameterStack ParameterStack => new ParameterStack(NpgsqlUtility);
    public IParameterFormula ParameterFormula => new ParameterFormula(NpgsqlUtility);
    public required INpgsqlUtility NpgsqlUtility { get; init; }
}