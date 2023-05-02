namespace IIoT.Station.Services.Architects;
public class ModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.Selectors.Any(item => item.AttributeRouteModel is not null)) return;
        if (controller.Selectors.Count is 0) controller.Selectors.Add(new());
        for (int i = default; i < controller.Selectors.Count; i++) controller.Selectors[i].AttributeRouteModel = new()
        {
            Template = AttributeRouteModel.CombineTemplates(controller.GetName(), controller.ControllerName)
        };
        for (int i = default; i < controller.Actions.Count; i++)
        {
            if (controller.Actions[i].Selectors.Any(item => item.AttributeRouteModel is not null)) continue;
            if (controller.Actions[i].Selectors.Count is 0) controller.Actions[i].Selectors.Add(new SelectorModel());
            for (int item = default; item < controller.Actions[i].Selectors.Count; item++) controller.Actions[i].Selectors[item].AttributeRouteModel = new()
            {
                Template = controller.Actions[i].ActionName
            };
        }
    }
}