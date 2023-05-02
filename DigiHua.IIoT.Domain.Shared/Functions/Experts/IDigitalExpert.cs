namespace IIoT.Domain.Shared.Functions.Experts;
public interface IDigitalExpert
{
    Subscription? AddSubscription(in IWorkshopRawdata.Title title);
    void RemoveSubscription(in Subscription subscription, in MonitoredItem[] monitors);
    void RemoveSubscriptions(in ConcurrentDictionary<string, (Subscription subscription, MonitoredItem[] monitors)> nodes);
    MonitoredItem[] AddMonitoredItems(in Subscription subscription, in (IWorkshopRawdata.EaiType type, string key, string path)[] parameters);
    ReferenceDescriptionCollection BrowserDetailNode(NodeId nodeId);
    ReferenceDescriptionCollection BrowserNode(NodeId nodeId);
    IEnumerable<DataValue> ReadNodes(NodeId[] nodeIds);
    Task<IEnumerable<DataValue>> ReadNodesAsync(NodeId[] nodeIds);
    DataValue[] ReadNodeAttributes(IEnumerable<NodeId> nodeIds);
    T ReadNode<T>(string nodeId);
    DataValue ReadNode(NodeId nodeId);
    Task<T> ReadNodeAsync<T>(string nodeId);
    IEnumerable<T> ReadNodes<T>(string[] tags);
    Task<IEnumerable<T>> ReadNodesAsync<T>(string[] tags);
    bool IsWriteableNode(NodeId nodeId);
    bool WriteNode<T>(string tag, T value);
    Task<bool> WriteNodeAsync<T>(string nodeId, T value);
    bool WriteNodes(string[] nodeIds, object[] values);
    string ClientId { get; set; }
    string Endpoint { get; set; }
    string Username { get; set; }
    string Password { get; set; }
    bool Connected { get; set; }
    ConcurrentDictionary<string, (Subscription subscription, MonitoredItem[] monitors)> Nodes { get; set; }
}