using static IIoT.Application.Contracts.Architects.Events.IExecutorEvent;
using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Runners.Events;

[Dependency(ServiceLifetime.Singleton)]
public sealed class ExecutorEvent : IExecutorEvent
{
    public async Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<InformationMission> missions)
    {
        List<IInformationStack.Entity> histories = new();
        List<IPushHistory.InformationRecord> records = new();
        foreach (var mission in missions)
        {
            var (status, eventTime) = RegisterTrigger.GetInformation(mission.Stack.Id);
            if (eventTime != default && mission.Stack.Status != status)
            {
                records.Add(new()
                {
                    EquipmentNo = mission.Equipment.EquipmentNo,
                    EquipmentName = mission.Equipment.EquipmentName,
                    Status = status,
                    EventTime = eventTime
                });
                histories.Add(new()
                {
                    Id = mission.Stack.Id,
                    Status = status,
                    CreateTime = eventTime
                });
            }
        }
        List<EaiRequest.ParameterRow> rows = new();
        for (int i = default; i < records.Count; i++) rows.Add(new()
        {
            Seq = (Array.IndexOf(records.ToArray(), records[i]) + Mark.Found).ToString(),
            Fields = new()
            {
                new()
                {
                    Name = ProcessEquipment.EquipmentNo,
                    Type = nameof(String),
                    Text = records[i].EquipmentNo
                },
                new()
                {
                    Name = ProcessEquipment.EquipmentStatus,
                    Type = nameof(String),
                    Text = RegisterTrigger.AsConverter(records[i].Status)
                },
                new()
                {
                    Name = ProcessEquipment.ReportDateTime,
                    Type = nameof(DateTime),
                    Text = records[i].EventTime.ToString(Converter.EaiSeconds)
                }
            }
        });
        if (rows.Any())
        {
            var outcome = await SendAsync(FoundationTrigger.UseSerializerXml(new EaiRequest
            {
                Host = new()
                {
                    Prod = IManufactureClient.Label.ProdName
                },
                Payload = new()
                {
                    Param = new()
                    {
                        Key = IManufactureClient.Label.StandardData,
                        Type = IManufactureClient.Label.TextType,
                        DataRequest = new()
                        {
                            Datainfo = new()
                            {
                                DataParameter = new()
                                {
                                    Key = ProcessEquipment.ParameterKey,
                                    Type = IManufactureClient.Label.ParamType,
                                    Data = new()
                                    {
                                        Name = ProcessEquipment.ParameterDataName,
                                        ParameterRows = rows
                                    }
                                }
                            }
                        }
                    }
                },
                Service = new()
                {
                    Name = IWorkshopRawdata.EaiType.Information.GetDesc()
                }
            }), environment, IWorkshopRawdata.EaiType.Information);
            if (outcome.ConsumeMS != default && outcome.Detail == string.Empty) await BusinessManufacture.PushHistory.AddAsync(new()
            {
                Id = Guid.NewGuid(),
                EnvironmentType = environment,
                EaiType = IWorkshopRawdata.EaiType.Information,
                ContentRecord = records.ToJson(),
                ResultRecord = outcome.Message,
                ConsumeMS = outcome.ConsumeMS,
                CreateTime = DateTime.UtcNow
            }, histories);
            EaistagePromoter.OnLatest(new IEaistagePromoter.HostEventArgs
            {
                Url = outcome.Endpoint,
                Message = outcome.Detail,
                Environment = environment
            });
        }
    }
    public async Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<ProductionMission> missions)
    {
        List<IProductionStack.Entity> stacks = new();
        List<IPushHistory.ProductionRecord> records = new();
        foreach (var mission in missions)
        {
            List<IPushHistory.ProductionRecord.Content> contents = new();
            var (establishId, orders) = RegisterTrigger.GetEquipmentOrder(mission.Equipment.Id);
            foreach (var (process, stack) in mission.Details)
            {
                var (processId, dispatchNo, batchNo) = orders.Find(item => item.processId == process.Id);
                if (processId != default)
                {
                    var (output, eventTime) = RegisterTrigger.GetProduction(processId);
                    if (eventTime != default && output != stack.Output) contents.Add(new()
                    {
                        DispatchNo = dispatchNo,
                        BatchNo = batchNo,
                        Output = output,
                        EventTime = eventTime
                    });
                    stacks.Add(new()
                    {
                        Id = processId,
                        Output = output,
                        CreateTime = DateTime.UtcNow
                    });
                }
            }
            if (contents.Any()) records.Add(new()
            {
                EquipmentNo = mission.Equipment.EquipmentNo,
                EquipmentName = mission.Equipment.EquipmentName,
                Contents = contents
            });
        }
        List<EaiRequest.ParameterRow> rows = new();
        for (int i = default; i < records.Count; i++)
        {
            List<EaiRequest.DetailRow> details = new();
            foreach (var content in records[i].Contents) details.Add(new()
            {
                Seq = (Array.IndexOf(records.ToArray(), records) + Mark.Found).ToString(),
                Fields = new()
                {
                    new()
                    {
                        Name = ProcessProduction.PlotNo,
                        Type = nameof(String),
                        Text = content.DispatchNo
                    },
                    new()
                    {
                        Name = ProcessProduction.OpNo,
                        Type = nameof(String),
                        Text = content.BatchNo
                    },
                    new()
                    {
                        Name = ProcessProduction.ProdQty,
                        Type = nameof(Int32),
                        Text = content.Output.ToString()
                    }
                }
            });
            if (details.Any()) rows.Add(new()
            {
                Seq = (Array.IndexOf(records.ToArray(), records[i]) + Mark.Found).ToString(),
                Fields = new()
                {
                    new()
                    {
                        Name = ProcessProduction.EquipmentNo,
                        Type = nameof(String),
                        Text = records[i].EquipmentNo
                    },
                    new()
                    {
                        Name = ProcessProduction.AttributeNo,
                        Type = nameof(String),
                        Text = "counter"
                    },
                    new()
                    {
                        Name = ProcessProduction.AttributeValue,
                        Type = nameof(String),
                        Text = records[i].Contents.Sum(item => item.Output).ToString()
                    },
                    new()
                    {
                        Name = ProcessProduction.ReportDateTime,
                        Type = nameof(DateTime),
                        Text = DateTime.Now.ToString(Converter.EaiSeconds)
                    }
                },
                Detail = new()
                {
                    Name = ProcessProduction.DetailName,
                    DetailRows = details
                }
            });
        }
        if (rows.Any())
        {
            var outcome = await SendAsync(FoundationTrigger.UseSerializerXml(new EaiRequest
            {
                Host = new()
                {
                    Prod = IManufactureClient.Label.ProdName
                },
                Payload = new()
                {
                    Param = new()
                    {
                        Key = IManufactureClient.Label.StandardData,
                        Type = IManufactureClient.Label.TextType,
                        DataRequest = new()
                        {
                            Datainfo = new()
                            {
                                DataParameter = new()
                                {
                                    Key = ProcessProduction.ParameterKey,
                                    Type = IManufactureClient.Label.ParamType,
                                    Data = new()
                                    {
                                        Name = ProcessProduction.ParameterDataName,
                                        ParameterRows = rows
                                    }
                                }
                            }
                        }
                    }
                },
                Service = new()
                {
                    Name = IWorkshopRawdata.EaiType.Production.GetDesc()
                }
            }), environment, IWorkshopRawdata.EaiType.Production);
            if (outcome.ConsumeMS != default && outcome.Detail == string.Empty) await BusinessManufacture.PushHistory.AddAsync(new IPushHistory.Entity
            {
                Id = Guid.NewGuid(),
                EnvironmentType = environment,
                EaiType = IWorkshopRawdata.EaiType.Production,
                ContentRecord = records.ToJson(),
                ResultRecord = outcome.Message,
                ConsumeMS = outcome.ConsumeMS,
                CreateTime = DateTime.UtcNow
            }, stacks);
            EaistagePromoter.OnLatest(new IEaistagePromoter.HostEventArgs
            {
                Url = outcome.Endpoint,
                Message = outcome.Detail,
                Environment = environment
            });
        }
    }
    public async Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<ParameterMission> missions)
    {
        var key = 1;
        List<IParameterStack.Entity> stacks = new();
        List<IPushHistory.ParameterRecord> records = new();
        List<EaiRequest.ParameterRow> rows = new();
        foreach (var mission in missions)
        {
            List<IPushHistory.ParameterRecord.Content> contents = new();
            var (establishParameterId, datas) = RegisterTrigger.GetDashboardData(mission.Equipment.Id);
            foreach (var (process, stack) in mission.Details)
            {
                var (processId, dataNo) = datas.Find(item => item.processId == process.Id);
                if (processId != default)
                {
                    var (dataValue, eventTime) = RegisterTrigger.GetParameter(processId);
                    if (eventTime != default && dataValue != stack.DataValue)
                    {
                        rows.Add(new()
                        {
                            Seq = key++.ToString(),
                            Fields = new()
                            {
                                new()
                                {
                                    Name = ProcessParameter.EquipmentNo,
                                    Type = nameof(String),
                                    Text = mission.Equipment.EquipmentNo
                                },
                                new()
                                {
                                    Name = ProcessParameter.AttributeNo,
                                    Type = nameof(String),
                                    Text = dataNo
                                },
                                new()
                                {
                                    Name = ProcessParameter.AttributeValue,
                                    Type = nameof(String),
                                    Text = dataValue.ToString()
                                },
                                new()
                                {
                                    Name = ProcessParameter.ReportDateTime,
                                    Type = nameof(DateTime),
                                    Text = eventTime.ToString(Converter.EaiSeconds)
                                }
                            }
                        });
                        stacks.Add(new()
                        {
                            Id = stack.Id,
                            DataValue = dataValue,
                            CreateTime = eventTime
                        });
                        contents.Add(new()
                        {
                            Id = stack.Id,
                            DataNo = dataNo,
                            DataValue = dataValue,
                            EventTime = eventTime
                        });
                    }
                }
            }
            if (contents.Any()) records.Add(new()
            {
                EquipmentNo = mission.Equipment.EquipmentNo,
                EquipmentName = mission.Equipment.EquipmentName,
                Contents = contents
            });
        }
        if (rows.Any())
        {
            var outcome = await SendAsync(FoundationTrigger.UseSerializerXml(new EaiRequest
            {
                Host = new()
                {
                    Prod = IManufactureClient.Label.ProdName
                },
                Payload = new()
                {
                    Param = new()
                    {
                        Key = IManufactureClient.Label.StandardData,
                        Type = IManufactureClient.Label.TextType,
                        DataRequest = new()
                        {
                            Datainfo = new()
                            {
                                DataParameter = new()
                                {
                                    Key = ProcessParameter.ParameterKey,
                                    Type = IManufactureClient.Label.ParamType,
                                    Data = new()
                                    {
                                        Name = ProcessParameter.ParameterDataName,
                                        ParameterRows = rows
                                    }
                                }
                            }
                        }
                    }
                },
                Service = new()
                {
                    Name = IWorkshopRawdata.EaiType.Parameter.GetDesc()
                }
            }), environment, IWorkshopRawdata.EaiType.Parameter);
            if (outcome.ConsumeMS != default && outcome.Detail == string.Empty) await BusinessManufacture.PushHistory.AddAsync(new IPushHistory.Entity
            {
                Id = Guid.NewGuid(),
                EnvironmentType = environment,
                EaiType = IWorkshopRawdata.EaiType.Parameter,
                ContentRecord = records.ToJson(),
                ResultRecord = outcome.Message,
                ConsumeMS = outcome.ConsumeMS,
                CreateTime = DateTime.UtcNow
            }, stacks);
            EaistagePromoter.OnLatest(new IEaistagePromoter.HostEventArgs
            {
                Url = outcome.Endpoint,
                Message = outcome.Detail,
                Environment = environment
            });
        }
    }
    public async Task<IEnumerable<ProductionState>> SendAsync(string text, IMissionPush.EnvironmentType environment)
    {
        using ManufactureClient client = new(EndpointConfiguration.wsEAISoap, environment switch
        {
            IMissionPush.EnvironmentType.Production => Floor.FormalLocation,
            _ => Floor.ExperiLocation
        });
        client.InnerChannel.OperationTimeout = UnifiedTimer;
        var invokeSrv = await client.LinkAsync(text);
        var response = FoundationTrigger.UseDeserializeXml<EaiResponse>(invokeSrv.Body.Result);
        await client.CloseAsync();
        var rows = response?.Payload?.Param?.DataResponse?.Datainfo?.Parameter?.Data?.Rows;
        List<ProductionState> states = new();
        if (rows is not null)
        {
            for (int i = default; i < rows.Count; i++)
            {
                if (rows[i].Field is not null)
                {
                    DateTime eventTime = default;
                    string machineNo = string.Empty, machineName = string.Empty, machineState = string.Empty, description = string.Empty;
                    for (int item = default; item < rows[i].Field!.Count; item++)
                    {
                        switch (rows[i].Field![item].Name)
                        {
                            case "machine_no":
                                machineNo = rows[i].Field![item].Text ?? machineNo;
                                break;

                            case "machine_name":
                                machineName = rows[i].Field![item].Text ?? machineName;
                                break;

                            case "machine_state":
                                machineState = rows[i].Field![item].Text ?? machineState;
                                break;

                            case "description":
                                description = rows[i].Field![item].Text ?? description;
                                break;

                            case "start_time":
                                if (rows[i].Field![item].Text is not null)
                                {
                                    eventTime = DateTime.ParseExact(rows[i].Field![item]!.Text!, Converter.EaiSeconds, provider: null);
                                }
                                break;
                        }
                    }
                    states.Add(new()
                    {
                        MachineNo = machineNo,
                        MachineName = machineName,
                        MachineStatus = machineState,
                        Description = description,
                        StartTime = eventTime.ToString(Converter.DefaultSeconds)
                    });
                    await BusinessManufacture.ProduceState.UpsertAsync(new IProduceState.Entity
                    {
                        EquipmentNo = machineNo,
                        EquipmentName = machineName,
                        EquipmentStatus = RegisterTrigger.ToEquipmentStatus(machineState),
                        EnvironmentType = environment,
                        Description = description,
                        CreateTime = eventTime
                    });
                }
            }
        }
        return states;
    }
    public async ValueTask<Outcome> SendAsync(string text, IMissionPush.EnvironmentType environment, IWorkshopRawdata.EaiType eaiType)
    {
        short consumeMS = default;
        string message = string.Empty, detail = string.Empty, endpoint = environment switch
        {
            IMissionPush.EnvironmentType.Production => Floor.FormalLocation,
            _ => Floor.ExperiLocation
        };
        try
        {
            using ManufactureClient client = new(EndpointConfiguration.wsEAISoap, endpoint);
            client.InnerChannel.OperationTimeout = UnifiedTimer;
            var watch = Stopwatch.GetTimestamp();
            var invokeSrv = await client.LinkAsync(text);
            var response = invokeSrv.Body.Result;
            var workshop = FoundationTrigger.UseDeserializeXml<EaiResponse>(response);
            if (workshop?.Payload?.Param is not null)
            {
                message = workshop?.Payload?.Param?.DataResponse?.Execution?.Status?.Description ?? "empty memory";
            }
            else
            {
                var particular = FoundationTrigger.UseDeserializeXml<ParticularResponse>(response);
                message = particular?.Payload?.Response?.Exception?.Mesmsg ?? "empty memory";
            }
            await client.CloseAsync();
            consumeMS = (short)Stopwatch.GetElapsedTime(watch).TotalMilliseconds;
            if (message != string.Empty || Floor.Tester) EaistagePromoter.OnLatest(new IEaistagePromoter.MessageEventArgs
            {
                Eendpoint = endpoint,
                EaiType = eaiType,
                Response = FoundationTrigger.UseFormatXml(response),
                Request = FoundationTrigger.UseFormatXml(text),
                ConsumeMS = consumeMS
            });
        }
        catch (Exception e)
        {
            detail = e.Message;
        }
        return new()
        {
            ConsumeMS = consumeMS,
            Endpoint = endpoint,
            Message = message,
            Detail = detail
        };
    }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IEaistagePromoter EaistagePromoter { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}