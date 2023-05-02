namespace IIoT.Station.Apis.Edifices.Peripheries;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Homes : ControllerBase
{
    [HttpGet("equipments", Name = nameof(ListEquipmentQuantity))]
    public async ValueTask<IActionResult> ListEquipmentQuantity([FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var equipmentsAsync = BusinessManufacture.Equipment.ListAsync();
                int equipmentStatusRunQuantity = default, equipmentStatusIdleQuantity = default,
                equipmentStatusErrorQuantity = default, equipmentStatusSettingsQuantity = default,
                equipmentStatusShutdownQuantity = default, equipmentStatusRepairQuantity = default,
                equipmentStatusMaintenanceQuantity = default, equipmentStatusHoldQuantity = default;
                foreach (var information in RegisterTrigger.ListInformation())
                {
                    switch (information.Value.status)
                    {
                        case IEquipment.Status.Run:
                            equipmentStatusRunQuantity++;
                            break;

                        case IEquipment.Status.Idle:
                            equipmentStatusIdleQuantity++;
                            break;

                        case IEquipment.Status.Error:
                            equipmentStatusErrorQuantity++;
                            break;

                        case IEquipment.Status.Setup:
                            equipmentStatusSettingsQuantity++;
                            break;

                        case IEquipment.Status.Shutdown:
                            equipmentStatusShutdownQuantity++;
                            break;

                        case IEquipment.Status.Repair:
                            equipmentStatusRepairQuantity++;
                            break;

                        case IEquipment.Status.Maintenance:
                            equipmentStatusMaintenanceQuantity++;
                            break;

                        case IEquipment.Status.Hold:
                            equipmentStatusHoldQuantity++;
                            break;
                    }
                }
                var equipments = await equipmentsAsync;
                return Ok(new
                {
                    equipmentTotal = equipments.ToArray().Length,
                    equipmentStatusRunQuantity,
                    equipmentStatusIdleQuantity,
                    equipmentStatusErrorQuantity,
                    equipmentStatusSettingsQuantity,
                    equipmentStatusShutdownQuantity,
                    equipmentStatusRepairQuantity,
                    equipmentStatusMaintenanceQuantity,
                    equipmentStatusHoldQuantity
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

    [HttpGet("transactions", Name = nameof(ListTransactionQuantityAsync))]
    public async ValueTask<IActionResult> ListTransactionQuantityAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<int> uploads = new();
                List<int> downloads = new();
                if (!string.IsNullOrEmpty(query.DateTimeGroup))
                {
                    foreach (var item in query.DateTimeGroup.Contains(',') ? query.DateTimeGroup.TrimEnd(',').Split(',') : new[]
                    {
                        query.DateTimeGroup
                    }) uploads.Add(await BusinessManufacture.PushHistory.GaugeAsync(item));
                }
                return Ok(new
                {
                    Upload = uploads,
                    Download = downloads
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
    public sealed class Query : Satchel
    {
        public required string DateTimeGroup { get; init; }
    }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
}