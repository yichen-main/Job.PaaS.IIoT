namespace IIoT.Application.Contracts.Makes.Launches;
public interface IMetadataLaunch
{
    Task PushAsync(IWorkshopRawdata.Title title, IWorkshopRawdata.Information.Meta content);
    Task PushAsync(IWorkshopRawdata.Title title, IEnumerable<IWorkshopRawdata.Production.Meta> contents);
    Task PushAsync(IWorkshopRawdata.Title title, IEnumerable<IWorkshopRawdata.Parameter.Universal> contents);
}