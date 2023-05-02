namespace IIoT.Retinue;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpAspNetCoreModule), typeof(IIoTApplicationModule))]
internal sealed class AppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        LogEventLevel.Error.UseRecord(RollingInterval.Day, Identification, 10);
        context.Services.AddHostedService<WatchmanGuard>();
        context.Services.AddControllers(item =>
        {
            item.ReturnHttpNotAcceptable = true;
        }).ConfigureApiBehaviorOptions(item =>
        {
            item.SuppressModelStateInvalidFilter = default;
        }).AddControllersAsServices();
        context.Services.Configure<ApiBehaviorOptions>(item =>
        {
            item.SuppressModelStateInvalidFilter = true;
            item.InvalidModelStateResponseFactory = (action) =>
            {
                List<string> details = new();
                var rootChildren = action.ModelState.Root.Children;
                {
                    if (rootChildren is not null)
                    {
                        foreach (var children in rootChildren)
                        {
                            var errors = children.Errors;
                            if (errors is not null)
                            {
                                foreach (var childrenError in errors)
                                {
                                    details.Add(childrenError.ErrorMessage);
                                }
                            }
                        }
                    }
                }
                return new BadRequestObjectResult(new ValidationProblemDetails()
                {
                    Type = "Detail",
                    Title = "TextData Fields Validator",
                    Detail = string.Join(",\u00A0", details),
                    Status = StatusCodes.Status400BadRequest,
                    Instance = action.ActionDescriptor.AttributeRouteInfo?.Template
                });
            };
        });
    }
}