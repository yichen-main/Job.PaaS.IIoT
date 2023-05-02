using static IIoT.Domain.Shared.Businesses.Workshops.Missions.IMission;

namespace IIoT.Station.Apis.Workshops.Missions;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Logics : ControllerBase
{
    [HttpGet(Name = nameof(UpperWorkshopMissionLogicAsync))]
    public async ValueTask<IActionResult> UpperWorkshopMissionLogicAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Upper> uppers = new();
                var missionPush = await BusinessManufacture.MissionPush.GaugeAsync();
                if (missionPush > 0) uppers.Add(new()
                {
                    MissionType = (int)Category.PushTask,
                    MissionTransl = Terminology[nameof(Category.PushTask)],
                    MissionTotal = missionPush
                });
                Pages<Upper> results = new(uppers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperWorkshopMissionLogicAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(categoryType)}}:int}""", Name = nameof(LowerWorkshopMissionLogicFilterAsync))]
    public async ValueTask<IActionResult> LowerWorkshopMissionLogicFilterAsync(int categoryType, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                var multipleMission = await BusinessManufacture.Mission.GetMultipleMissionAsync();
                switch (categoryType)
                {
                    case (int)Category.PushTask:
                        {
                            foreach (var entity in multipleMission.Entities)
                            {
                                var push = multipleMission.Pushs.First(item => item.Id == entity.Id);
                                var equipment = multipleMission.Equipments.First(item => item.Id == entity.EquipmentId);
                                lowers.Add(new()
                                {
                                    Id = entity.Id,
                                    MissionType = Category.PushTask,
                                    MissionTransl = Terminology[nameof(Category.PushTask)],
                                    OperateType = push.OperateType,
                                    OperateTransl = Terminology[push.OperateType.ToString()],
                                    EquipmentNo = equipment.EquipmentNo,
                                    EquipmentName = equipment.EquipmentName,
                                    Creator = entity.Creator,
                                    CreateTime = entity.CreateTime
                                });
                            }
                            if (!string.IsNullOrEmpty(query.EquipmentNo))
                            {
                                lowers = lowers.Where(item => item.EquipmentNo == query.EquipmentNo).ToList();
                            }
                        }
                        break;

                    default:
                        throw new Exception($"[{categoryType}] mission EaiType wrong");
                }
                Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerWorkshopMissionLogicFilterAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        pageSize = results.PageSize,
                        search = query.Search,
                        equipmentNo = query.EquipmentNo
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search,
                        query.EquipmentNo
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search,
                        query.EquipmentNo
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search,
                        query.EquipmentNo
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(GetWorkshopMissionLogicAsync))]
    public async ValueTask<IActionResult> GetWorkshopMissionLogicAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                object result = new();
                var singleMission = await BusinessManufacture.Mission.GetSingleMissionAsync(id);
                switch (singleMission.Entity.CategoryType)
                {
                    case Category.PushTask:
                        {
                            var equipment = singleMission.Equipments.First(item => item.Id == singleMission.Entity.EquipmentId);
                            var group = await BusinessManufacture.FactoryGroup.GetAsync(equipment.GroupId);
                            var factory = await BusinessManufacture.Factory.GetAsync(group.FactoryId);
                            result = new Row
                            {
                                Id = singleMission.Entity.Id,
                                EquipmentId = equipment.Id,
                                MissionType = Category.PushTask,
                                MissionTransl = Terminology[nameof(Category.PushTask)],
                                OperateType = singleMission.Push.OperateType,
                                OperateTransl = Terminology[singleMission.Push.OperateType.ToString()],
                                FactoryNo = factory.FactoryNo,
                                FactoryName = factory.FactoryName,
                                GroupNo = group.GroupNo,
                                GroupName = group.GroupName,
                                EquipmentNo = equipment.EquipmentNo,
                                EquipmentName = equipment.EquipmentName,
                                Creator = singleMission.Entity.Creator,
                                CreateTime = singleMission.Entity.CreateTime,
                                Part = new
                                {
                                    singleMission.Push.EnvironmentType,
                                    SituationTransl = Terminology[singleMission.Push.EnvironmentType.ToString()].ToString()
                                }
                            };
                        }
                        break;

                    default:
                        throw new Exception($"[{singleMission.Entity.CategoryType}] mission EaiType wrong");
                }
                return Ok(result);
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

    [HttpPost($$"""{{{nameof(categoryType)}}:int}""", Name = nameof(InsertWorkshopMissionLogicAsync))]
    public async ValueTask<IActionResult> InsertWorkshopMissionLogicAsync(int categoryType, [FromHeader] Header header, [FromQuery] Query query, [FromBody] MissionInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = Guid.NewGuid();
                switch (categoryType)
                {
                    case (int)Category.PushTask:
                        if (body.Part is not null)
                        {
                            var part = JToken.FromObject(body.Part).ToObject<PushPart>();
                            if (Enum.IsDefined(typeof(IMissionPush.EnvironmentType), part.SituationType))
                            {
                                await BusinessManufacture.Mission.AddAsync(new()
                                {
                                    Id = id,
                                    CategoryType = Category.PushTask,
                                    EquipmentId = body.EquipmentId,
                                    Creator = body.Creator,
                                    CreateTime = DateTime.UtcNow
                                }, new IMissionPush.Entity
                                {
                                    Id = id,
                                    OperateType = body.OperateType,
                                    EnvironmentType = part.SituationType
                                });
                            }
                            else throw new Exception("situationType enum does not match");
                        }
                        else throw new Exception("part is null");
                        break;

                    default:
                        throw new Exception($"[{categoryType}] mission EaiType wrong");
                }
                return CreatedAtRoute(nameof(GetWorkshopMissionLogicAsync), new
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.EquipmentId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.EquipmentId)],
                        var item when item.Contains(LinkCategoryType) => Fielder["db.equipment.mission.repeat.setting"],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut($$"""{{{nameof(categoryType)}}:int}""", Name = nameof(UpdateWorkshopMissionLogicAsync))]
    public async ValueTask<IActionResult> UpdateWorkshopMissionLogicAsync(int categoryType, [FromHeader] Header header, [FromQuery] Query query, [FromBody] MissionUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                switch (categoryType)
                {
                    case (int)Category.PushTask:
                        if (body.Part is not null)
                        {
                            var part = JToken.FromObject(body.Part).ToObject<PushPart>();
                            if (Enum.IsDefined(typeof(IMissionPush.EnvironmentType), part.SituationType))
                            {
                                await BusinessManufacture.Mission.UpdateAsync(new()
                                {
                                    Id = body.Id,
                                    CategoryType = Category.PushTask,
                                    EquipmentId = body.EquipmentId,
                                    Creator = body.Creator,
                                    CreateTime = DateTime.UtcNow
                                }, new IMissionPush.Entity
                                {
                                    Id = body.Id,
                                    OperateType = body.OperateType,
                                    EnvironmentType = part.SituationType
                                });
                            }
                            else throw new Exception("situationType enum does not match");
                        }
                        else throw new Exception("part is null");
                        break;

                    default:
                        throw new Exception($"[{categoryType}] mission EaiType wrong");
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.EquipmentId).To<Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.EquipmentId)],
                        var item when item.Contains(LinkCategoryType) => Fielder["db.equipment.mission.repeat.setting"],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(categoryType)}}:int}/{{{nameof(identifier)}}}""", Name = nameof(DeleteWorkshopMissionLogicAsync))]
    public async ValueTask<IActionResult> DeleteWorkshopMissionLogicAsync(int categoryType, string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var ids = ReduxService.ConvertGuid(identifier);
                switch (categoryType)
                {
                    case (int)Category.PushTask:
                        {
                            List<Task<Entity>> entitiesAsync = new();
                            foreach (var id in ids) entitiesAsync.Add(BusinessManufacture.Mission.GetAsync(id));
                            await ClearerEvent.MissionAsync(await Task.WhenAll(entitiesAsync));
                        }
                        break;

                    default:
                        throw new Exception($"[{categoryType}] mission EaiType wrong");
                }
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

    [HttpGet("item-types", Name = nameof(ListWorkshopMissionLogicType))]
    public IActionResult ListWorkshopMissionLogicType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(Category))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(Category), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListWorkshopMissionLogicType), Url, Response.Headers, results, new()
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

    [HttpGet("environment-types", Name = nameof(ListProductionEnvironmentType))]
    public IActionResult ListProductionEnvironmentType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(IMissionPush.EnvironmentType))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(IMissionPush.EnvironmentType), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListProductionEnvironmentType), Url, Response.Headers, results, new()
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
        public string? EquipmentNo { get; init; }
    }
    public readonly record struct Upper
    {
        public required int MissionType { get; init; }
        public required string MissionTransl { get; init; }
        public required int MissionTotal { get; init; }
    }
    public readonly record struct Lower
    {
        public required Guid Id { get; init; }
        public required Category MissionType { get; init; }
        public required string MissionTransl { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Row
    {
        public required Guid Id { get; init; }
        public required Guid EquipmentId { get; init; }
        public required Category MissionType { get; init; }
        public required string MissionTransl { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
        public required object Part { get; init; }
    }
    public sealed class MissionInsert
    {
        public required Guid EquipmentId { get; init; }
        public required Operate OperateType { get; init; }
        public required string Creator { get; init; }
        public required object Part { get; init; }
        public sealed class Validator : AbstractValidator<MissionInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.EquipmentId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(EquipmentId)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class MissionUpdate
    {
        public required Guid Id { get; init; }
        public required Guid EquipmentId { get; init; }
        public required Operate OperateType { get; init; }
        public required string Creator { get; init; }
        public required object Part { get; init; }
        public sealed class Validator : AbstractValidator<MissionUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.EquipmentId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(EquipmentId)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public readonly record struct PushPart
    {
        public required IMissionPush.EnvironmentType SituationType { get; init; }
    }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}