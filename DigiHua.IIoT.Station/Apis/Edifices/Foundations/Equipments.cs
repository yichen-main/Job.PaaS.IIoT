using static IIoT.Domain.Shared.Businesses.Roots.Equipments.IEquipment;

namespace IIoT.Station.Apis.Edifices.Foundations;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Equipments : ControllerBase
{
    [HttpGet(Name = nameof(UpperEquipmentAsync))]
    public async ValueTask<IActionResult> UpperEquipmentAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var multipleMission = await BusinessManufacture.Mission.GetMultipleMissionAsync();
                var uppers = multipleMission.Equipments.Select(item => new Upper
                {
                    Id = item.Id,
                    GroupId = item.GroupId,
                    NetworkId = item.NetworkId,
                    OperateType = item.OperateType,
                    OperateTransl = Terminology[item.OperateType.ToString()],
                    EquipmentNo = item.EquipmentNo,
                    EquipmentName = item.EquipmentName,
                    Creator = item.Creator,
                    CreateTime = item.CreateTime
                });
                if (query.GroupId != default) uppers = uppers.Where(item => item.GroupId == query.GroupId);
                if (query.PushTaskFilter)
                {
                    var ids = multipleMission.Entities.Select(item => item.EquipmentId).ToArray();
                    uppers = uppers.Where(item => Array.IndexOf(ids, item.Id) is Timeout.Infinite);
                }
                if (!string.IsNullOrWhiteSpace(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.EquipmentNo,
                    item.EquipmentName
                }.Any(item => item.Contains(query.Search)));
                Pages<Upper> results = new(uppers.OrderByDescending(item => item.CreateTime), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperEquipmentAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.GroupId,
                        query.PushTaskFilter
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.GroupId,
                        query.PushTaskFilter
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search,
                        query.GroupId,
                        query.PushTaskFilter
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search,
                        query.GroupId,
                        query.PushTaskFilter
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

    [HttpGet($$"""{{{nameof(equipmentNo)}}}/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(LowerEquipmentAsync))]
    public async ValueTask<IActionResult> LowerEquipmentAsync(string equipmentNo, Guid id, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                var (networkId, groupId, equipmentId) = RegisterTrigger.GetEquipment(equipmentNo);
                foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(id))
                {
                    switch (establish.ProcessType)
                    {
                        case IProcessEstablish.ProcessType.EquipmentStatus:
                            {
                                var information = await BusinessManufacture.EstablishInformation.GetAsync(establish.Id);
                                if (information.Id != default)
                                {
                                    var (status, eventTime) = RegisterTrigger.GetInformation(information.Id);
                                    if (eventTime != default) lowers.Add(new()
                                    {
                                        CategoryType = (int)IProcessEstablish.ProcessType.EquipmentStatus,
                                        CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentStatus)],
                                        DataNo = "status",
                                        DataValue = Terminology[status.ToString()],
                                        CreateTime = eventTime
                                    });
                                    else lowers.Add(new()
                                    {
                                        CategoryType = (int)IProcessEstablish.ProcessType.EquipmentStatus,
                                        CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentStatus)],
                                        DataNo = "status",
                                        DataValue = Terminology["Unused"],
                                        CreateTime = null
                                    });
                                }
                            }
                            break;

                        case IProcessEstablish.ProcessType.EquipmentOutput:
                            {
                                var (establishId, orders) = RegisterTrigger.GetEquipmentOrder(equipmentId);
                                foreach (var production in await BusinessManufacture.EstablishProduction.ListEstablishAsync(establish.Id))
                                {
                                    var (processId, dispatchNo, batchNo) = orders.Find(item => item.processId == production.Id);
                                    var (output, eventTime) = RegisterTrigger.GetProduction(processId);
                                    if (processId != default && eventTime != default) lowers.Add(new()
                                    {
                                        CategoryType = (int)IProcessEstablish.ProcessType.EquipmentOutput,
                                        CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentOutput)],
                                        DataNo = $"{production.DispatchNo}:{production.BatchNo}",
                                        DataValue = output.ToString(),
                                        CreateTime = eventTime
                                    });
                                    else lowers.Add(new()
                                    {
                                        CategoryType = (int)IProcessEstablish.ProcessType.EquipmentOutput,
                                        CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentOutput)],
                                        DataNo = $"{production.DispatchNo}:{production.BatchNo}",
                                        DataValue = Terminology["Unused"],
                                        CreateTime = null
                                    });
                                }
                            }
                            break;

                        case IProcessEstablish.ProcessType.EquipmentParameter:
                            {
                                var (establishParameterId, datas) = RegisterTrigger.GetDashboardData(equipmentId);
                                foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                                {
                                    var (processId, dataNo) = datas.Find(item => item.processId == parameter.Id);
                                    if (processId != default)
                                    {
                                        var (dataValue, eventTime) = RegisterTrigger.GetParameter(processId);
                                        lowers.Add(new()
                                        {
                                            CategoryType = (int)IProcessEstablish.ProcessType.EquipmentParameter,
                                            CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentParameter)],
                                            DataNo = parameter.DataNo,
                                            DataValue = dataValue.ToString(),
                                            CreateTime = eventTime
                                        });
                                    }
                                    else lowers.Add(new()
                                    {
                                        CategoryType = (int)IProcessEstablish.ProcessType.EquipmentParameter,
                                        CategoryTransl = Terminology[nameof(IProcessEstablish.ProcessType.EquipmentParameter)],
                                        DataNo = parameter.DataNo,
                                        DataValue = Terminology["Unused"],
                                        CreateTime = null
                                    });
                                }
                            }
                            break;
                    }
                }
                Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerEquipmentAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(GetEquipmentAsync))]
    public async ValueTask<IActionResult> GetEquipmentAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var equipment = await BusinessManufacture.Equipment.GetAsync(id);
                var networkAsync = BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                var group = await BusinessManufacture.FactoryGroup.GetAsync(equipment.GroupId);
                var factory = await BusinessManufacture.Factory.GetAsync(group.FactoryId);
                var network = await networkAsync;
                return Ok(new Row
                {
                    Id = equipment.Id,
                    NetworkType = network.CategoryType,
                    NetworkTransl = Terminology[network.CategoryType.ToString()],
                    OperateType = equipment.OperateType,
                    OperateTransl = Terminology[equipment.OperateType.ToString()],
                    FactoryNo = factory.FactoryNo,
                    FactoryName = factory.FactoryName,
                    GroupNo = group.GroupNo,
                    GroupName = group.GroupName,
                    SessionNo = network.SessionNo,
                    SessionName = network.SessionName,
                    EquipmentNo = equipment.EquipmentNo,
                    EquipmentName = equipment.EquipmentName,
                    Creator = equipment.Creator,
                    CreateTime = equipment.CreateTime
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

    [HttpPost(Name = nameof(InsertEquipmentAsync))]
    public async ValueTask<IActionResult> InsertEquipmentAsync([FromHeader] Header header, [FromBody] EquipmentInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = RegisterTrigger.PutEquipment(body.NetworkId, body.GroupId, Guid.NewGuid(), body.EquipmentNo);
                await BusinessManufacture.Equipment.AddAsync(new Entity
                {
                    Id = id,
                    GroupId = body.GroupId,
                    NetworkId = body.NetworkId,
                    OperateType = body.OperateType,
                    EquipmentNo = body.EquipmentNo,
                    EquipmentName = body.EquipmentName ?? string.Empty,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                });
                return CreatedAtRoute(nameof(GetEquipmentAsync), new
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.GroupId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.GroupId)],
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.NetworkId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.NetworkId)],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.EquipmentNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.EquipmentNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut(Name = nameof(UpdateEquipmentAsync))]
    public async ValueTask<IActionResult> UpdateEquipmentAsync([FromHeader] Header header, [FromBody] EquipmentUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                await BusinessManufacture.Equipment.UpdateAsync(new()
                {
                    Id = body.Id,
                    GroupId = body.GroupId,
                    NetworkId = body.NetworkId,
                    OperateType = body.OperateType,
                    EquipmentNo = body.EquipmentNo,
                    EquipmentName = body.EquipmentName ?? string.Empty,
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.GroupId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.GroupId)],
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.NetworkId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.NetworkId)],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.EquipmentNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.EquipmentNo)]],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteEquipmentAsync))]
    public async ValueTask<IActionResult> DeleteEquipmentAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                RegisterTrigger.IsEquipment = true;
                await ClearerEvent.EquipmentAsync(ReduxService.ConvertGuid(identifier));
                RegisterTrigger.IsEquipment = default;
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
        public required Guid GroupId { get; init; }
        public required bool PushTaskFilter { get; init; }
    }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required Guid GroupId { get; init; }
        public required Guid NetworkId { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Lower
    {
        public required int CategoryType { get; init; }
        public required string CategoryTransl { get; init; }
        public required string DataNo { get; init; }
        public required string? DataValue { get; init; }
        public required DateTime? CreateTime { get; init; }
    }
    public readonly record struct Row
    {
        public required Guid Id { get; init; }
        public required INetwork.Category NetworkType { get; init; }
        public required string NetworkTransl { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
        public required string SessionNo { get; init; }
        public required string SessionName { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public sealed class EquipmentInsert
    {
        public required Guid GroupId { get; init; }
        public required Guid NetworkId { get; init; }
        public required Operate OperateType { get; init; }
        public required string EquipmentNo { get; init; }
        public required string? EquipmentName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<EquipmentInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.GroupId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(GroupId)]);
                    RuleFor(item => item.NetworkId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(NetworkId)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.EquipmentNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(EquipmentNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(EquipmentNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class EquipmentUpdate
    {
        public required Guid Id { get; init; }
        public required Guid GroupId { get; init; }
        public required Guid NetworkId { get; init; }
        public required Operate OperateType { get; init; }
        public required string EquipmentNo { get; init; }
        public required string? EquipmentName { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<EquipmentUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.GroupId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(GroupId)]);
                    RuleFor(item => item.NetworkId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(NetworkId)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.EquipmentNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(EquipmentNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(EquipmentNo), ProhibitSign]);
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