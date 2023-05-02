namespace IIoT.Station.Apis.Foreigns.Athenas;

[Route($"{nameof(IIoT)}/{AthenaMedium.SteadyDesk}/{AthenaMedium.Energy}")]
[ApiController, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Application))]
public class Energies : Controller
{
    [HttpPost]
    public async ValueTask<IActionResult> ListElectricityMeterAsync([FromHeader] HeaderMain header, [FromBody] JToken body)
    {
        try
        {
            var standardData = body.SelectToken(IManufactureClient.Label.StandardData);
            if (standardData is not null)
            {
                var parameter = standardData.SelectToken(AthenaMedium.Parameter);
                if (parameter is not null)
                {
                    if (header.DigiService != string.Empty)
                    {
                        switch (header.DigiService.ToObject<IdentifyServiceMain>().Name)
                        {
                            case ApplicationMain.ValidOrganizationDataGet:
                                var organization = parameter.ToObject<OrganizationRequest>();
                                ArgumentNullException.ThrowIfNull(organization, nameof(OrganizationRequest));
                                return Ok(await MakeMessage.Organization.PushAsync(new AthenaMedium.Organization()
                                {
                                    Companies = new[]
                                    {
                                        new AthenaMedium.Organization.Company()
                                        {
                                            CompanyNo = RunnerText.Organization.CompanyID,
                                            CompanyName = RunnerText.Organization.CompanyName,
                                            Sites = new[]
                                            {
                                                new AthenaMedium.Organization.Site()
                                                {
                                                    SiteNo = RunnerText.Organization.SiteID,
                                                    SiteName = RunnerText.Organization.SiteName
                                                }
                                            }
                                        }
                                    },
                                    Regions = new[]
                                    {
                                        new AthenaMedium.Organization.Region()
                                        {
                                            RegionNo = string.Empty,
                                            RegionName = string.Empty,
                                            RegionType = string.Empty
                                        }
                                    },
                                    Sites = new[]
                                    {
                                        new AthenaMedium.Organization.Site()
                                        {
                                            SiteNo = RunnerText.Organization.SiteID,
                                            SiteName = RunnerText.Organization.SiteName
                                        }
                                    }
                                }));

                            case ApplicationMain.MeterDayEnergyConsumeDataGet:
                                var energyDay = parameter.ToObject<EnergyRequest>();
                                ArgumentNullException.ThrowIfNull(energyDay, AthenaMedium.ChangeObjects);
                                return Ok(await MakeMessage.Electricity.PushAsync(
                                (RollingInterval.Day, energyDay.StartTime.ToDateTime().Date, energyDay.EndTime.ToDateTime().Date)));

                            case ApplicationMain.MeterHourEnergyConsumeDataGet:
                                var energyHour = parameter.ToObject<EnergyRequest>();
                                ArgumentNullException.ThrowIfNull(energyHour, AthenaMedium.ChangeObjects);
                                return Ok(await MakeMessage.Electricity.PushAsync(
                                (RollingInterval.Hour, energyHour.StartTime.ToDateTime().GetHour(), energyHour.EndTime.ToDateTime().GetHour())));

                            default:
                                throw new Exception($"[{nameof(HeaderMain.DigiService).HeaderName<HeaderMain>()}.name] not as expected");
                        }
                    }
                    throw new Exception($"[{nameof(HeaderMain.DigiService).HeaderName<HeaderMain>()}] is null");
                }
                throw new Exception($"[{AthenaMedium.Parameter}] is null");
            }
            throw new Exception($"[{IManufactureClient.Label.StandardData}] is null");
        }
        catch (Exception e)
        {
            Srvcode = IManufactureClient.Label.Failure;
            return Ok(new JObject()
            {
                {
                    IManufactureClient.Label.StandardData, new JObject()
                    {
                        {
                            AthenaMedium.Execution, JObject.FromObject(new AthenaMedium.Result()
                            {
                                Code = IManufactureClient.Label.Failure,
                                SqlCode = string.Empty,
                                Description = e.Message
                            })
                        }
                    }
                }
            });
        }
        finally
        {
            Response.Headers.Add(nameof(HeaderMain.DigiSrvcode).HeaderName<HeaderMain>(), Srvcode);
            Response.Headers.Add(nameof(HeaderMain.DigiDatakey).HeaderName<HeaderMain>(), new object().ToJson());
            Response.Headers.Add(nameof(HeaderMain.DigiSrvver).HeaderName<HeaderMain>(), IManufactureClient.Label.Srvver);
            Response.Headers.Add(nameof(HeaderMain.DigiProtocol).HeaderName<HeaderMain>(), IManufactureClient.Label.Protocol);
        }
    }
    public readonly ref struct ApplicationMain
    {
        public const string ValidOrganizationDataGet = "valid.organization.data.get";
        public const string MeterDayEnergyConsumeDataGet = "meter.day.energy.consume.data.get";
        public const string MeterHourEnergyConsumeDataGet = "meter.hour.energy.consume.data.get";
    }
    public record HeaderMain
    {
        [FromHeader(Name = "digi-host")] public required string DigiHost { get; init; }
        [FromHeader(Name = "digi-service")] public required string DigiService { get; init; }
        [FromHeader(Name = "digi-srvver")] public required string DigiSrvver { get; init; }
        [FromHeader(Name = "digi-srvcode")] public required string DigiSrvcode { get; init; }
        [FromHeader(Name = "digi-datakey")] public required string DigiDatakey { get; init; }
        [FromHeader(Name = "digi-protocol")] public required string DigiProtocol { get; init; }
    }
    public readonly record struct IdentifyHostMain
    {
        [JsonProperty("prod")] public string Product { get; init; }
        [JsonProperty("ip")] public string Ip { get; init; }
        [JsonProperty("lang")] public string Language { get; init; }
        [JsonProperty("timestamp")] public string Timestamp { get; init; }
        [JsonProperty("acct")] public string Account { get; init; }
    }
    public readonly record struct IdentifyServiceMain
    {
        [JsonProperty("prod")] public string Product { get; init; }
        [JsonProperty("name")] public string Name { get; init; }
        [JsonProperty("ip")] public string Ip { get; init; }
        [JsonProperty("id")] public string Id { get; init; }
    }
    public readonly record struct EnergyRequest
    {
        [JsonProperty("enterprise_no")] public string EnterpriseNo { get; init; }
        [JsonProperty("site_no")] public string SiteNo { get; init; }
        [JsonProperty("start_time")] public string StartTime { get; init; }
        [JsonProperty("end_time")] public string EndTime { get; init; }
    }
    public readonly record struct OrganizationRequest
    {
        [JsonProperty("enterprise_no")] public string EnterpriseNo { get; init; }
        [JsonProperty("site_no")] public string SiteNo { get; init; }
        [JsonProperty("call_id")] public string CallId { get; init; }
    }
    string Srvcode { get; set; } = IManufactureClient.Label.Success;
    public required IMakeMessageWrapper MakeMessage { get; init; }
}