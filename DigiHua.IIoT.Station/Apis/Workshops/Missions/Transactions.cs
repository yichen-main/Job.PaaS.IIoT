using static IIoT.Domain.Shared.Businesses.Workshops.Missions.IMissionPush;

namespace IIoT.Station.Apis.Workshops.Missions;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Transactions : ControllerBase
{
    [HttpGet(Name = nameof(ListWorkshopTransactionAsync))]
    public async ValueTask<IActionResult> ListWorkshopTransactionAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var uppers = Enumerable.Empty<Upper>();
                List<(string field, string value)> filters = new();
                if (!string.IsNullOrEmpty(query.Conditions))
                {
                    Array.ForEach(query.Conditions.Contains(',') ? query.Conditions.TrimEnd(',').Split(',') : new[]
                    {
                        query.Conditions
                    }, condition =>
                    {
                        var queries = condition.Split("@");
                        switch (queries[default])
                        {
                            case var first when first == ((int)IPushHistory.QueryCondition.ProductionEnvironment).ToString():
                                switch (queries[1])
                                {
                                    case var second when second == nameof(IPushHistory.Comparison.Equal):
                                        filters.Add((nameof(IPushHistory.Entity.EnvironmentType).To<IPushHistory.Entity>(), queries[2]));
                                        break;
                                }
                                break;
                        }
                    });
                }
                switch (query.MissionType)
                {
                    case IMission.Category.PushTask:
                        {
                            if (query.DateTimeFilter is not null)
                            {
                                var entities = await BusinessManufacture.PushHistory.ListAsync(query.DateTimeFilter, filters);
                                uppers = entities.Select(item => new Upper
                                {
                                    Id = item.Id,
                                    SituationType = item.EnvironmentType,
                                    SituationTransl = Terminology[item.EnvironmentType.ToString()],
                                    ResultMessage = item.ResultRecord,
                                    UsageTime = item.ConsumeMS,
                                    CreateTime = item.CreateTime
                                });
                            }
                        }
                        break;
                }
                Pages<Upper> results = new(uppers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListWorkshopTransactionAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.MissionType,
                        query.Conditions,
                        query.DateTimeFilter
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.MissionType,
                        query.Conditions,
                        query.DateTimeFilter
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search,
                        query.MissionType,
                        query.Conditions,
                        query.DateTimeFilter
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search,
                        query.MissionType,
                        query.Conditions,
                        query.DateTimeFilter
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(LowerWorkshopTransactionAsync))]
    public async ValueTask<IActionResult> LowerWorkshopTransactionAsync(Guid id, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                switch (query.MissionType)
                {
                    case IMission.Category.PushTask:
                        var pushHistory = await BusinessManufacture.PushHistory.GetAsync(id);
                        switch (pushHistory.EaiType)
                        {
                            case IWorkshopRawdata.EaiType.Information:
                                {
                                    var records = pushHistory.ContentRecord.ToObject<IPushHistory.InformationRecord[]>() ?? Enumerable.Empty<IPushHistory.InformationRecord>();
                                    var record = records.Select(item => new Lower()
                                    {
                                        CategoryType = pushHistory.EaiType,
                                        CategoryTransl = Terminology[pushHistory.EaiType.ToString()],
                                        EquipmentNo = item.EquipmentNo,
                                        EquipmentName = item.EquipmentName,
                                        DataNo = nameof(IProcessEstablish.ProcessType.EquipmentStatus),
                                        DataValue = RegisterTrigger.AsConverter(item.Status),
                                        CreateTime = item.EventTime
                                    });
                                    Pages<Lower> results = new(record, query.PageNumber, query.PageSize);
                                    ReduxService.AddPage(nameof(LowerWorkshopTransactionAsync), Url, Response.Headers, results, new()
                                    {
                                        PreviousPage = new
                                        {
                                            pageNumber = ReduxService.UpperPage(results.CurrentPage),
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        NextPage = new
                                        {
                                            pageNumber = ReduxService.DownPage(results.CurrentPage),
                                            pageSize = results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        FirstPage = new
                                        {
                                            pageNumber = Mark.Found,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        LastPage = new
                                        {
                                            pageNumber = results.TotalPage,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        }
                                    });
                                    return Ok(results);
                                }

                            case IWorkshopRawdata.EaiType.Production:
                                {
                                    List<Lower> lowers = new();
                                    var records = pushHistory.ContentRecord.ToObject<IPushHistory.ProductionRecord[]>() ?? Enumerable.Empty<IPushHistory.ProductionRecord>();
                                    foreach (var record in records) lowers.Add(new()
                                    {
                                        CategoryType = pushHistory.EaiType,
                                        CategoryTransl = Terminology[pushHistory.EaiType.ToString()],
                                        EquipmentNo = record.EquipmentNo,
                                        EquipmentName = record.EquipmentName,
                                        DataNo = nameof(IProcessEstablish.ProcessType.EquipmentOutput),
                                        DataValue = record.Contents.Sum(item => item.Output).ToString(),
                                        CreateTime = record.Contents.Last().EventTime
                                    });
                                    Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                                    ReduxService.AddPage(nameof(LowerWorkshopTransactionAsync), Url, Response.Headers, results, new()
                                    {
                                        PreviousPage = new
                                        {
                                            pageNumber = ReduxService.UpperPage(results.CurrentPage),
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        NextPage = new
                                        {
                                            pageNumber = ReduxService.DownPage(results.CurrentPage),
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        FirstPage = new
                                        {
                                            pageNumber = Mark.Found,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        LastPage = new
                                        {
                                            pageNumber = results.TotalPage,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        }
                                    });
                                    return Ok(results);
                                }

                            case IWorkshopRawdata.EaiType.Parameter:
                                {
                                    List<Lower> lowers = new();
                                    var records = pushHistory.ContentRecord.ToObject<IPushHistory.ParameterRecord[]>() ?? Enumerable.Empty<IPushHistory.ParameterRecord>();
                                    foreach (var record in records)
                                    {
                                        foreach (var content in record.Contents) lowers.Add(new()
                                        {
                                            CategoryType = pushHistory.EaiType,
                                            CategoryTransl = Terminology[pushHistory.EaiType.ToString()],
                                            EquipmentNo = record.EquipmentNo,
                                            EquipmentName = record.EquipmentName,
                                            DataNo = content.DataNo,
                                            DataValue = content.DataValue.ToString(),
                                            CreateTime = content.EventTime
                                        });
                                    }
                                    Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                                    ReduxService.AddPage(nameof(LowerWorkshopTransactionAsync), Url, Response.Headers, results, new()
                                    {
                                        PreviousPage = new
                                        {
                                            pageNumber = ReduxService.UpperPage(results.CurrentPage),
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        NextPage = new
                                        {
                                            pageNumber = ReduxService.DownPage(results.CurrentPage),
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        FirstPage = new
                                        {
                                            pageNumber = Mark.Found,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        },
                                        LastPage = new
                                        {
                                            pageNumber = results.TotalPage,
                                            results.PageSize,
                                            query.Search,
                                            query.MissionType,
                                            query.Conditions,
                                            query.DateTimeFilter
                                        }
                                    });
                                    return Ok(results);
                                }

                            default:
                                throw new Exception("wrong manufacture data");
                        }

                    default:
                        throw new Exception("wrong task number");
                }
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

    [HttpGet("query-conditions", Name = nameof(ListQueryConditionType))]
    public IActionResult ListQueryConditionType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(IPushHistory.QueryCondition))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(IPushHistory.QueryCondition), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListQueryConditionType), Url, Response.Headers, results, new()
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
    public sealed class Query : Satchel
    {
        public required IMission.Category MissionType { get; init; }
        public string? Conditions { get; init; }
        public string? DateTimeFilter { get; init; }
    }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required EnvironmentType SituationType { get; init; }
        public required string SituationTransl { get; init; }
        public required string ResultMessage { get; init; }
        public required long UsageTime { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Lower
    {
        public required IWorkshopRawdata.EaiType CategoryType { get; init; }
        public required string CategoryTransl { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string DataNo { get; init; }
        public required string DataValue { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}