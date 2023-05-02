namespace IIoT.Domain.Shared.Wrappers;
public interface IBusinessManufactureWrapper
{
    IFactory Factory { get; }
    IFactoryGroup FactoryGroup { get; }
    INetwork Network { get; }
    INetworkMqtt NetworkMqtt { get; }
    INetworkOpcUa NetworkOpcUa { get; }
    IEquipment Equipment { get; }
    IEquipmentAlarm EquipmentAlarm { get; }
    IOpcUaProcess OpcUaProcess { get; }
    IProduceState ProduceState { get; }
    IMission Mission { get; }
    IMissionPush MissionPush { get; }
    IPushHistory PushHistory { get; }
    IProcessEstablish ProcessEstablish { get; }
    IEstablishInformation EstablishInformation { get; }
    IInformationStack InformationStack { get; }
    IEstablishProduction EstablishProduction { get; }
    IProductionStack ProductionStack { get; }
    IEstablishParameter EstablishParameter { get; }
    IParameterStack ParameterStack { get; }
    IParameterFormula ParameterFormula { get; }
}