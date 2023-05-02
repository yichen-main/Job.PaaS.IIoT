using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IProcessEstablish;

namespace IIoT.Station.Apis.Workshops.Produces;

[Authorize, EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Parameters : ControllerBase
{
    [HttpGet(Name = nameof(UpperWorkshopDataAsync))]
    public async ValueTask<IActionResult> UpperWorkshopDataAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Upper> uppers = new();
                var establishes = await BusinessManufacture.ProcessEstablish.ListAsync();
                foreach (var establish in establishes.DistinctBy(item => item.EquipmentId))
                {
                    var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
                    var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                    uppers.Add(new()
                    {
                        EquipmentId = equipment.Id,
                        NetworkType = network.CategoryType,
                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                        EquipmentNo = equipment.EquipmentNo,
                        EquipmentName = equipment.EquipmentName
                    });
                }
                if (!string.IsNullOrEmpty(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.EquipmentNo,
                    item.EquipmentName
                }.Any(item => item.Contains(query.Search))).ToList();
                Pages<Upper> results = new(uppers.OrderBy(item => item.EquipmentNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperWorkshopDataAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(equipmentId)}}:{{nameof(Guid)}}}""", Name = nameof(LowerWorkshopDataAsync))]
    public async ValueTask<IActionResult> LowerWorkshopDataAsync(Guid equipmentId, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                var opcUasAsync = BusinessManufacture.OpcUaProcess.ListEquipmentAsync(equipmentId);
                var establishes = await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipmentId);
                var opcUas = await opcUasAsync;
                foreach (var establish in establishes)
                {
                    switch (establish.ProcessType)
                    {
                        case ProcessType.EquipmentStatus:
                            {
                                var opcUa = opcUas.FirstOrDefault(item => item.Id == establish.Id);
                                lowers.Add(new()
                                {
                                    Id = establish.Id,
                                    ProcessType = ProcessType.EquipmentStatus,
                                    ProcessTransl = Terminology[nameof(ProcessType.EquipmentStatus)],
                                    DataNo = "status",
                                    DataPath = opcUa.NodePath
                                });
                            }
                            break;

                        case ProcessType.EquipmentOutput:
                            {
                                foreach (var production in await BusinessManufacture.EstablishProduction.ListEstablishAsync(establish.Id))
                                {
                                    var opcUa = opcUas.FirstOrDefault(item => item.Id == production.Id);
                                    lowers.Add(new()
                                    {
                                        Id = production.Id,
                                        ProcessType = ProcessType.EquipmentOutput,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentOutput)],
                                        DataNo = "counter",
                                        DataPath = opcUa.NodePath
                                    });
                                }
                            }
                            break;

                        case ProcessType.EquipmentParameter:
                            {
                                foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                                {
                                    var opcUa = opcUas.FirstOrDefault(item => item.Id == parameter.Id);
                                    lowers.Add(new()
                                    {
                                        Id = parameter.Id,
                                        ProcessType = ProcessType.EquipmentParameter,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentParameter)],
                                        DataNo = parameter.DataNo,
                                        DataPath = opcUa.NodePath
                                    });
                                }
                            }
                            break;
                    }
                }
                Pages<Lower> results = new(lowers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerWorkshopDataAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(processType)}}:int}/{{{nameof(processId)}}:{{nameof(Guid)}}}""", Name = nameof(GetParameterEditorAsync))]
    public async ValueTask<IActionResult> GetParameterEditorAsync(int processType, Guid processId, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                object result = new();
                switch (processType)
                {
                    case (int)ProcessType.EquipmentStatus:
                        {
                            var establish = await BusinessManufacture.ProcessEstablish.GetAsync(processId);
                            var information = await BusinessManufacture.EstablishInformation.GetAsync(processId);
                            var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
                            var networkAsync = BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                            var group = await BusinessManufacture.FactoryGroup.GetAsync(equipment.GroupId);
                            var factory = await BusinessManufacture.Factory.GetAsync(group.FactoryId);
                            var mapper = new IEstablishInformation.StatusLabel
                            {
                                Run = information.Run,
                                Idle = information.Idle,
                                Error = information.Error,
                                Setup = information.Setup,
                                Shutdown = information.Shutdown,
                                Maintenance = information.Maintenance,
                                Repair = information.Repair,
                                Hold = information.Hold
                            };
                            var network = await networkAsync;
                            switch (network.CategoryType)
                            {
                                case INetwork.Category.PassiveReception:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentStatus,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentStatus)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = mapper
                                    };
                                    break;

                                case INetwork.Category.MessageQueuingTelemetryTransport:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentStatus,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentStatus)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = mapper
                                    };
                                    break;

                                case INetwork.Category.OPCUnifiedArchitecture:
                                    var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(processId);
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentStatus,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentStatus)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = new ExecutionOpcUa
                                        {
                                            NodePath = opcUa.NodePath
                                        },
                                        Mapper = mapper
                                    };
                                    break;

                                default:
                                    throw new Exception($"[{network.CategoryType}] network.ProcessType wrong");
                            }
                        }
                        break;

                    case (int)ProcessType.EquipmentOutput:
                        {
                            var production = await BusinessManufacture.EstablishProduction.GetAsync(processId);
                            var establish = await BusinessManufacture.ProcessEstablish.GetAsync(production.EstablishId);
                            var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
                            var networkAsync = BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                            var group = await BusinessManufacture.FactoryGroup.GetAsync(equipment.GroupId);
                            var factory = await BusinessManufacture.Factory.GetAsync(group.FactoryId);
                            var network = await networkAsync;
                            switch (network.CategoryType)
                            {
                                case INetwork.Category.PassiveReception:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentOutput,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentOutput)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = default
                                    };
                                    break;

                                case INetwork.Category.MessageQueuingTelemetryTransport:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentOutput,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentOutput)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = default
                                    };
                                    break;

                                case INetwork.Category.OPCUnifiedArchitecture:
                                    var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(processId);
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentOutput,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentOutput)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = new ExecutionOpcUa
                                        {
                                            NodePath = opcUa.NodePath
                                        },
                                        Mapper = default
                                    };
                                    break;

                                default:
                                    throw new Exception($"[{network.CategoryType}] network.ProcessType wrong");
                            }
                        }
                        break;

                    case (int)ProcessType.EquipmentParameter:
                        {
                            var parameter = await BusinessManufacture.EstablishParameter.GetAsync(processId);
                            var establish = await BusinessManufacture.ProcessEstablish.GetAsync(parameter.EstablishId);
                            var equipment = await BusinessManufacture.Equipment.GetAsync(establish.EquipmentId);
                            var networkAsync = BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                            var group = await BusinessManufacture.FactoryGroup.GetAsync(equipment.GroupId);
                            var factory = await BusinessManufacture.Factory.GetAsync(group.FactoryId);
                            var network = await networkAsync;
                            switch (network.CategoryType)
                            {
                                case INetwork.Category.PassiveReception:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentParameter,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentParameter)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = default
                                    };
                                    break;

                                case INetwork.Category.MessageQueuingTelemetryTransport:
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentParameter,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentParameter)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = default,
                                        Mapper = default
                                    };
                                    break;

                                case INetwork.Category.OPCUnifiedArchitecture:
                                    var opcUa = await BusinessManufacture.OpcUaProcess.GetAsync(processId);
                                    result = new Row
                                    {
                                        Id = processId,
                                        EquipmentId = equipment.Id,
                                        NetworkType = network.CategoryType,
                                        NetworkTransl = Terminology[network.CategoryType.ToString()],
                                        ProcessType = ProcessType.EquipmentParameter,
                                        ProcessTransl = Terminology[nameof(ProcessType.EquipmentParameter)],
                                        FactoryNo = factory.FactoryNo,
                                        FactoryName = factory.FactoryName,
                                        GroupNo = group.GroupNo,
                                        GroupName = group.GroupName,
                                        SessionNo = network.SessionNo,
                                        SessionName = network.SessionName,
                                        EquipmentNo = equipment.EquipmentNo,
                                        EquipmentName = equipment.EquipmentName,
                                        Detail = new ExecutionOpcUa
                                        {
                                            NodePath = opcUa.NodePath
                                        },
                                        Mapper = default
                                    };
                                    break;

                                default:
                                    throw new Exception($"[{network.CategoryType}] network.ProcessType wrong");
                            }
                        }
                        break;

                    default:
                        throw new Exception($"[{processType}] processType wrong");
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

    [HttpPost($$"""{{{nameof(processType)}}:int}""", Name = nameof(InsertWorkshopParameterAsync))]
    public async ValueTask<IActionResult> InsertWorkshopParameterAsync(int processType, [FromHeader] Header header, [FromQuery] Query query, [FromBody] ProcessInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var equipment = await BusinessManufacture.Equipment.GetAsync(body.EquipmentId);
                var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                var establishes = await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipment.Id);
                switch (processType)
                {
                    case (int)ProcessType.EquipmentStatus:
                        {
                            if (body.Mapper is not null)
                            {
                                var establish = establishes.FirstOrDefault(item => item.ProcessType is ProcessType.EquipmentStatus);
                                if (establish.Id != default) throw new Exception(Fielder["db.equipment.data.repeat.setting"]);
                                var mapper = JToken.FromObject(body.Mapper).ToObject<IEstablishInformation.StatusLabel>();
                                var establishId = RegisterTrigger.PutEstablishInformation(equipment.Id, Guid.NewGuid(), new IEstablishInformation.StatusLabel
                                {
                                    Run = mapper.Run,
                                    Idle = mapper.Idle,
                                    Error = mapper.Error,
                                    Setup = mapper.Setup,
                                    Shutdown = mapper.Shutdown,
                                    Maintenance = mapper.Maintenance,
                                    Repair = mapper.Repair,
                                    Hold = mapper.Hold
                                });
                                switch (network.CategoryType)
                                {
                                    case INetwork.Category.OPCUnifiedArchitecture:
                                        if (body.Detail is not null)
                                        {
                                            var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                            await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                            {
                                                Id = establishId,
                                                EquipmentId = equipment.Id,
                                                ProcessType = ProcessType.EquipmentStatus,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, new IEstablishInformation.Entity
                                            {
                                                Id = establishId,
                                                Run = mapper.Run,
                                                Idle = mapper.Idle,
                                                Error = mapper.Error,
                                                Setup = mapper.Setup,
                                                Shutdown = mapper.Shutdown,
                                                Maintenance = mapper.Maintenance,
                                                Repair = mapper.Repair,
                                                Hold = mapper.Hold
                                            }, new IOpcUaProcess.Entity
                                            {
                                                Id = establishId,
                                                EquipmentId = body.EquipmentId,
                                                NodePath = detail.NodePath ?? string.Empty
                                            });
                                        }
                                        break;

                                    default:
                                        await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                        {
                                            Id = establishId,
                                            EquipmentId = equipment.Id,
                                            ProcessType = ProcessType.EquipmentStatus,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new IEstablishInformation.Entity
                                        {
                                            Id = establishId,
                                            Run = mapper.Run,
                                            Idle = mapper.Idle,
                                            Error = mapper.Error,
                                            Setup = mapper.Setup,
                                            Shutdown = mapper.Shutdown,
                                            Maintenance = mapper.Maintenance,
                                            Repair = mapper.Repair,
                                            Hold = mapper.Hold
                                        });
                                        break;
                                }
                            }
                            else throw new Exception("mapper is null");
                        }
                        break;

                    case (int)ProcessType.EquipmentOutput:
                        {
                            var establish = establishes.FirstOrDefault(item => item.ProcessType is ProcessType.EquipmentOutput);
                            if (establish.Id != default) throw new Exception(Fielder["db.equipment.data.repeat.setting"]);
                            var (establishId, processId) = RegisterTrigger.PutEstablishProduction(equipment.Id, Guid.NewGuid(), Guid.NewGuid(), string.Empty, string.Empty);
                            switch (network.CategoryType)
                            {
                                case INetwork.Category.OPCUnifiedArchitecture:
                                    if (body.Detail is not null)
                                    {
                                        var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                        await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                        {
                                            Id = establishId,
                                            EquipmentId = equipment.Id,
                                            ProcessType = ProcessType.EquipmentOutput,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new[]
                                        {
                                            (new IEstablishProduction.Entity
                                            {
                                                Id = processId,
                                                EstablishId = establishId,
                                                DispatchNo = string.Empty,
                                                BatchNo= string.Empty
                                            }, new IOpcUaProcess.Entity
                                            {
                                                Id = processId,
                                                EquipmentId = equipment.Id,
                                                NodePath = detail.NodePath ?? string.Empty
                                            })
                                        });
                                    }
                                    break;

                                default:
                                    await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                    {
                                        Id = establishId,
                                        EquipmentId = equipment.Id,
                                        ProcessType = ProcessType.EquipmentOutput,
                                        Creator = body.Creator,
                                        CreateTime = DateTime.UtcNow
                                    }, new[]
                                    {
                                        new IEstablishProduction.Entity
                                        {
                                            Id = processId,
                                            EstablishId = establishId,
                                            DispatchNo = string.Empty,
                                            BatchNo = string.Empty
                                        }
                                    });
                                    break;
                            }
                        }
                        break;

                    case (int)ProcessType.EquipmentParameter:
                        {
                            if (body.Part is not null)
                            {
                                var processId = Guid.NewGuid();
                                var part = JToken.FromObject(body.Part).ToObject<ParameterPart>();
                                var establishId = establishes.FirstOrDefault(item => item.ProcessType is ProcessType.EquipmentParameter).Id;
                                if (establishId == default) establishId = Guid.NewGuid();
                                var parameters = await BusinessManufacture.EstablishParameter.ListEstablishAsync(establishId);
                                switch (network.CategoryType)
                                {
                                    case INetwork.Category.OPCUnifiedArchitecture:
                                        if (body.Detail is not null)
                                        {
                                            var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                            if (!parameters.Any(item => item.DataNo == part.DataNo)) await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                            {
                                                Id = establishId,
                                                EquipmentId = equipment.Id,
                                                ProcessType = ProcessType.EquipmentParameter,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, new[]
                                            {
                                                (new IEstablishParameter.Entity
                                                {
                                                    Id = processId,
                                                    EstablishId = establishId,
                                                    DataNo= part.DataNo
                                                }, new IOpcUaProcess.Entity
                                                {
                                                    Id = processId,
                                                    EquipmentId = body.EquipmentId,
                                                    NodePath = detail.NodePath ?? string.Empty
                                                })
                                            });
                                            else throw new Exception(Fielder["db.equipment.data.repeat.setting"]);
                                            RegisterTrigger.PutEstablishParameter(equipment.Id, establishId, processId, part.DataNo);
                                        }
                                        break;

                                    default:
                                        if (!parameters.Any(item => item.DataNo == part.DataNo)) await BusinessManufacture.ProcessEstablish.AddAsync(new()
                                        {
                                            Id = establishId,
                                            EquipmentId = equipment.Id,
                                            ProcessType = ProcessType.EquipmentParameter,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new[]
                                        {
                                            new IEstablishParameter.Entity
                                            {
                                                Id = processId,
                                                EstablishId = establishId,
                                                DataNo= part.DataNo
                                            }
                                        });
                                        else throw new Exception(Fielder["db.equipment.data.repeat.setting"]);
                                        RegisterTrigger.PutEstablishParameter(equipment.Id, establishId, processId, part.DataNo);
                                        break;
                                }
                            }
                            else throw new Exception("part is null");
                        }
                        break;

                    default:
                        throw new Exception($"[{processType}] processType wrong");
                }
                return CreatedAtRoute(nameof(GetParameterEditorAsync), new
                {
                    processType,
                    processId = Guid.NewGuid()
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
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<IEstablishProduction.Entity>(), nameof(IEstablishProduction.Entity.EstablishId).To<IEstablishProduction.Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(IEstablishProduction.Entity.EstablishId)],
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<IEstablishParameter.Entity>(), nameof(IEstablishParameter.Entity.EstablishId).To<IEstablishParameter.Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(IEstablishParameter.Entity.EstablishId)],
                        var item when item.Contains(IEstablishParameter.LinkDataNo) => Fielder["db.equipment.data.repeat.setting"],
                        var item when item.Contains(IOpcUaProcess.LinkNode) => Fielder["db.network.opcua.node.path.repeat.setting"],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut($$"""{{{nameof(processType)}}:int}""", Name = nameof(UpdateWorkshopParameterAsync))]
    public async ValueTask<IActionResult> UpdateWorkshopParameterAsync(int processType, [FromHeader] Header header, [FromBody] ProcessUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var equipment = await BusinessManufacture.Equipment.GetAsync(body.EquipmentId);
                var network = await BusinessManufacture.Network.GetAsync(equipment.NetworkId);
                switch (processType)
                {
                    case (int)ProcessType.EquipmentStatus:
                        {
                            if (body.Mapper is not null)
                            {
                                var mapper = JToken.FromObject(body.Mapper).ToObject<IEstablishInformation.StatusLabel>();
                                RegisterTrigger.PutEstablishInformation(equipment.Id, body.Id, new IEstablishInformation.StatusLabel
                                {
                                    Run = mapper.Run,
                                    Idle = mapper.Idle,
                                    Error = mapper.Error,
                                    Setup = mapper.Setup,
                                    Shutdown = mapper.Shutdown,
                                    Maintenance = mapper.Maintenance,
                                    Repair = mapper.Repair,
                                    Hold = mapper.Hold
                                });
                                switch (network.CategoryType)
                                {
                                    case INetwork.Category.OPCUnifiedArchitecture:
                                        {
                                            if (body.Detail is not null)
                                            {
                                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                                await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                                {
                                                    Id = body.Id,
                                                    EquipmentId = equipment.Id,
                                                    ProcessType = ProcessType.EquipmentStatus,
                                                    Creator = body.Creator,
                                                    CreateTime = DateTime.UtcNow
                                                }, new IEstablishInformation.Entity
                                                {
                                                    Id = body.Id,
                                                    Run = mapper.Run,
                                                    Idle = mapper.Idle,
                                                    Error = mapper.Error,
                                                    Setup = mapper.Setup,
                                                    Shutdown = mapper.Shutdown,
                                                    Maintenance = mapper.Maintenance,
                                                    Repair = mapper.Repair,
                                                    Hold = mapper.Hold
                                                }, new IOpcUaProcess.Entity
                                                {
                                                    Id = body.Id,
                                                    EquipmentId = body.EquipmentId,
                                                    NodePath = detail.NodePath ?? string.Empty
                                                });
                                            }
                                        }
                                        break;

                                    default:
                                        await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                        {
                                            Id = body.Id,
                                            EquipmentId = equipment.Id,
                                            ProcessType = ProcessType.EquipmentStatus,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new IEstablishInformation.Entity
                                        {
                                            Id = body.Id,
                                            Run = mapper.Run,
                                            Idle = mapper.Idle,
                                            Error = mapper.Error,
                                            Setup = mapper.Setup,
                                            Shutdown = mapper.Shutdown,
                                            Maintenance = mapper.Maintenance,
                                            Repair = mapper.Repair,
                                            Hold = mapper.Hold
                                        });
                                        break;
                                }
                            }
                            else throw new Exception("mapper is null");
                        }
                        break;

                    case (int)ProcessType.EquipmentOutput:
                        {
                            var production = await BusinessManufacture.EstablishProduction.GetAsync(body.Id);
                            var establish = await BusinessManufacture.ProcessEstablish.GetAsync(production.EstablishId);
                            switch (network.CategoryType)
                            {
                                case INetwork.Category.OPCUnifiedArchitecture:
                                    {
                                        if (body.Detail is not null)
                                        {
                                            var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                            await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                            {
                                                Id = establish.Id,
                                                EquipmentId = equipment.Id,
                                                ProcessType = ProcessType.EquipmentOutput,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, production, new IOpcUaProcess.Entity
                                            {
                                                Id = production.Id,
                                                EquipmentId = body.EquipmentId,
                                                NodePath = detail.NodePath ?? string.Empty
                                            });
                                        }
                                    }
                                    break;

                                default:
                                    await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                    {
                                        Id = establish.Id,
                                        EquipmentId = equipment.Id,
                                        ProcessType = ProcessType.EquipmentOutput,
                                        Creator = body.Creator,
                                        CreateTime = DateTime.UtcNow
                                    }, production);
                                    break;
                            }
                        }
                        break;

                    case (int)ProcessType.EquipmentParameter:
                        {
                            var parameter = await BusinessManufacture.EstablishParameter.GetAsync(body.Id);
                            var establish = await BusinessManufacture.ProcessEstablish.GetAsync(parameter.EstablishId);
                            if (body.Part is not null)
                            {
                                var part = JToken.FromObject(body.Part).ToObject<ParameterPart>();
                                switch (network.CategoryType)
                                {
                                    case INetwork.Category.OPCUnifiedArchitecture:
                                        {
                                            if (body.Detail is not null)
                                            {
                                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                                await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                                {
                                                    Id = establish.Id,
                                                    EquipmentId = body.EquipmentId,
                                                    ProcessType = ProcessType.EquipmentParameter,
                                                    Creator = body.Creator,
                                                    CreateTime = DateTime.UtcNow
                                                }, parameter, new IOpcUaProcess.Entity
                                                {
                                                    Id = parameter.Id,
                                                    EquipmentId = body.EquipmentId,
                                                    NodePath = detail.NodePath ?? string.Empty
                                                });
                                            }
                                        }
                                        break;

                                    default:
                                        await BusinessManufacture.ProcessEstablish.UpdateAsync(new()
                                        {
                                            Id = establish.Id,
                                            EquipmentId = body.EquipmentId,
                                            ProcessType = ProcessType.EquipmentParameter,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, parameter);
                                        break;
                                }
                            }
                            else throw new Exception("part is null");
                        }
                        break;
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<Entity>(), nameof(Entity.EquipmentId).To<IProcessEstablish.Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(Entity.EquipmentId)],
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<IEstablishProduction.Entity>(), nameof(IEstablishProduction.Entity.EstablishId).To<IEstablishProduction.Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(IEstablishProduction.Entity.EstablishId)],
                        var item when item.Contains(ReduxService.MakeForeignKeyIndexTag(TableName<IEstablishParameter.Entity>(), nameof(IEstablishParameter.Entity.EstablishId).To<IEstablishParameter.Entity>())) =>
                        Fielder["db.foreign.key.does.not.exist", nameof(IEstablishParameter.Entity.EstablishId)],
                        var item when item.Contains(IEstablishParameter.LinkDataNo) => Fielder["db.equipment.data.repeat.setting"],
                        var item when item.Contains(IOpcUaProcess.LinkNode) => Fielder["db.network.opcua.node.path.repeat.setting"],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpGet($$"""export-nodes/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(ExportParameterNodeAsync))]
    public async ValueTask<IActionResult> ExportParameterNodeAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<ParameterCSV> results = new();
                var establishesAsync = BusinessManufacture.ProcessEstablish.ListEquipmentAsync(id);
                var opcUas = await BusinessManufacture.OpcUaProcess.ListEquipmentAsync(id);
                foreach (var establish in await establishesAsync)
                {
                    switch (establish.ProcessType)
                    {
                        case ProcessType.EquipmentParameter:
                            foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                            {
                                results.Add(new()
                                {
                                    DataNo = parameter.DataNo,
                                    NodeId = opcUas.First(item => item.Id == parameter.Id).NodePath
                                });
                            }
                            break;
                    }
                }
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

    [HttpPost($$"""import-nodes/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(ImportParameterNodeAsync))]
    public async ValueTask<IActionResult> ImportParameterNodeAsync(Guid id, [FromHeader] Header header, [FromBody] IEnumerable<ParameterCSV> bodies)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var userAsync = ReduxService.UserInfoAsync(User);
                var equipmentAsync = BusinessManufacture.Equipment.GetAsync(id);
                var networkAsync = BusinessManufacture.Equipment.GetNetworkAsync(id);
                if (!bodies.Any()) throw new Exception(Fielder["import.data.is.zero"]);
                await Task.WhenAll(new[]
                {
                    Task.Run(() =>
                    {
                        var dataNos = bodies.Select(item => item.DataNo).ToArray();
                        var repeats = FoundationTrigger.FindRepeat(dataNos);
                        if (dataNos.Any(item => string.IsNullOrWhiteSpace(item))) throw new Exception(Fielder["field.cannot.be.invalid.content", nameof(ParameterCSV.DataNo)]);
                        if (repeats.Any()) throw new Exception(Fielder["field.invalid.duplicate.value", nameof(ParameterCSV.DataNo), string.Join(",\u00A0", repeats.Distinct())]);
                    }),
                    Task.Run(() =>
                    {
                        var nodeIds = bodies.Select(item => item.NodeId).ToArray();
                        var repeats = FoundationTrigger.FindRepeat(nodeIds);
                        if (nodeIds.Any(item => string.IsNullOrWhiteSpace(item))) throw new Exception(Fielder["field.cannot.be.invalid.content", nameof(ParameterCSV.NodeId)]);
                        if (repeats.Any()) throw new Exception(Fielder["field.invalid.duplicate.value", nameof(ParameterCSV.NodeId), string.Join(",\u00A0", repeats.Distinct())]);
                    })
                });
                var network = await networkAsync;
                if (network.Id != default && network.CategoryType is INetwork.Category.OPCUnifiedArchitecture)
                {
                    var user = await userAsync;
                    var equipment = await equipmentAsync;
                    foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipment.Id))
                    {
                        switch (establish.ProcessType)
                        {
                            case ProcessType.EquipmentParameter:
                                foreach (var parameter in await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id))
                                {
                                    await ClearerEvent.EstablishParameterAsync(parameter);
                                }
                                break;
                        }
                    }
                    var establishId = Guid.NewGuid();
                    List<(IEstablishParameter.Entity process, IOpcUaProcess.Entity opcUa)> parameters = new();
                    foreach (var body in bodies)
                    {
                        var processId = Guid.NewGuid();
                        parameters.Add((new()
                        {
                            Id = processId,
                            EstablishId = establishId,
                            DataNo = body.DataNo
                        }, new()
                        {
                            Id = processId,
                            EquipmentId = equipment.Id,
                            NodePath = body.NodeId
                        }));
                    }
                    await BusinessManufacture.ProcessEstablish.AddAsync(new()
                    {
                        Id = Guid.NewGuid(),
                        EquipmentId = equipment.Id,
                        ProcessType = ProcessType.EquipmentParameter,
                        Creator = user.Username,
                        CreateTime = DateTime.UtcNow,
                    }, parameters);
                }
                return CreatedAtRoute(nameof(ExportParameterNodeAsync), new
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
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteWorkshopParameterAsync))]
    public async ValueTask<IActionResult> DeleteWorkshopParameterAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                RegisterTrigger.IsEstablish = true;
                await Parallel.ForEachAsync(ReduxService.ConvertGuid(identifier), new ParallelOptions
                {
                    MaxDegreeOfParallelism = 2
                }, async (id, _) =>
                {
                    var information = await BusinessManufacture.EstablishInformation.GetAsync(id);
                    if (information.Id != default)
                    {
                        await ClearerEvent.EstablishInformationAsync(information);
                    }
                    else
                    {
                        var production = await BusinessManufacture.EstablishProduction.GetAsync(id);
                        if (production.Id != default)
                        {
                            await ClearerEvent.EstablishProductionAsync(production);
                        }
                        else
                        {
                            var parameter = await BusinessManufacture.EstablishParameter.GetAsync(id);
                            if (parameter.Id != default)
                            {
                                await ClearerEvent.EstablishParameterAsync(parameter);
                            }
                        }
                    }
                });
                RegisterTrigger.IsEstablish = default;
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

    [HttpGet("process-types", Name = nameof(ListWorkshopProcessType))]
    public IActionResult ListWorkshopProcessType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(ProcessType))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(ProcessType), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListWorkshopProcessType), Url, Response.Headers, results, new()
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

    }
    public readonly record struct Upper
    {
        public required Guid EquipmentId { get; init; }
        public required INetwork.Category NetworkType { get; init; }
        public required string NetworkTransl { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
    }
    public readonly record struct Lower
    {
        public required Guid Id { get; init; }
        public required ProcessType ProcessType { get; init; }
        public required string ProcessTransl { get; init; }
        public required string DataNo { get; init; }
        public required string DataPath { get; init; }
    }
    public readonly record struct Row
    {
        public required Guid Id { get; init; }
        public required Guid EquipmentId { get; init; }
        public required INetwork.Category NetworkType { get; init; }
        public required string NetworkTransl { get; init; }
        public required ProcessType ProcessType { get; init; }
        public required string ProcessTransl { get; init; }
        public required string FactoryNo { get; init; }
        public required string FactoryName { get; init; }
        public required string GroupNo { get; init; }
        public required string GroupName { get; init; }
        public required string SessionNo { get; init; }
        public required string SessionName { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required object? Mapper { get; init; }
        public required object? Detail { get; init; }
    }
    public sealed class ProcessInsert
    {
        public required Guid EquipmentId { get; init; }
        public required string Creator { get; init; }
        public required object? Part { get; init; }
        public required object? Mapper { get; init; }
        public required object? Detail { get; init; }
        public sealed class Validator : AbstractValidator<ProcessInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.EquipmentId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(EquipmentId)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class ProcessUpdate
    {
        public required Guid Id { get; init; }
        public required Guid EquipmentId { get; init; }
        public required string Creator { get; init; }
        public required object? Part { get; init; }
        public required object? Mapper { get; init; }
        public required object? Detail { get; init; }
        public sealed class Validator : AbstractValidator<ProcessUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.EquipmentId).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(EquipmentId)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public readonly record struct ParameterPart
    {
        public required string DataNo { get; init; }
    }
    public readonly record struct ExecutionOpcUa
    {
        public required string? NodePath { get; init; }
    }
    public readonly record struct ParameterCSV
    {
        public required string DataNo { get; init; }
        public required string NodeId { get; init; }
    }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}