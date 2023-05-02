namespace IIoT.Retinue.Services;
internal sealed class PlatformService //: ITacticContract
{
    //public async Task<string> BuildAsync(string requestXML)
    //{
    //    //try
    //    //{
    //    //    var equipmentsAsync = BusinessManufacture.Equipment.ListAsync();
    //    //    //var request = requestXML.ToDeserializeXml<EaiserviceElement.Request>() as EaiserviceElement.Request;
    //    //    //var content = request?.Payload?.Param?.DataRequest?.Datainfo?.DataParameter?.Data;
    //    //    //{
    //    //    //    ArgumentNullException.ThrowIfNull(request, "text format error");
    //    //    //    ArgumentNullException.ThrowIfNull(content, "content is empty");
    //    //    //}
    //    //    //using (CultureHelper.Use(request.Host?.Lang ?? string.Empty))
    //    //    //{
    //    //    //    var equipments = await equipmentsAsync;
    //    //    //    //if (equipments.Any()) Result = request.Service?.Name switch
    //    //    //    //{
    //    //    //    //    var item when item is ManufactureSturdy.ParameterExcavation => await ManufactureMessageWrapper!.DownParameter.PushAsync(content),
    //    //    //    //    var item when item is ManufactureSturdy.FormulaExcavation => await ManufactureMessageWrapper!.DownFormula.PushAsync((equipments, content)),
    //    //    //    //    var item when item is ManufactureSturdy.EquipmentInformation => await ManufactureMessageWrapper!.UpperEquipment.PushAsync((equipments, content)),
    //    //    //    //    var item when item is ManufactureSturdy.ProductionInformation => await ManufactureMessageWrapper!.UpperProduction.PushAsync((equipments, content)),
    //    //    //    //    _ => new()
    //    //    //    //    {
    //    //    //    //        Execution = new()
    //    //    //    //        {
    //    //    //    //            Status = new()
    //    //    //    //            {
    //    //    //    //                Description = ConstructLocalizer["wrong.service.name", request.Service?.Name ?? string.Empty]
    //    //    //    //            }
    //    //    //    //        }
    //    //    //    //    }
    //    //    //    //};
    //    //    //}
    //    //}
    //    //catch (Exception e)
    //    //{
    //    //    Result.Execution.Status.Description = e.Message switch
    //    //    {
    //    //        var item when item.Contains("Cannot access a closed pipe.") => string.Empty,
    //    //        _ => e.Message
    //    //    };
    //    //}
    //    //return new WorkshopComponent.MessageStandard()
    //    //{
    //    //    Payload = new()
    //    //    {
    //    //        Param = new()
    //    //        {
    //    //            DataResponse = Result
    //    //        }
    //    //    }
    //    //}.ToSerializerXml();
    //}
    //WorkshopComponent.MessageDataResponse Result { get; set; } = new();
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    //public required IMakeMessageWrapper MakeMessage { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}