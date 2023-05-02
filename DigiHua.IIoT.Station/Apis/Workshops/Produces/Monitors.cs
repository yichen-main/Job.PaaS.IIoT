namespace IIoT.Station.Apis.Workshops.Produces;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Monitors : ControllerBase
{
    [HttpGet(Name = nameof(ListWorkshopMonitorAsync))]
    public async ValueTask<IActionResult> ListWorkshopMonitorAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var produceStates = await BusinessManufacture.ProduceState.ListAsync();
                var uppers = produceStates.Where(item => item.EnvironmentType == query.EnvironmentType).Select(item => new Upper
                {
                    EquipmentNo = item.EquipmentNo,
                    EquipmentName = item.EquipmentName,
                    StatusType = item.EquipmentStatus,
                    StatusTransl = Terminology[item.EquipmentStatus.ToString()],
                    Description = item.Description,
                    CreateTime = item.CreateTime
                });
                if (!string.IsNullOrWhiteSpace(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.EquipmentNo,
                    item.EquipmentName
                }.Any(item => item.Contains(query.Search)));
                Pages<Upper> results = new(uppers.OrderBy(item => item.EquipmentNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListWorkshopMonitorAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.EnvironmentType
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.EnvironmentType
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search,
                        query.EnvironmentType
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search,
                        query.EnvironmentType
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

    [HttpGet("equipment-status", Name = nameof(ListEquipmentStatusType))]
    public IActionResult ListEquipmentStatusType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(IEquipment.Status))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(IEquipment.Status), item) ?? string.Empty],
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListEquipmentStatusType), Url, Response.Headers, results, new()
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
        public IMissionPush.EnvironmentType EnvironmentType { get; init; }
    }
    public readonly record struct Upper
    {
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required IEquipment.Status StatusType { get; init; }
        public required string StatusTransl { get; init; }
        public required string Description { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}