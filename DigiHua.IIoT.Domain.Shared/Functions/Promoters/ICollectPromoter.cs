namespace IIoT.Domain.Shared.Functions.Promoters;
public interface ICollectPromoter
{
    void OnLatest(in BackgroundEventArgs @event);
    void OnLatest(in CollectiveEventArgs @event);
    void OnLatest(in NativeQueueEventArgs @event);
    sealed class CollectiveEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;
        public string Burst { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Trace { get; set; } = string.Empty;
    }
    sealed class BackgroundEventArgs : EventArgs
    {
        public long ConsumeTime { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Trace { get; set; } = string.Empty;
    }
    sealed class NativeQueueEventArgs : EventArgs
    {
        public INetworkMqtt.Customer Type { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}