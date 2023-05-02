namespace IIoT.Application.Makes.Messages;
public sealed class PlatformerMessage : IPlatformerService
{
    public async Task<string> BuildAsync(string text)
    {
        try
        {
            var request = FoundationTrigger.UseDeserializeXml<StandardRequest>(text);
            ArgumentNullException.ThrowIfNull(request?.Service, nameof(request.Service));
            using (CultureHelper.Use(request.Service.Language ?? string.Empty))
            {
                switch (request.Service.Name)
                {
                    case "workshops_produces_metadatas":
                        {
                            List<StandardEquipment> equipments = new();
                            for (int i1 = default; i1 < request.Payload.Equipments.Count; i1++)
                            {
                                var seqNo = 1;
                                List<StandardRow> rows = new();
                                for (int i2 = default; i2 < request.Payload.Equipments[i1].Rows.Count; i2++)
                                {
                                    List<StandardField> fields = new();
                                    for (int i3 = default; i3 < request.Payload.Equipments[i1].Rows[i2].Fields.Count; i3++)
                                    {
                                        switch (request.Payload.Equipments[i1].Rows[i2].Fields[i3].Name)
                                        {
                                            case var item when item is "data_no":
                                                var (networkId, groupId, equipmentId) = RegisterTrigger.GetEquipment(request.Payload.Equipments[i1].Name);
                                                if (equipmentId != default)
                                                {
                                                    var (establishId, datas) = RegisterTrigger.GetDashboardData(equipmentId);
                                                    foreach (var establish in await BusinessManufacture.ProcessEstablish.ListEquipmentAsync(equipmentId))
                                                    {
                                                        switch (establish.ProcessType)
                                                        {
                                                            case IProcessEstablish.ProcessType.EquipmentParameter:
                                                                var parameters = await BusinessManufacture.EstablishParameter.ListEstablishAsync(establish.Id);
                                                                var parameter = parameters.FirstOrDefault(item => item.DataNo == request.Payload.Equipments[i1].Rows[i2].Fields[i3].Text);
                                                                if (parameter.Id != default)
                                                                {
                                                                    var (processId, dataNo) = datas.Find(item => item.processId == parameter.Id);
                                                                    if (processId != default)
                                                                    {
                                                                        var (dataValue, eventTime) = RegisterTrigger.GetParameter(processId);
                                                                        fields.Add(new()
                                                                        {
                                                                            Name = "data_no",
                                                                            Type = "string",
                                                                            Text = request.Payload.Equipments[i1].Rows[i2].Fields[i3].Text
                                                                        });
                                                                        fields.Add(new()
                                                                        {
                                                                            Name = "data_value",
                                                                            Type = "double",
                                                                            Text = dataValue.ToString()
                                                                        });
                                                                        fields.Add(new()
                                                                        {
                                                                            Name = "event_time",
                                                                            Type = "datetime",
                                                                            Text = eventTime.ToString(Converter.EaiSeconds)
                                                                        });
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    rows.Add(new()
                                    {
                                        Seq = seqNo++.ToString(),
                                        Fields = fields
                                    });
                                }
                                equipments.Add(new()
                                {
                                    Name = request.Payload.Equipments[i1].Name,
                                    Rows = rows
                                });
                            }
                            return FoundationTrigger.UseSerializerXml(new StandardResponse
                            {
                                Execution = new()
                                {
                                    Status = new()
                                    {
                                        Code = Microsoft.AspNetCore.Http.StatusCodes.Status200OK.ToString(),
                                        Description = string.Empty
                                    }
                                },
                                Payload = new()
                                {
                                    Equipments = equipments
                                }
                            });
                        }

                    default:
                        throw new Exception(Fielder["wrong.service.name"]);
                }
            }
        }
        catch (Exception e)
        {
            return FoundationTrigger.UseSerializerXml(new StandardResponse
            {
                Execution = new()
                {
                    Status = new()
                    {
                        Code = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound.ToString(),
                        Description = e.Message
                    }
                },
                Payload = default
            });
        }
    }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}