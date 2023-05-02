namespace IIoT.Application.Contracts.Architects.Events;
public interface IClearerEvent : ITacticExpert
{
    Task UserAsync(IEnumerable<Guid> ids);
    Task FactoryAsync(IEnumerable<Guid> ids);
    Task FactoryGroupAsync(IEnumerable<Guid> ids);
    Task EquipmentAsync(IEnumerable<Guid> ids);
    Task EquipmentAlarmAsync(IEnumerable<Guid> ids);
    Task NetworkAsync(IEnumerable<INetwork.Entity> entities);
    Task MissionAsync(IEnumerable<IMission.Entity> entities);
    Task ProcessEstablishAsync(IEnumerable<IProcessEstablish.Entity> entities);
    Task EstablishInformationAsync(IEstablishInformation.Entity entity);
    Task EstablishProductionAsync(IEstablishProduction.Entity entity);
    Task EstablishParameterAsync(IEstablishParameter.Entity entity);
}