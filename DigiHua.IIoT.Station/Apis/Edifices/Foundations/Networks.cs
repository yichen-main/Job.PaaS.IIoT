using static IIoT.Domain.Shared.Businesses.Roots.Networks.INetwork;

namespace IIoT.Station.Apis.Edifices.Foundations;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Networks : ControllerBase
{
    [HttpGet(Name = nameof(UpperEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> UpperEquipmentNetworkAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entities = await BusinessManufacture.Network.ListAsync();
                var uppers = entities.Select(item => new Upper
                {
                    Id = item.Id,
                    NetworkType = item.CategoryType,
                    NetworkTransl = Terminology[item.CategoryType.ToString()],
                    SessionNo = item.SessionNo,
                    SessionName = item.SessionName,
                    Creator = item.Creator,
                    CreateTime = item.CreateTime
                });
                if (!string.IsNullOrEmpty(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.SessionNo,
                    item.SessionName
                }.Any(item => item.Contains(query.Search)));
                Pages<Upper> results = new(uppers.OrderByDescending(item => item.CreateTime), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperEquipmentNetworkAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(LowerEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> LowerEquipmentNetworkAsync(Guid id, [FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Lower> lowers = new();
                await foreach (var item in BusinessManufacture.Equipment.ListFactoryNetworkAsync(id)) lowers.Add(new()
                {
                    OperateType = Operate.Enable,
                    OperateTransl = Terminology[nameof(Operate.Enable)],
                    NetworkStatus = Terminology[nameof(Status.Unused)],
                    GroupNo = item.Group.GroupNo,
                    EquipmentNo = item.Entity.EquipmentNo,
                    EquipmentName = item.Entity.EquipmentName
                });
                Pages<Lower> results = new(lowers.OrderBy(item => item.GroupNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(LowerEquipmentNetworkAsync), Url, Response.Headers, results, new()
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

    [HttpGet($$"""{{{nameof(categoryType)}}:int}/{{{nameof(id)}}:{{nameof(Guid)}}}""", Name = nameof(GetEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> GetEquipmentNetworkAsync(int categoryType, Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                object result = new();
                switch (categoryType)
                {
                    case (int)Category.PassiveReception:
                        {
                            var entity = await BusinessManufacture.Network.GetAsync(id);
                            result = new Row
                            {
                                NetworkType = entity.CategoryType,
                                NetworkTransl = Terminology[entity.CategoryType.ToString()],
                                SessionNo = entity.SessionNo,
                                SessionName = entity.SessionName,
                                Detail = default
                            };
                        }
                        break;

                    case (int)Category.MessageQueuingTelemetryTransport:
                        {
                            var group = await BusinessManufacture.NetworkMqtt.GetAsync(id);
                            result = new Row
                            {
                                NetworkType = group.Network.CategoryType,
                                NetworkTransl = Terminology[group.Network.CategoryType.ToString()],
                                SessionNo = group.Network.SessionNo,
                                SessionName = group.Network.SessionName,
                                Detail = new Row.Mqtt
                                {
                                    CustomerType = group.Entity.CustomerType,
                                    CustomerTransl = Terminology[group.Entity.CustomerType.ToString()],
                                    Port = group.Entity.Port,
                                    Ip = group.Entity.Ip,
                                    Username = group.Entity.Username,
                                    Password = group.Entity.Password
                                }
                            };
                        }
                        break;

                    case (int)Category.OPCUnifiedArchitecture:
                        {
                            var group = await BusinessManufacture.NetworkOpcUa.GetAsync(id);
                            result = new Row
                            {
                                NetworkType = group.Network.CategoryType,
                                NetworkTransl = Terminology[group.Network.CategoryType.ToString()],
                                SessionNo = group.Network.SessionNo,
                                SessionName = group.Network.SessionName,
                                Detail = new Row.OpcUa
                                {
                                    Endpoint = group.Entity.Endpoint,
                                    Username = group.Entity.Username,
                                    Password = group.Entity.Password
                                }
                            };
                        }
                        break;

                    default:
                        throw new Exception($"[{categoryType}] entity EaiType wrong");
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

    [HttpPost($$"""{{{nameof(categoryType)}}:int}""", Name = nameof(InsertEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> InsertEquipmentNetworkAsync(int categoryType, [FromHeader] Header header, [FromBody] NetworkInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = RegisterTrigger.PutNetwork(Guid.NewGuid(), body.SessionNo);
                switch (categoryType)
                {
                    case (int)Category.PassiveReception:
                        {
                            await BusinessManufacture.Network.AddAsync(new()
                            {
                                Id = id,
                                CategoryType = Category.PassiveReception,
                                SessionNo = body.SessionNo,
                                SessionName = body.SessionName ?? string.Empty,
                                Creator = body.Creator,
                                CreateTime = DateTime.UtcNow
                            });
                        }
                        break;

                    case (int)Category.MessageQueuingTelemetryTransport:
                        {
                            if (body.Detail is not null)
                            {
                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionMqtt>();
                                ReduxService.CheckIpLocation(detail.Ip, nameof(detail.Ip));
                                await BusinessManufacture.Network.AddAsync(new()
                                {
                                    Id = id,
                                    CategoryType = Category.MessageQueuingTelemetryTransport,
                                    SessionNo = body.SessionNo,
                                    SessionName = body.SessionName ?? string.Empty,
                                    Creator = body.Creator,
                                    CreateTime = DateTime.UtcNow
                                }, new INetworkMqtt.Entity
                                {
                                    CustomerType = detail.CustomerType,
                                    Id = id,
                                    Ip = detail.Ip,
                                    Port = detail.Port,
                                    Username = detail.Username ?? string.Empty,
                                    Password = detail.Password ?? string.Empty
                                });
                            }
                        }
                        break;

                    case (int)Category.OPCUnifiedArchitecture:
                        {
                            if (body.Detail is not null)
                            {
                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                ReduxService.CheckOpcUaUrl(detail.Endpoint, nameof(detail.Endpoint));
                                await BusinessManufacture.Network.AddAsync(new()
                                {
                                    Id = id,
                                    CategoryType = Category.OPCUnifiedArchitecture,
                                    SessionNo = body.SessionNo,
                                    SessionName = body.SessionName ?? string.Empty,
                                    Creator = body.Creator,
                                    CreateTime = DateTime.UtcNow
                                }, new INetworkOpcUa.Entity
                                {
                                    Id = id,
                                    Endpoint = detail.Endpoint,
                                    Username = detail.Username ?? string.Empty,
                                    Password = detail.Password ?? string.Empty
                                });
                            }
                        }
                        break;

                    default:
                        throw new Exception($"[{categoryType}] entity EaiType wrong");
                }
                return CreatedAtRoute(nameof(GetEquipmentNetworkAsync), new
                {
                    categoryType,
                    id
                }, default);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<INetwork.Entity>(), nameof(Entity.SessionNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.SessionNo)]],
                        var item when item.Contains(INetworkMqtt.LinkAddress) => Fielder["db.mqtt.customer.ip.repeat.setting"],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<INetworkOpcUa.Entity>(), nameof(INetworkOpcUa.Entity.Endpoint).To<Entity>())) =>
                        Fielder["db.repeat.input.information", nameof(INetworkOpcUa.Entity.Endpoint)],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut($$"""{{{nameof(categoryType)}}:int}""", Name = nameof(UpdateEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> UpdateEquipmentNetworkAsync(int categoryType, [FromHeader] Header header, [FromBody] NetworkUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entityAsync = BusinessManufacture.Network.GetAsync(body.Id);
                switch (categoryType)
                {
                    case (int)Category.PassiveReception:
                        {
                            var entity = await entityAsync;
                            switch (entity.CategoryType)
                            {
                                case Category.PassiveReception:
                                    await BusinessManufacture.Network.UpdateAsync(new()
                                    {
                                        Id = body.Id,
                                        CategoryType = Category.PassiveReception,
                                        SessionNo = body.SessionNo,
                                        SessionName = body.SessionName ?? string.Empty,
                                        Creator = body.Creator,
                                        CreateTime = DateTime.UtcNow
                                    });
                                    break;

                                case Category.MessageQueuingTelemetryTransport:
                                    await BusinessManufacture.Network.UpdateAsync(new()
                                    {
                                        Id = body.Id,
                                        CategoryType = Category.PassiveReception,
                                        SessionNo = body.SessionNo,
                                        SessionName = body.SessionName ?? string.Empty,
                                        Creator = body.Creator,
                                        CreateTime = DateTime.UtcNow
                                    }, (body.Id, TableName<INetworkMqtt.Entity>()));
                                    break;

                                case Category.OPCUnifiedArchitecture:
                                    await BusinessManufacture.Network.UpdateAsync(new()
                                    {
                                        Id = body.Id,
                                        CategoryType = Category.PassiveReception,
                                        SessionNo = body.SessionNo,
                                        SessionName = body.SessionName ?? string.Empty,
                                        Creator = body.Creator,
                                        CreateTime = DateTime.UtcNow
                                    }, (body.Id, TableName<INetworkOpcUa.Entity>()));
                                    break;

                                default:
                                    throw new Exception($"[{entity.CategoryType}] field: entity EaiType wrong");
                            }
                        }
                        break;

                    case (int)Category.MessageQueuingTelemetryTransport:
                        {
                            if (body.Detail is not null)
                            {
                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionMqtt>();
                                ReduxService.CheckIpLocation(detail.Ip, nameof(detail.Ip));
                                var entity = await entityAsync;
                                {
                                    switch (entity.CategoryType)
                                    {
                                        case Category.PassiveReception:
                                            await BusinessManufacture.Network.UpdateChangeAsync(new()
                                            {
                                                Id = body.Id,
                                                CategoryType = Category.MessageQueuingTelemetryTransport,
                                                SessionNo = body.SessionNo,
                                                SessionName = body.SessionName ?? string.Empty,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, new INetworkMqtt.Entity
                                            {
                                                CustomerType = detail.CustomerType,
                                                Id = body.Id,
                                                Ip = detail.Ip,
                                                Port = detail.Port,
                                                Username = detail.Username ?? string.Empty,
                                                Password = detail.Password ?? string.Empty
                                            });
                                            break;

                                        case Category.MessageQueuingTelemetryTransport:
                                            await BusinessManufacture.Network.UpdateAsync(new()
                                            {
                                                Id = body.Id,
                                                CategoryType = Category.MessageQueuingTelemetryTransport,
                                                SessionNo = body.SessionNo,
                                                SessionName = body.SessionName ?? string.Empty,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, new INetworkMqtt.Entity
                                            {
                                                CustomerType = detail.CustomerType,
                                                Id = body.Id,
                                                Ip = detail.Ip,
                                                Port = detail.Port,
                                                Username = detail.Username ?? string.Empty,
                                                Password = detail.Password ?? string.Empty
                                            });
                                            break;

                                        case Category.OPCUnifiedArchitecture:
                                            await BusinessManufacture.Network.UpdateAsync(new()
                                            {
                                                Id = body.Id,
                                                CategoryType = Category.MessageQueuingTelemetryTransport,
                                                SessionNo = body.SessionNo,
                                                SessionName = body.SessionName ?? string.Empty,
                                                Creator = body.Creator,
                                                CreateTime = DateTime.UtcNow
                                            }, new INetworkMqtt.Entity
                                            {
                                                CustomerType = detail.CustomerType,
                                                Id = body.Id,
                                                Ip = detail.Ip,
                                                Port = detail.Port,
                                                Username = detail.Username ?? string.Empty,
                                                Password = detail.Password ?? string.Empty
                                            }, (body.Id, TableName<INetworkOpcUa.Entity>()));
                                            break;

                                        default:
                                            throw new Exception($"[{entity.CategoryType}] field: entity EaiType wrong");
                                    }
                                }
                            }
                        }
                        break;

                    case (int)Category.OPCUnifiedArchitecture:
                        {
                            if (body.Detail is not null)
                            {
                                var detail = JToken.FromObject(body.Detail).ToObject<ExecutionOpcUa>();
                                ReduxService.CheckOpcUaUrl(detail.Endpoint, nameof(detail.Endpoint));
                                var entity = await entityAsync;
                                switch (entity.CategoryType)
                                {
                                    case Category.PassiveReception:
                                        await BusinessManufacture.Network.UpdateChangeAsync(new()
                                        {
                                            Id = body.Id,
                                            CategoryType = Category.OPCUnifiedArchitecture,
                                            SessionNo = body.SessionNo,
                                            SessionName = body.SessionName ?? string.Empty,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new INetworkOpcUa.Entity
                                        {
                                            Id = body.Id,
                                            Endpoint = detail.Endpoint,
                                            Username = detail.Username ?? string.Empty,
                                            Password = detail.Password ?? string.Empty
                                        });
                                        break;

                                    case Category.MessageQueuingTelemetryTransport:
                                        await BusinessManufacture.Network.UpdateAsync(new()
                                        {
                                            Id = body.Id,
                                            CategoryType = Category.OPCUnifiedArchitecture,
                                            SessionNo = body.SessionNo,
                                            SessionName = body.SessionName ?? string.Empty,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new INetworkOpcUa.Entity
                                        {
                                            Id = body.Id,
                                            Endpoint = detail.Endpoint,
                                            Username = detail.Username ?? string.Empty,
                                            Password = detail.Password ?? string.Empty
                                        }, (body.Id, TableName<INetworkMqtt.Entity>()));
                                        break;

                                    case Category.OPCUnifiedArchitecture:
                                        await BusinessManufacture.Network.UpdateAsync(new()
                                        {
                                            Id = body.Id,
                                            CategoryType = Category.OPCUnifiedArchitecture,
                                            SessionNo = body.SessionNo,
                                            SessionName = body.SessionName ?? string.Empty,
                                            Creator = body.Creator,
                                            CreateTime = DateTime.UtcNow
                                        }, new INetworkOpcUa.Entity
                                        {
                                            Id = body.Id,
                                            Endpoint = detail.Endpoint,
                                            Username = detail.Username ?? string.Empty,
                                            Password = detail.Password ?? string.Empty
                                        });
                                        break;

                                    default:
                                        throw new Exception($"[{entity.CategoryType}] field: entity EaiType wrong");
                                }
                            }
                        }
                        break;

                    default:
                        throw new Exception($"[{categoryType}] entity EaiType wrong");
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.SessionNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.SessionNo)]],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.SessionNo).To<Entity>())) =>
                        Fielder["db.repeat.input.information", Search[nameof(Entity.SessionNo)]],
                        var item when item.Contains(INetworkMqtt.LinkAddress) => Fielder["db.mqtt.customer.ip.repeat.setting"],
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<INetworkOpcUa.Entity>(), nameof(INetworkOpcUa.Entity.Endpoint).To<Entity>())) =>
                        Fielder["db.repeat.input.information", nameof(INetworkOpcUa.Entity.Endpoint)],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteEquipmentNetworkAsync))]
    public async ValueTask<IActionResult> DeleteEquipmentNetworkAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                RegisterTrigger.IsNetwork = true;
                List<Entity> entities = new();
                foreach (var item in ReduxService.ConvertGuid(identifier))
                {
                    entities.Add(await BusinessManufacture.Network.GetAsync(item));
                }
                await ClearerEvent.NetworkAsync(entities);
                RegisterTrigger.IsNetwork = default;
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

    [HttpGet("item-types", Name = nameof(ListEquipmentNetworkType))]
    public IActionResult ListEquipmentNetworkType([FromHeader] Header header, [FromQuery] Query query)
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
                ReduxService.AddPage(nameof(ListEquipmentNetworkType), Url, Response.Headers, results, new()
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

    [HttpGet("mqtt-customer-types", Name = nameof(ListMqttCustomerType))]
    public IActionResult ListMqttCustomerType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(INetworkMqtt.Customer))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(INetworkMqtt.Customer), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListMqttCustomerType), Url, Response.Headers, results, new()
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
    public sealed class Query : Satchel { }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required Category NetworkType { get; init; }
        public required string NetworkTransl { get; init; }
        public required string SessionNo { get; init; }
        public required string SessionName { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Lower
    {
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string NetworkStatus { get; init; }
        public required string GroupNo { get; init; }
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
    }
    public readonly record struct Row
    {
        public required Category NetworkType { get; init; }
        public required string NetworkTransl { get; init; }
        public required string SessionNo { get; init; }
        public required string SessionName { get; init; }
        public required object? Detail { get; init; }
        public readonly record struct Mqtt
        {
            public required INetworkMqtt.Customer CustomerType { get; init; }
            public required string CustomerTransl { get; init; }
            public required string Ip { get; init; }
            public required int Port { get; init; }
            public required string Username { get; init; }
            public required string Password { get; init; }
        }
        public readonly record struct OpcUa
        {
            public required string Endpoint { get; init; }
            public required string Username { get; init; }
            public required string Password { get; init; }
        }
    }
    public sealed class NetworkInsert
    {
        public required string SessionNo { get; init; }
        public required string? SessionName { get; init; }
        public required string Creator { get; init; }
        public required object? Detail { get; init; }
        public sealed class Validator : AbstractValidator<NetworkInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.SessionNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(SessionNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(SessionNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class NetworkUpdate
    {
        public required Guid Id { get; init; }
        public required string SessionNo { get; init; }
        public required string? SessionName { get; init; }
        public required string Creator { get; init; }
        public required object? Detail { get; init; }
        public sealed class Validator : AbstractValidator<NetworkUpdate>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.Id).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Id)]);
                    RuleFor(item => item.SessionNo).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(SessionNo)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(SessionNo), ProhibitSign]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public readonly record struct ExecutionMqtt
    {
        public required INetworkMqtt.Customer CustomerType { get; init; }
        public required int Port { get; init; }
        public required string Ip { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
    public readonly record struct ExecutionOpcUa
    {
        public required string Endpoint { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
    public required IStringLocalizer<Search> Search { get; init; }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}