using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Station.Services.Architects;

[Dependency(ServiceLifetime.Singleton)]
internal sealed partial class ReduxService : IReduxService
{
    public ReduxService()
    {
        Transport = new MqttFactory().CreateMqttServer(new MqttServerOptionsBuilder().WithDefaultEndpoint().WithDefaultEndpointPort(RunnerText.Platform.Mqbroker).Build());
    }
    public async ValueTask InitialAsync()
    {
        await Runner.BuildAsync();
        Floor.Tester = RunnerText.Organization.Debug;
        Floor.SecretKey = RunnerText.Platform.Hash;
        Language = RunnerText.Organization.Language;
        Floor.CompanyName = RunnerText.Organization.CompanyName;
        await Manager.BuildAsync();
        Floor.ExperiLocation = ManagerText.Assembly.ExperiBlock;
        Floor.FormalLocation = ManagerText.Assembly.FormalBlock;
        Morse.Address = ManagerText.Hangar.Location;
        Morse.Username = ManagerText.Hangar.Identifier;
        Morse.Password = ManagerText.Hangar.Pespond;
        Morse.Flowmeter = Morse.Address.AddURL(int.Parse(ManagerText.Hangar.Flowmeter));
        ConnectionString = Morse.Address.Nameplate(int.Parse(ManagerText.Hangar.Merchant),
        ManagerText.Hangar.Plaque, Morse.Username, Morse.Password);
    }
    public async ValueTask AddCommercialAsync(IBusinessFoundationWrapper wrapper)
    {
        await wrapper.User.InstallAsync();
        await wrapper.UserVerification.InstallAsync();
    }
    public async ValueTask AddCommercialAsync(IBusinessManufactureWrapper wrapper)
    {
        await wrapper.Factory.InstallAsync();
        {
            await wrapper.FactoryGroup.InstallAsync();
        }
        await wrapper.Network.InstallAsync();
        {
            await wrapper.NetworkMqtt.InstallAsync();
            await wrapper.NetworkOpcUa.InstallAsync();
        }
        await wrapper.Equipment.InstallAsync();
        {
            await wrapper.EquipmentAlarm.InstallAsync();
            await wrapper.ProduceState.InstallAsync();
            await wrapper.OpcUaProcess.InstallAsync();
        }
        await wrapper.Mission.InstallAsync();
        {
            await wrapper.MissionPush.InstallAsync();
            await wrapper.PushHistory.InstallAsync();
        }
        await wrapper.ProcessEstablish.InstallAsync();
        {
            await wrapper.EstablishInformation.InstallAsync();
            await wrapper.InformationStack.InstallAsync();
            await wrapper.EstablishProduction.InstallAsync();
            await wrapper.ProductionStack.InstallAsync();
            await wrapper.EstablishParameter.InstallAsync();
            await wrapper.ParameterStack.InstallAsync();
            await wrapper.ParameterFormula.InstallAsync();
        }
    }
    public async Task<IUser.Entity> UserInfoAsync(ClaimsPrincipal principal)
    {
        var id = Guid.Parse(principal.Identity!.Name!);
        if (id != default) return await BusinessFoundation.User.GetAsync(id);
        return default;
    }
    public string MakeUniqueIndexTag(in string table, in string field) => $"{table}_{field}_key";
    public string MakeForeignKeyIndexTag(in string table, in string field) => $"{table}_{field}_fkey";
    public IEnumerable<Guid> ConvertGuid(in string text, in char symbol = ',')
    {
        List<Guid> results = new();
        Array.ForEach(text.Contains(symbol, StringComparison.OrdinalIgnoreCase) ? text.TrimEnd(symbol).Split(symbol) : new[]
        {
            text
        }, tag =>
        {
            if (Guid.TryParse(tag, out Guid id)) results.Add(id);
        });
        return results;
    }
    public void CheckProhibitSign(in string text, in string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new Exception(Fielder["field.cannot.be.empty", fieldName]);
        if (text.Contains(ProhibitSign)) throw new Exception(Fielder["field.with.prohibition.sign", fieldName, ProhibitSign]);
    }
    public void CheckIpLocation(in string ip, in string fieldName)
    {
        if (string.IsNullOrWhiteSpace(ip)) throw new Exception(Fielder["field.cannot.be.empty", fieldName]);
        if (!IpRegex().IsMatch(ip)) throw new Exception(Fielder["field.invalid.ip.location", fieldName]);
    }
    public void CheckHttpUrl(in string url, in string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new Exception(Fielder["field.cannot.be.empty", fieldName]);
        if (!(Uri.TryCreate(url, UriKind.Absolute, out Uri? result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps)))
        {
            throw new Exception(Fielder["field.invalid.url.location", fieldName]);
        }
    }
    public void CheckOpcUaUrl(in string url, in string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new Exception(Fielder["field.cannot.be.empty", fieldName]);
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? result) || result.Scheme is not INetworkOpcUa.Scheme)
        {
            throw new Exception(Fielder["field.invalid.url.location", fieldName]);
        }
    }
    public void AddPage<T>(in string name, in IUrlHelper helper, in IHeaderDictionary dictionaries, in Pages<T> pages, in Mark mark)
    {
        dictionaries.Add(Head.Pagination, new
        {
            pages.CurrentPage,
            pages.PageSize,
            pages.TotalCount,
            totalPages = pages.TotalPage,
            firstLink = helper.Link(name, mark.FirstPage),
            lastLink = helper.Link(name, mark.LastPage),
            previousLink = pages.CurrentPage > Mark.Found ? helper.Link(name, mark.PreviousPage) : helper.Link(name, mark.FirstPage),
            nextLink = pages.CurrentPage < pages.TotalPage ? helper.Link(name, mark.NextPage) : helper.Link(name, mark.LastPage)
        }.ToJson());
    }
    public int DownPage(in int count) => count + Mark.Found;
    public int UpperPage(in int count) => count is Mark.Found ? Mark.Found : count - Mark.Found;
    [GeneratedRegex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$")] public static partial Regex IpRegex();
    public required MqttServer Transport { get; init; }
    public required IRunnerProfile Runner { get; init; }
    public required IManagerProfile Manager { get; init; }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IBusinessFoundationWrapper BusinessFoundation { get; init; }
}