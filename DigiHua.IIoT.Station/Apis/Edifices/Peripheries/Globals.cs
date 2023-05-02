namespace IIoT.Station.Apis.Edifices.Peripheries;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Globals : ControllerBase
{
    [HttpGet(Name = nameof(ListGlobalParameterAsync))]
    public async ValueTask<IActionResult> ListGlobalParameterAsync([FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                await ManagerProfile.BuildAsync();
                return Ok(new Deploy
                {
                    SMES = new()
                    {
                        TestAreaURL = ManagerText.Assembly.ExperiBlock,
                        FormalAreaURL = ManagerText.Assembly.FormalBlock
                    },
                    Poller = new()
                    {
                        PushTask = new()
                        {
                            Frequency = int.TryParse(ManagerText.Assembly.PushCycle, out var cycle) ? cycle : Mark.Found
                        }
                    },
                    PCBA = new()
                    {
                        Url = ManagerText.Assembly.CollectBlock
                    },
                    Database = new()
                    {
                        IP = ManagerText.Hangar.Location,
                        PostgresPort = int.TryParse(ManagerText.Hangar.Merchant, out var merchant) ? merchant : 5432,
                        InfluxdbPort = int.TryParse(ManagerText.Hangar.Flowmeter, out var flowmeter) ? flowmeter : 8086,
                        Database = ManagerText.Hangar.Plaque,
                        Username = ManagerText.Hangar.Identifier,
                        Password = ManagerText.Hangar.Pespond
                    }
                });
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut(Name = nameof(UpdateGlobalParameterAsync))]
    public async ValueTask<IActionResult> UpdateGlobalParameterAsync([FromHeader] Header header, [FromBody] Deploy body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                ReduxService.CheckHttpUrl(body.PCBA.Url, nameof(body.PCBA).Joint(nameof(body.PCBA.Url)));
                ReduxService.CheckHttpUrl(body.SMES.TestAreaURL, nameof(body.SMES).Joint(nameof(body.SMES.TestAreaURL)));
                ReduxService.CheckHttpUrl(body.SMES.FormalAreaURL, nameof(body.SMES).Joint(nameof(body.SMES.FormalAreaURL)));
                ReduxService.CheckIpLocation(body.Database.IP, nameof(body.Database).Joint(nameof(body.Database.IP)));
                await FoundationTrigger.CreateFileAaync(ManagerProfile.FullPath, new IManagerProfile.Text
                {
                    Hangar = new()
                    {
                        Location = FoundationTrigger.UseEncryptAES(body.Database.IP),
                        Merchant = FoundationTrigger.UseEncryptAES(body.Database.PostgresPort.ToString()),
                        Flowmeter = FoundationTrigger.UseEncryptAES(body.Database.InfluxdbPort.ToString()),
                        Plaque = FoundationTrigger.UseEncryptAES(body.Database.Database),
                        Identifier = FoundationTrigger.UseEncryptAES(body.Database.Username),
                        Pespond = FoundationTrigger.UseEncryptAES(body.Database.Password)
                    },
                    Assembly = new()
                    {
                        PushCycle = FoundationTrigger.UseEncryptAES(body.Poller.PushTask.Frequency.ToString()),
                        ExperiBlock = FoundationTrigger.UseEncryptAES(body.SMES.TestAreaURL),
                        FormalBlock = FoundationTrigger.UseEncryptAES(body.SMES.FormalAreaURL),
                        CollectBlock = FoundationTrigger.UseEncryptAES(body.PCBA.Url)
                    }
                }, Extension.Yaml, cover: true);
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpGet("operate_types", Name = nameof(ListGlobalOperateType))]
    public IActionResult ListGlobalOperateType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(Operate))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(Operate), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListGlobalOperateType), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize
                    }
                });
                return Ok(results);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }
    public sealed class Query : Satchel { }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Deploy
    {
        public required TextSMES SMES { get; init; }
        public required TextPoller Poller { get; init; }
        public required TextPCBA PCBA { get; init; }
        public required TextDatabase Database { get; init; }
        public readonly record struct TextSMES
        {
            public required string FormalAreaURL { get; init; }
            public required string TestAreaURL { get; init; }
        }
        public readonly record struct TextPoller
        {
            public required TextPushTask PushTask { get; init; }
            public readonly record struct TextPushTask
            {
                public required int Frequency { get; init; }
            }
        }
        public readonly record struct TextPCBA
        {
            public required string Url { get; init; }
        }
        public readonly record struct TextDatabase
        {
            public required string IP { get; init; }
            public required int PostgresPort { get; init; }
            public required int InfluxdbPort { get; init; }
            public required string Database { get; init; }
            public required string Username { get; init; }
            public required string Password { get; init; }
        }
    }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IManagerProfile ManagerProfile { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
}