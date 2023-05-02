namespace IIoT.Station.Services.Architects;
internal sealed class AuthenticateHandler : AuthenticationHandler<AuthenticateOption>
{
    public AuthenticateHandler(IOptionsMonitor<AuthenticateOption> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) { }
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (Request.Headers.ContainsKey(Head.Authorization))
            {
                var result = string.Empty;
                string? header = Request?.Headers[Head.Authorization];
                if (string.IsNullOrEmpty(header)) throw new Exception();
                if (!header.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)) throw new Exception();
                {
                    result = header[JwtBearerDefaults.AuthenticationScheme.Length..].Trim();
                    if (string.IsNullOrEmpty(result)) throw new Exception();
                }
                foreach (var item in AuthenticateService.Validators.Where(item => item.Value.token == result))
                {
                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new GenericPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, item.Value.entity.Id.ToString())
                    }, result), roles: null), Scheme.Name)));
                }
            }
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        catch (Exception e)
        {
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }
    public required IAuthenticateService AuthenticateService { get; init; }
}