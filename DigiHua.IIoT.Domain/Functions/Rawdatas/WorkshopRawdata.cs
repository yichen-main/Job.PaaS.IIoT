using static IIoT.Domain.Shared.Functions.Rawdatas.IWorkshopRawdata;

namespace IIoT.Domain.Functions.Rawdatas;
internal sealed class WorkshopRawdata : IWorkshopRawdata
{
    public async ValueTask BuildAsync()
    {
        using var influx = new InfluxDBClient(Morse.Flowmeter, Morse.Username, Morse.Password);
        var bucket = await influx.GetBucketsApi().FindBucketByNameAsync(HeadName);
        if (bucket is null)
        {
            BucketRetentionRules rule = new(BucketRetentionRules.TypeEnum.Expire, 100 * Day);
            var organizations = await influx.GetOrganizationsApi().FindOrganizationsAsync(org: Floor.CompanyName);
            await influx.GetBucketsApi().CreateBucketAsync(HeadName, rule, organizations[default].Id);
        }
    }
    public async ValueTask InsertAsync(Title title, IEquipment.Status ststus)
    {
        if (Morse.Meter) await WriteAsync(new Information
        {
            Status = (byte)ststus,
            Timestamp = DateTime.UtcNow,
            WorkshopNo = title.EquipmentNo,
            IdentifyNo = (ushort)IProcessEstablish.ProcessType.EquipmentStatus
        });
    }
    public async ValueTask InsertAsync(Title title, IEnumerable<Parameter.Meta> texts)
    {
        if (Morse.Meter && texts.Any()) await WriteAsync(texts.Select(item => new Parameter
        {
            DataNo = item.DataNo,
            DataValue = item.DataValue,
            Timestamp = DateTime.UtcNow,
            WorkshopNo = title.EquipmentNo,
            IdentifyNo = (ushort)IProcessEstablish.ProcessType.EquipmentParameter
        }).ToArray());
    }
    public async ValueTask InsertAsync(Title title, IEnumerable<Production.Meta> texts)
    {
        if (Morse.Meter && texts.Any()) await WriteAsync(texts.Select(item => new Production
        {
            WorkshopNo = title.EquipmentNo,
            DispatchNo = item.DispatchNo,
            BatchNo = item.BatchNo,
            Output = item.Output,
            Timestamp = DateTime.UtcNow,
            IdentifyNo = (ushort)IProcessEstablish.ProcessType.EquipmentOutput
        }).ToArray());
    }
    public async ValueTask WriteAsync<TEntity>(TEntity entity) where TEntity : Timeseries
    {
        using var influx = new InfluxDBClient(Morse.Flowmeter, Morse.Username, Morse.Password);
        await influx.GetWriteApiAsync().WriteMeasurementAsync(entity, WritePrecision.Ns, HeadName, Floor.CompanyName);
    }
    public async ValueTask WriteAsync<TEntity>(TEntity[] entities) where TEntity : Timeseries
    {
        using var influx = new InfluxDBClient(Morse.Flowmeter, Morse.Username, Morse.Password);
        await influx.GetWriteApiAsync().WriteMeasurementsAsync(entities, WritePrecision.Ns, HeadName, Floor.CompanyName);
    }
    public IDictionary<string, TEntity[]> Read<TEntity>(IProcessEstablish.ProcessType type, DateTimeOffset start, DateTimeOffset end) where TEntity : Timeseries
    {
        using var influx = new InfluxDBClient(Morse.Flowmeter, Morse.Username, Morse.Password);
        return InfluxDBQueryable<TEntity>.Queryable(HeadName, Floor.CompanyName, influx.GetQueryApiSync()).Where(item =>
        item.WorkshopNo != default && item.IdentifyNo == (ushort)type && item.Timestamp > start.UtcDateTime && item.Timestamp < end.UtcDateTime)
        .OrderByDescending(item => item.Timestamp).ToArray().GroupBy(item => item.WorkshopNo)
        .ToDictionary(item => item.Key, item => item.ToArray()).ToImmutableDictionary();
    }
}