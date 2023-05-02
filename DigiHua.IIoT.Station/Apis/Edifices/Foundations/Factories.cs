using static IIoT.Domain.Shared.Businesses.Roots.Factories.IFactory;

namespace IIoT.Station.Apis.Edifices.Foundations;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Factories : ControllerBase
{
    [HttpGet(Name = nameof(UpperFactoryAsync))]
    public async ValueTask<IActionResult> UpperFactoryAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entity = await BusinessManufacture.Factory.ListAsync();
                var uppers = entity.Select(item => new Upper
                {
                    Id = item.Id,
                    FactoryNo = item.FactoryNo,
                    FactoryName = item.FactoryName,
                    Creator = item.Creator,
                    CreateTime = item.CreateTime
                });
                if (!string.IsNullOrWhiteSpace(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.FactoryNo,
                    item.FactoryName
                }.Any(item => item.Contains(query.Search)));
                Pages<Upper> results = new(uppers.OrderByDescending(item => item.CreateTime), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperFactoryAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search
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

    [HttpGet($$"""lower/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(LowerFactoryAsync))]
    public async ValueTask<IActionResult> LowerFactoryAsync(Guid id, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                foreach (var item in await BusinessManufacture.FactoryGroup.ListFactoryAsync(id)) lowers.Add(new()
                {
                    GroupNo = item.GroupNo,
                    GroupName = item.GroupName,
                    Creator = item.Creator,
                    CreateTime = item.CreateTime
                });
                Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerFactoryAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(GetFactoryAsync))]
    public async ValueTask<IActionResult> GetFactoryAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entity = await BusinessManufacture.Factory.GetAsync(id);
                return Ok(new Row
                {
                    FactoryNo = entity.FactoryNo,
                    FactoryName = entity.FactoryName
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

    [HttpPost(Name = nameof(InsertFactoryAsync))]
    public async ValueTask<IActionResult> InsertFactoryAsync([FromHeader] Header header, [FromBody] FactoryInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = RegisterTrigger.PutFactory(Guid.NewGuid(), body.FactoryNo);
                await BusinessManufacture.Factory.AddAsync(new()
                {
                    Id = id,
                    FactoryNo = body.FactoryNo,
                    FactoryName = body.FactoryName ?? string.Empty,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                });
                return CreatedAtRoute(nameof(GetFactoryAsync), new
                {
                    id
                }, default);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.FactoryNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.FactoryNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut(Name = nameof(UpdateFactoryAsync))]
    public async ValueTask<IActionResult> UpdateFactoryAsync([FromHeader] Header header, [FromBody] FactoryUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                await BusinessManufacture.Factory.UpdateAsync(new()
                {
                    Id = body.Id,
                    FactoryNo = body.FactoryNo,
                    FactoryName = body.FactoryName ?? string.Empty,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                });
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.FactoryNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.FactoryNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteFactoryAsync))]
    public async ValueTask<IActionResult> DeleteFactoryAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                RegisterTrigger.IsFactory = true;
                await ClearerEvent.FactoryAsync(ReduxService.ConvertGuid(identifier));
                RegisterTrigger.IsFactory = default;
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
    public sealed class Query : Satchel { }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Lower
    {
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Row
    {
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
    }
    public sealed class FactoryInsert
    {
        public required string FactoryNo { get; init; }
        public required string? FactoryName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<FactoryInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.FactoryNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(FactoryNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(FactoryNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class FactoryUpdate
    {
        public required Guid Id { get; init; }
        public required string FactoryNo { get; init; }
        public required string? FactoryName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<FactoryUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.FactoryNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(FactoryNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(FactoryNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public required IStringLocalizer<Search> Search { get; init; }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}