namespace IIoT.Domain.Shared.Functions.Promoters;
public interface IEaistagePromoter
{
    void OnLatest(in HostEventArgs @event);
    void OnLatest(in MessageEventArgs @event);
    sealed class HostEventArgs : EventArgs
    {
        public required IMissionPush.EnvironmentType Environment { get; init; }
        public required string Url { get; init; }
        public required string Message { get; init; }
    }
    sealed class MessageEventArgs : EventArgs
    {
        public required IWorkshopRawdata.EaiType EaiType { get; init; }
        public required long ConsumeMS { get; init; }
        public required string Eendpoint { get; init; }
        public required string Request { get; init; }
        public required string Response { get; init; }
    }
}