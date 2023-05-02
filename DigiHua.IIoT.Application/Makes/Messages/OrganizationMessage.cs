namespace IIoT.Application.Makes.Messages;
internal sealed class OrganizationMessage : IEntranceTrigger<AthenaMedium.Organization, JObject>
{
    public ValueTask<JObject> PushAsync(AthenaMedium.Organization entity) => ValueTask.FromResult(new JObject()
    {
        {
            IManufactureClient.Label.StandardData, new JObject()
            {
                {
                    AthenaMedium.Execution, JObject.FromObject(new AthenaMedium.Result()
                    {
                        Code = ((int)default).ToString(),
                        SqlCode = string.Empty,
                        Description = string.Empty
                    })
                },
                {
                    AthenaMedium.Parameter, JObject.FromObject(entity)
                }
            }
        }
    });
}