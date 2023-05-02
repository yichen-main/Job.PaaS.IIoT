using static IIoT.Domain.Shared.Businesses.Roots.Factories.IFactoryGroup;

namespace IIoT.Station.Apis.Edifices.Foundations;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Groups : ControllerBase
{
    [HttpGet(Name = nameof(UpperFactoryGroupAsync))]
    public async ValueTask<IActionResult> UpperFactoryGroupAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Upper> uppers = new();
                if (query.FactoryId != default)
                {
                    await foreach (var (factory, group) in BusinessManufacture.FactoryGroup.ListFactoryGroupAsync())
                    {
                        if (factory.Id == query.FactoryId) uppers.Add(new()
                        {
                            Id = group.Id,
                            FactoryNo = factory.FactoryNo,
                            GroupNo = group.GroupNo,
                            GroupName = group.GroupName,
                            Creator = group.Creator,
                            CreateTime = group.CreateTime
                        });
                    }
                }
                else await foreach (var (factory, group) in BusinessManufacture.FactoryGroup.ListFactoryGroupAsync()) uppers.Add(new()
                {
                    Id = group.Id,
                    FactoryNo = factory.FactoryNo,
                    GroupNo = group.GroupNo,
                    GroupName = group.GroupName,
                    Creator = group.Creator,
                    CreateTime = group.CreateTime
                });
                if (!string.IsNullOrWhiteSpace(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.GroupNo,
                    item.GroupName
                }.Any(item => item.Contains(query.Search))).ToList();
                Pages<Upper> results = new(uppers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperFactoryGroupAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.FactoryId
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.FactoryId
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search,
                        query.FactoryId
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search,
                        query.FactoryId
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

    [HttpGet($$"""lower/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(LowerFactoryGroupAsync))]
    public async ValueTask<IActionResult> LowerFactoryGroupAsync(Guid id, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                await foreach (var (network, equipment) in BusinessManufacture.FactoryGroup.ListNetworkEquipmentAsync(id)) lowers.Add(new()
                {
                    SessionNo = network.SessionNo,
                    SessionName = network.SessionName,
                    EquipmentNo = equipment.EquipmentNo,
                    EquipmentName = equipment.EquipmentName,
                    NetworkTransl = Terminology[network.CategoryType.ToString()]
                });
                Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerFactoryGroupAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(GetFactoryGroupAsync))]
    public async ValueTask<IActionResult> GetFactoryGroupAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entity = await BusinessManufacture.FactoryGroup.GetAsync(id);
                var factory = await BusinessManufacture.Factory.GetAsync(entity.FactoryId);
                return Ok(new Row
                {
                    FactoryId = entity.FactoryId,
                    GroupNo = entity.GroupNo,
                    GroupName = entity.GroupName,
                    FactoryNo = factory.FactoryNo,
                    FactoryName = factory.FactoryName
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

    [HttpPost(Name = nameof(InsertFactoryGroupAsync))]
    public async ValueTask<IActionResult> InsertFactoryGroupAsync([FromHeader] Header header, [FromBody] GroupInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = RegisterTrigger.PutGroup(body.FactoryId, Guid.NewGuid(), body.GroupNo);
                await BusinessManufacture.FactoryGroup.AddAsync(new()
                {
                    Id = id,
                    FactoryId = body.FactoryId,
                    GroupNo = body.GroupNo,
                    GroupName = body.GroupName ?? string.Empty,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                });
                return CreatedAtRoute(nameof(GetFactoryGroupAsync), new
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.FactoryId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.FactoryId)],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.GroupNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.GroupNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut(Name = nameof(UpdateFactoryGroupAsync))]
    public async ValueTask<IActionResult> UpdateFactoryGroupAsync([FromHeader] Header header, [FromBody] GroupUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                await BusinessManufacture.FactoryGroup.UpdateAsync(new()
                {
                    Id = body.Id,
                    FactoryId = body.FactoryId,
                    GroupNo = body.GroupNo,
                    GroupName = body.GroupName ?? string.Empty,
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.FactoryId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.FactoryId)],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.GroupNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.GroupNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteFactoryGroupAsync))]
    public async ValueTask<IActionResult> DeleteFactoryGroupAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                RegisterTrigger.IsGroup = true;
                await ClearerEvent.FactoryGroupAsync(ReduxService.ConvertGuid(identifier));
                RegisterTrigger.IsGroup = default;
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
    public sealed class Query : Satchel
    {
        public required Guid FactoryId { get; init; }
    }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required string FactoryNo { get; init; }
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Lower
    {
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string SessionNo { get; init; }
        public required string SessionName { get; init; }
        public required string NetworkTransl { get; init; }
    }
    public readonly record struct Row
    {
        public required Guid FactoryId { get; init; }
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
    }
    public sealed class GroupInsert
    {
        public required Guid FactoryId { get; init; }
        public required string GroupNo { get; init; }
        public required string? GroupName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<GroupInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.FactoryId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(FactoryId)]);
                    RuleFor(item => item.GroupNo)
                        .NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(GroupNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(GroupNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class GroupUpdate
    {
        public required Guid Id { get; init; }
        public required Guid FactoryId { get; init; }
        public required string GroupNo { get; init; }
        public required string? GroupName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<GroupUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.FactoryId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(FactoryId)]);
                    RuleFor(item => item.GroupNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(GroupNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(GroupNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public required IStringLocalizer<Search> Search { get; init; }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}