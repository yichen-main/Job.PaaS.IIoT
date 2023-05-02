using static IIoT.Domain.Shared.Businesses.Workshops.Processes.IProcessEstablish;

namespace IIoT.Domain.Businesses.Workshops.Processes;
internal sealed class ProcessEstablish : TacticExpert, IProcessEstablish
{
    readonly INpgsqlUtility _npgsqlUtility;
    public ProcessEstablish(INpgsqlUtility npgsqlUtility) => _npgsqlUtility = npgsqlUtility;
    public async ValueTask InstallAsync()
    {
        if (!await ExistTableAsync(TableName)) await ExecuteAsync(_npgsqlUtility.MarkTable<Entity>(new()
        {
            Foreigns = new[]
            {
                (nameof(Entity.EquipmentId).FieldInfo<Entity>().Name, TableName<IEquipment.Entity>(), CurrentSign)
            }
        }), default, enable: true);
    }
    public async ValueTask AddAsync(Entity entity, IEstablishInformation.Entity process) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity),
        (_npgsqlUtility.MarkInsert<IEstablishInformation.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process),
        (_npgsqlUtility.MarkInsert<IInformationStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IInformationStack.Entity
        {
            Id = entity.Id,
            Status = IEquipment.Status.Unused,
            CreateTime = default
        })
    });
    public async ValueTask AddAsync(Entity entity, IEstablishInformation.Entity process, IOpcUaProcess.Entity detail)
    {
        await TransactionAsync(new List<(string content, object? @object)>
        {
            (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity),
            (_npgsqlUtility.MarkInsert<IEstablishInformation.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process),
            (_npgsqlUtility.MarkInsert<IInformationStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IInformationStack.Entity
            {
                Id = entity.Id,
                Status = IEquipment.Status.Unused,
                CreateTime = default
            }), (_npgsqlUtility.MarkInsert<IOpcUaProcess.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), detail)
        });
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<IEstablishProduction.Entity> processes)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity)
        };
        foreach (var process in processes)
        {
            results.Add((_npgsqlUtility.MarkInsert<IEstablishProduction.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process));
            results.Add((_npgsqlUtility.MarkInsert<IProductionStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IProductionStack.Entity
            {
                Id = process.Id,
                Output = default,
                CreateTime = default
            }));
        }
        await TransactionAsync(results);
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<(IEstablishProduction.Entity process, IOpcUaProcess.Entity opcUa)> details)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity)
        };
        foreach (var (process, opcUa) in details)
        {
            results.Add((_npgsqlUtility.MarkInsert<IEstablishProduction.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process));
            results.Add((_npgsqlUtility.MarkInsert<IProductionStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IProductionStack.Entity
            {
                Id = process.Id,
                Output = default,
                CreateTime = default
            }));
            results.Add((_npgsqlUtility.MarkInsert<IOpcUaProcess.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), opcUa));
        }
        await TransactionAsync(results);
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<IEstablishParameter.Entity> processes)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity)
        };
        foreach (var process in processes)
        {
            results.Add((_npgsqlUtility.MarkInsert<IEstablishParameter.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process));
            results.Add((_npgsqlUtility.MarkInsert<IParameterStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IParameterStack.Entity
            {
                Id = process.Id,
                DataValue = default,
                CreateTime = default
            }));
        }
        await TransactionAsync(results);
    }
    public async ValueTask AddAsync(Entity entity, IEnumerable<(IEstablishParameter.Entity process, IOpcUaProcess.Entity opcUa)> details)
    {
        List<(string content, object? @object)> results = new()
        {
            (_npgsqlUtility.MarkInsert<Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), entity)
        };
        foreach (var (process, opcUa) in details)
        {
            results.Add((_npgsqlUtility.MarkInsert<IEstablishParameter.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), process));
            results.Add((_npgsqlUtility.MarkInsert<IParameterStack.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), new IParameterStack.Entity
            {
                Id = process.Id,
                DataValue = default,
                CreateTime = default
            }));
            results.Add((_npgsqlUtility.MarkInsert<IOpcUaProcess.Entity>().AddUpsert(nameof(Entity.Id).To<Entity>()), opcUa));
        }
        await TransactionAsync(results);
    }
    public async ValueTask UpdateAsync(Entity entity, IEstablishInformation.Entity process) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<IEstablishInformation.Entity>(new[]
        {
            nameof(IEstablishInformation.Entity.Run),
            nameof(IEstablishInformation.Entity.Idle),
            nameof(IEstablishInformation.Entity.Error),
            nameof(IEstablishInformation.Entity.Setup),
            nameof(IEstablishInformation.Entity.Shutdown),
            nameof(IEstablishInformation.Entity.Repair),
            nameof(IEstablishInformation.Entity.Maintenance),
            nameof(IEstablishInformation.Entity.Hold)
        }), process)
    });
    public async ValueTask UpdateAsync(Entity entity, IEstablishInformation.Entity process, IOpcUaProcess.Entity detail)
    {
        await TransactionAsync(new List<(string content, object? @object)>
        {
            (_npgsqlUtility.MarkUpdate<Entity>(new[]
            {
                nameof(Entity.Creator),
                nameof(Entity.CreateTime)
            }), entity), (_npgsqlUtility.MarkUpdate<IEstablishInformation.Entity>(new[]
            {
                nameof(IEstablishInformation.Entity.Run),
                nameof(IEstablishInformation.Entity.Idle),
                nameof(IEstablishInformation.Entity.Error),
                nameof(IEstablishInformation.Entity.Setup),
                nameof(IEstablishInformation.Entity.Shutdown),
                nameof(IEstablishInformation.Entity.Repair),
                nameof(IEstablishInformation.Entity.Maintenance),
                nameof(IEstablishInformation.Entity.Hold)
            }), process), (_npgsqlUtility.MarkUpdate<IOpcUaProcess.Entity>(new[]
            {
                nameof(IOpcUaProcess.Entity.NodePath)
            }), detail)
        });
    }
    public async ValueTask UpdateAsync(Entity entity, IEstablishProduction.Entity process) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),(_npgsqlUtility.MarkUpdate<IEstablishProduction.Entity>(new[]
        {
            nameof(IEstablishProduction.Entity.EstablishId),
            nameof(IEstablishProduction.Entity.DispatchNo),
            nameof(IEstablishProduction.Entity.BatchNo)
        }), process)
    });
    public async ValueTask UpdateAsync(Entity entity, IEstablishProduction.Entity process, IOpcUaProcess.Entity opcUa) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity),(_npgsqlUtility.MarkUpdate<IEstablishProduction.Entity>(new[]
        {
            nameof(IEstablishProduction.Entity.EstablishId),
            nameof(IEstablishProduction.Entity.DispatchNo),
            nameof(IEstablishProduction.Entity.BatchNo)
        }), process), (_npgsqlUtility.MarkUpdate<IOpcUaProcess.Entity>(new[]
        {
            nameof(IOpcUaProcess.Entity.NodePath)
        }), opcUa)
    });
    public async ValueTask UpdateAsync(Entity entity, IEstablishParameter.Entity process) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<IEstablishParameter.Entity>(new[]
        {
            nameof(IEstablishParameter.Entity.EstablishId),
        }), process)
    });
    public async ValueTask UpdateAsync(Entity entity, IEstablishParameter.Entity process, IOpcUaProcess.Entity opcUa) => await TransactionAsync(new List<(string content, object? @object)>
    {
        (_npgsqlUtility.MarkUpdate<Entity>(new[]
        {
            nameof(Entity.Creator),
            nameof(Entity.CreateTime)
        }), entity), (_npgsqlUtility.MarkUpdate<IEstablishParameter.Entity>(new[]
        {
            nameof(IEstablishParameter.Entity.EstablishId)
        }), process), (_npgsqlUtility.MarkUpdate<IOpcUaProcess.Entity>(new[]
        {
            nameof(IOpcUaProcess.Entity.NodePath)
        }), opcUa)
    });
    public Task<Entity> GetAsync(Guid id) => SingleQueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ProcessType),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.Id).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public async Task<WorkshopData> GetWorkshopDataAsync(Guid establishId)
    {
        if (Morse.Passer && ConnectionString != string.Empty)
        {
            await using NpgsqlConnection npgsql = new(ConnectionString);
            await npgsql.OpenAsync();
            using var result = await npgsql.QueryMultipleAsync(new[]
            {
                _npgsqlUtility.MarkQuery<Entity>(new[]
                {
                    nameof(Entity.Id),
                    nameof(Entity.EquipmentId),
                    nameof(Entity.ProcessType),
                    nameof(Entity.Creator),
                    nameof(Entity.CreateTime)
                }).AddEqualFilter(nameof(Entity.Id).To<Entity>(), establishId),
                _npgsqlUtility.MarkQuery<IEstablishInformation.Entity>(new[]
                {
                    nameof(IEstablishInformation.Entity.Id),
                    nameof(IEstablishInformation.Entity.Run),
                    nameof(IEstablishInformation.Entity.Idle),
                    nameof(IEstablishInformation.Entity.Error),
                    nameof(IEstablishInformation.Entity.Setup),
                    nameof(IEstablishInformation.Entity.Shutdown),
                    nameof(IEstablishInformation.Entity.Repair),
                    nameof(IEstablishInformation.Entity.Maintenance),
                    nameof(IEstablishInformation.Entity.Hold)
                }).AddEqualFilter(nameof(IEstablishInformation.Entity.Id).To<IEstablishInformation.Entity>(), establishId),
                _npgsqlUtility.MarkQuery<IEstablishProduction.Entity>(new[]
                {
                    nameof(IEstablishProduction.Entity.Id),
                    nameof(IEstablishProduction.Entity.EstablishId),
                    nameof(IEstablishProduction.Entity.DispatchNo),
                    nameof(IEstablishProduction.Entity.BatchNo)
                }).AddEqualFilter(nameof(IEstablishProduction.Entity.EstablishId).To<IEstablishProduction.Entity>(), establishId),
                _npgsqlUtility.MarkQuery<IEstablishParameter.Entity>(new[]
                {
                    nameof(IEstablishParameter.Entity.Id),
                    nameof(IEstablishParameter.Entity.EstablishId),
                    nameof(IEstablishParameter.Entity.DataNo)
                }).AddEqualFilter(nameof(IEstablishParameter.Entity.EstablishId).To<IEstablishParameter.Entity>(), establishId)
            }.DelimitMark(Delimiter.Finish));
            return new()
            {
                Entity = await result.ReadSingleOrDefaultAsync<Entity>(),
                Information = await result.ReadSingleOrDefaultAsync<IEstablishInformation.Entity>(),
                Productions = await result.ReadAsync<IEstablishProduction.Entity>(),
                Parameters = await result.ReadAsync<IEstablishParameter.Entity>()
            };
        }
        return default;
    }
    public Task<IEnumerable<Entity>> ListAsync() => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ProcessType),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).ToString(), default, Morse.Passer);
    public Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id) => QueryAsync<Entity>(_npgsqlUtility.MarkQuery<Entity>(new[]
    {
        nameof(Entity.Id),
        nameof(Entity.EquipmentId),
        nameof(Entity.ProcessType),
        nameof(Entity.Creator),
        nameof(Entity.CreateTime)
    }).AddObjectFilter(nameof(Entity.EquipmentId).To<Entity>(), nameof(id)), new
    {
        id
    }, Morse.Passer);
    public string TableName { get; init; } = TableName<Entity>();
}