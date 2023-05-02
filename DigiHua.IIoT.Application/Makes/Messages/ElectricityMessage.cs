namespace IIoT.Application.Makes.Messages;
internal sealed class ElectricityMessage : IEntranceTrigger<(RollingInterval interval, DateTimeOffset start, DateTimeOffset end), JObject>
{
    readonly IStringLocalizer<Fielder> _fielder;
    public ElectricityMessage(IStringLocalizer<Fielder> fielder)
    {
        _fielder = fielder;
    }
    public async ValueTask<JObject> PushAsync((RollingInterval interval, DateTimeOffset start, DateTimeOffset end) entity)
    {
        List<(DateTimeOffset startTime, DateTimeOffset endTime)> dayIntervals = new();
        List<(string meterNo, double value, DateTimeOffset startTime, DateTimeOffset endTime)> hourBoxes = new();
        AthenaMedium.Result result = new()
        {
            SqlCode = string.Empty,
            Code = ((int)default).ToString(),
            Description = string.Empty
        };
        try
        {
            using (CultureHelper.Use(Language))
            {
                var FF = TimeOnly.FromDateTime(DateTime.UtcNow);
                var LL = FF.ToString("HH:mm:ss");
                //var electricitiesAsync = _originalEnvironment.Electricity.QueryAsync(entity.start, entity.end);
                if (entity.start >= entity.end) throw new Exception(_fielder["wrong.starttime.endtime.comparison"]);
                List<(DateTimeOffset startTime, DateTimeOffset endTime)> hourIntervals = new();
                {
                    await Task.WhenAll(new Task[]
                    {
                        Task.Run(() =>
                        {
                            var startTimeValidator = entity.start;
                            while (startTimeValidator < entity.end)
                            {
                                var endTimeValidator = startTimeValidator.AddDays(1);
                                if (endTimeValidator > entity.end) endTimeValidator = entity.end;
                                {
                                     dayIntervals.Add(new()
                                     {
                                         startTime = startTimeValidator,
                                         endTime = endTimeValidator
                                     });
                                }
                                startTimeValidator = endTimeValidator;
                            }
                        }),
                        Task.Run(() =>
                        {
                            var startTimeValidator = entity.start;
                            while (startTimeValidator < entity.end)
                            {
                                var endTimeValidator = startTimeValidator.AddHours(1);
                                if (endTimeValidator > entity.end) endTimeValidator = entity.end;
                                {
                                    hourIntervals.Add(new()
                                    {
                                        startTime = startTimeValidator,
                                        endTime = endTimeValidator
                                    });
                                }
                                startTimeValidator = endTimeValidator;
                            }
                        })
                    });
                }
                //var electricities = await electricitiesAsync;
                foreach (var (startTime, endTime) in hourIntervals)
                {
                    //foreach (var item in electricities.Where(item => item.EventTime >= startTime && item.EventTime < endTime)
                    //.GroupBy(item => item.MeterNo).ToDictionary(item => item.Key, item => item.Average(item => item.TotalPower)))
                    //{
                    //    hourBoxes.Add((item.Key, Math.Ceiling(item.Value), startTime, endTime));
                    //}
                }
            }
        }
        catch (NpgsqlException e)
        {
            result.Code = IManufactureClient.Label.Failure;
            result.SqlCode = e.Message;
        }
        catch (Exception e)
        {
            result.Code = IManufactureClient.Label.Failure;
            result.Description = e.Message;
        }
        JArray changeObjects = new();
        switch (entity.interval)
        {
            case RollingInterval.Day:
                List<Parameter> dayParameters = new();
                foreach (var (dayStartTime, dayEndTime) in dayIntervals)
                {
                    List<(string meterNo, double value)> items = new();
                    foreach (var (meterNo, value, hourStartTime, hourEndTime) in hourBoxes)
                    {
                        if (hourStartTime >= dayStartTime && hourEndTime <= dayEndTime) items.Add((meterNo, value));
                    }
                    foreach (var item in items.GroupBy(item => item.meterNo).ToDictionary(item => item.Key, item => item.Sum(item => item.value)))
                    {
                        dayParameters.Add(new()
                        {
                            MeterNo = item.Key,
                            StartTime = dayStartTime,
                            EndTime = dayEndTime,
                            ParameterNo = AthenaMedium.DayType.Electricity.GetDesc(),
                            ParameterValue = item.Value.ToString()
                        });
                    }
                }
                changeObjects = JArray.FromObject(dayParameters);
                break;

            case RollingInterval.Hour:
                changeObjects = JArray.FromObject(hourBoxes.Select(item => new Parameter
                {
                    MeterNo = item.meterNo,
                    StartTime = item.startTime,
                    EndTime = item.endTime,
                    ParameterNo = AthenaMedium.HourType.Electricity.GetDesc(),
                    ParameterValue = item.value.ToString()
                }));
                break;
        }
        return new JObject()
        {
            {
                IManufactureClient.Label.StandardData, new JObject()
                {
                    {
                        AthenaMedium.Execution, JObject.FromObject(result)
                    },
                    {
                        AthenaMedium.Parameter, new JObject()
                        {
                            {
                                AthenaMedium.ChangeObjects, changeObjects
                            }
                        }
                    }
                }
            }
        };
    }
    public readonly record struct Parameter
    {
        [JsonProperty("meter_no")] public string MeterNo { get; init; }
        [JsonProperty("parameter_no")] public string ParameterNo { get; init; }
        [JsonProperty("parameter_value")] public string ParameterValue { get; init; }
        [JsonProperty("start_time"), JsonConverter(typeof(DateTimeConvert))] public DateTimeOffset StartTime { get; init; }
        [JsonProperty("end_time"), JsonConverter(typeof(DateTimeConvert))] public DateTimeOffset EndTime { get; init; }
        internal class DateTimeConvert : DateTimeConverterBase
        {
            static readonly IsoDateTimeConverter _converter = new()
            {
                DateTimeFormat = Converter.EaiSeconds
            };
            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                return _converter.ReadJson(reader, objectType, existingValue, serializer) ?? new object();
            }
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                _converter.WriteJson(writer, value, serializer);
            }
        }
    }
}