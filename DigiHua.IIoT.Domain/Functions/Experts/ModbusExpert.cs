namespace IIoT.Domain.Functions.Experts;
public abstract class ModbusExpert
{
    public async Task<IDictionary<string, HashSet<(int key, ushort value)>>> ReadRegisterAsync(Parameter parameter)
    {
        ICollectPromoter.CollectiveEventArgs collectiveEvent = new()
        {
            Title = nameof(ModbusExpert)
        };
        try
        {
            using TcpClient client = new(parameter.Ip, parameter.Port);
            var master = new ModbusFactory().CreateMaster(client);
            master.Transport.ReadTimeout = 10000;
            master.Transport.Retries = 2000;
            List<(Parameter.Device device, Task<ushort[]> datas)> datalinks = new();
            Array.ForEach(parameter.Devices, device =>
            {
                switch (device.FunctionCode)
                {
                    case Code.InputRegisters:
                        datalinks.Add((device, master.ReadInputRegistersAsync(
                        parameter.SlaveNumber, device.StartAddress, (ushort)device.Points.Length)));
                        break;

                    case Code.HoldingRegisters:
                        datalinks.Add((device, master.ReadHoldingRegistersAsync(
                        parameter.SlaveNumber, device.StartAddress, (ushort)device.Points.Length)));
                        break;
                }
            });
            foreach (var datalink in datalinks)
            {
                var datas = await datalink.datas;
                HashSet<(int key, ushort value)> contents = new();
                Array.ForEach(datalink.device.Points, point =>
                {
                    contents.Add((point.Key, datas[Array.IndexOf(datalink.device.Points, point)]));
                });
                if (Nodes.TryGetValue(datalink.device.Id, out var oldValue))
                {
                    oldValue.UnionWith(contents);
                }
                else
                {
                    Nodes.Add(datalink.device.Id, contents);
                }
            }
            master.Dispose();
            client.Close();
        }
        catch (SlaveException e)
        {
            collectiveEvent.Burst = e.Message;
        }
        catch (Exception e)
        {
            collectiveEvent.Detail = e.Message;
        }
        return Nodes;
    }
    public enum Code
    {
        InputRegisters = 03,
        HoldingRegisters = 04
    }
    public struct Parameter
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public byte SlaveNumber { get; set; }
        public Device[] Devices { get; set; }
        public struct Device
        {
            public string Id { get; set; }
            public ushort StartAddress { get; set; }
            public Code FunctionCode { get; set; }
            public Point[] Points { get; set; }
            public struct Point
            {
                public int Key { get; set; }
            }
        }
    }
    Dictionary<string, HashSet<(int key, ushort value)>> Nodes { get; } = new();
}