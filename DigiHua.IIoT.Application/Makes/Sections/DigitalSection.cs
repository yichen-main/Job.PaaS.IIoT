using static IIoT.Application.Contracts.Makes.Sections.IDigitalSection;
using EndpointConfiguration = Opc.Ua.EndpointConfiguration;
using StatusCodes = Opc.Ua.StatusCodes;

namespace IIoT.Application.Makes.Sections;
internal sealed class DigitalSection : IDigitalSection, IDisposable
{
    bool disposedValue;
    public async Task<Session?> OpenAsync(Guid sessionId, string sessionNo, INetworkOpcUa.Entity entity)
    {
        try
        {
            if (!Scavengers.Any(item => item == sessionId))
            {
                Clear(sessionId);
                var result = await CreateAsync(await BeginFunctionAsync());
                result.KeepAlive += new KeepAliveEventHandler((session, @event) =>
                {
                    if (@event.Status is not null && ServiceResult.IsNotGood(@event.Status)) Clear(sessionId);
                });
                return result;
            }
            Task<Session> CreateAsync(ApplicationConfiguration configuration)
            {
                var disableKey = entity.Username == string.Empty && entity.Password == string.Empty;
                var description = CoreClientUtils.SelectEndpoint(entity.Endpoint, useSecurity: false);
                var userIdentity = new UserIdentity(new AnonymousIdentityToken());
                return Session.Create(configuration: configuration, sessionName: sessionNo,
                updateBeforeConnect: default, checkDomain: default, sessionTimeout: 60 * 1000,
                endpoint: new(collection: default, description, EndpointConfiguration.Create(configuration)),
                identity: disableKey ? userIdentity : new(entity.Username, entity.Password),
                preferredLocales: new[] { Language });
            }
            async Task<ApplicationConfiguration> BeginFunctionAsync()
            {
                CertificateValidator certificate = new();
                certificate.CertificateValidation += (validator, @event) =>
                {
                    if (ServiceResult.IsGood(@event.Error)) @event.Accept = true;
                    else if (@event.Error.StatusCode.Code is StatusCodes.BadCertificateUntrusted) @event.Accept = true;
                    else
                    {
                        throw new Exception($"Certificate validation error： {@event.Error.Code}: {@event.Error.AdditionalInfo}");
                    }
                };
                await certificate.Update(new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024,
                });
                ApplicationConfiguration configuration = new()
                {
                    DisableHiResClock = true,
                    ApplicationType = ApplicationType.Client,
                    CertificateValidator = certificate,
                    ServerConfiguration = new()
                    {
                        MaxMessageQueueSize = 100000,
                        MaxSubscriptionCount = 100000,
                        MaxPublishRequestCount = 100000,
                        MaxNotificationQueueSize = 100000
                    },
                    SecurityConfiguration = new()
                    {
                        MinimumCertificateKeySize = 1024,
                        RejectSHA1SignedCertificates = false,
                        SuppressNonceValidationErrors = true,
                        AutoAcceptUntrustedCertificates = true,
                        ApplicationCertificate = new()
                        {
                            StoreType = CertificateStoreType.X509Store,
                            StorePath = "CurrentUser\\My"
                        },
                        TrustedIssuerCertificates = new()
                        {
                            StoreType = CertificateStoreType.X509Store,
                            StorePath = "CurrentUser\\Root"
                        },
                        TrustedPeerCertificates = new()
                        {
                            StoreType = CertificateStoreType.X509Store,
                            StorePath = "CurrentUser\\Root"
                        }
                    },
                    TransportQuotas = new()
                    {
                        MaxBufferSize = 65535,
                        MaxArrayLength = 65535,
                        MaxMessageSize = 419430400,
                        OperationTimeout = 6000000,
                        MaxStringLength = int.MaxValue,
                        MaxByteStringLength = int.MaxValue,
                        ChannelLifetime = Timeout.Infinite,
                        SecurityTokenLifetime = Timeout.Infinite
                    },
                    ClientConfiguration = new()
                    {
                        DefaultSessionTimeout = Timeout.Infinite,
                        MinSubscriptionLifetime = Timeout.Infinite
                    },
                    ApplicationName = nameof(Morse.DigiHua).Joint(nameof(IIoT))
                };
                await configuration.Validate(ApplicationType.Client);
                return configuration;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
    public Subscription AddLink(in Session entity, in IWorkshopRawdata.Title title) => new(entity.DefaultSubscription)
    {
        Priority = 100,
        PublishingEnabled = true,
        DisplayName = title.ToJson(),
        PublishingInterval = default,
        LifetimeCount = uint.MaxValue,
        KeepAliveCount = uint.MaxValue,
        MaxNotificationsPerPublish = uint.MaxValue
    };
    public Subscription AddItem(in Subscription entity, in IEnumerable<Formula> formulas)
    {
        var title = entity.DisplayName.ToObject<IWorkshopRawdata.Title>();
        foreach (var formula in formulas)
        {
            MonitoredItem monitoredItem = new()
            {
                AttributeId = Attributes.Value,
                SamplingInterval = 500,
                StartNodeId = new NodeId(formula.NodePath),
                DisplayName = formula.ToJson()
            };
            monitoredItem.Notification += async (item, @event) =>
            {
                try
                {
                    var formula = item.DisplayName.ToObject<Formula>();
                    foreach (var data in item.DequeueValues())
                    {
                        var metadata = Convert.ToString(data.Value);
                        if (metadata is not null)
                        {
                            switch (formula.EaiType)
                            {
                                case IWorkshopRawdata.EaiType.Information:
                                    {
                                        var status = RegisterTrigger.CacheData(formula.EquipmentId, formula.EstablishId, metadata, data.SourceTimestamp);
                                        if (status is not IEquipment.Status.Unused) await WorkshopRawdata.InsertAsync(title, status);
                                        else CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                                        {
                                            Title = nameof(DigitalExpert).Joint(nameof(AddItem)).Joint(nameof(IWorkshopRawdata.EaiType.Information)),
                                            Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}",
                                            Detail = data.WrappedValue.TypeInfo.BuiltInType.ToString(),
                                            Trace = metadata
                                        });
                                    }
                                    break;

                                case IWorkshopRawdata.EaiType.Production:
                                    {
                                        if (int.TryParse(metadata, out var value))
                                        {
                                            RegisterTrigger.CacheData(formula.EquipmentId, formula.EstablishId, formula.ProcessId, string.Empty, string.Empty, value, data.SourceTimestamp);
                                            await WorkshopRawdata.InsertAsync(title, new[]
                                            {
                                                new IWorkshopRawdata.Production.Meta
                                                {
                                                    DispatchNo = string.Empty,
                                                    BatchNo = string.Empty,
                                                    Output = value
                                                }
                                            });
                                        }
                                        else CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                                        {
                                            Title = nameof(DigitalExpert).Joint(nameof(AddItem)).Joint(nameof(IWorkshopRawdata.EaiType.Production)),
                                            Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}",
                                            Detail = data.WrappedValue.TypeInfo.BuiltInType.ToString(),
                                            Trace = metadata
                                        });
                                    }
                                    break;

                                case IWorkshopRawdata.EaiType.Parameter:
                                    {
                                        if (float.TryParse(metadata, out var value))
                                        {
                                            RegisterTrigger.CacheData(formula.EquipmentId, formula.EstablishId, formula.ProcessId, formula.DataNo, value, data.SourceTimestamp);
                                            await WorkshopRawdata.InsertAsync(title, new[]
                                            {
                                                new IWorkshopRawdata.Parameter.Meta
                                                {
                                                    DataNo = formula.DataNo,
                                                    DataValue = value
                                                }
                                            });
                                        }
                                        else await BusinessManufacture.ParameterFormula.AddAsync(new[]
                                        {
                                            new IParameterFormula.Entity
                                            {
                                                Id =  Guid.NewGuid(),
                                                EquipmentNo = title.EquipmentNo,
                                                DataNo = formula.DataNo,
                                                DataValue = metadata,
                                                CreateTime = data.SourceTimestamp
                                            }
                                        });
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    CollectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs
                    {
                        Title = nameof(DigitalExpert).Joint(nameof(AddItem)).Joint(nameof(monitoredItem.Notification)),
                        Burst = item.DisplayName.ToObject<Formula>().EaiType.ToString(),
                        Detail = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}",
                        Trace = e.Message
                    });
                }
            };
            entity.AddItem(monitoredItem);
        }
        return entity;
    }
    public async ValueTask BuildAsync(IEquipment.Entity entity, Subscription subscription, List<Formula> formulas, CancellationToken stoppingToken)
    {
        if (Mains.TryGetValue(entity.NetworkId, out var title))
        {
            title.Entity.AddSubscription(subscription);
            await subscription.CreateAsync(stoppingToken);
            if (Links.TryAdd(entity.Id, (entity.EquipmentNo, subscription)))
            {
                if (!Providers.TryAdd((entity.NetworkId, entity.Id), formulas)) Clear(entity.NetworkId);
            }
        }
    }
    public async ValueTask RemoveLinkAsync(Guid sessionId, Guid equipmentId)
    {
        Providers.Remove((sessionId, equipmentId), out _);
        if (Links.Remove(equipmentId, out var link))
        {
            await link.entity.DeleteAsync(silent: true);
            if (Mains.TryGetValue(sessionId, out var title)) await title.Entity.RemoveSubscriptionAsync(link.entity);
            link.entity.Dispose();
        }
    }
    public void RemoveItem(in Guid sessionId, in Guid equipmentId)
    {
        if (Providers.TryGetValue((sessionId, equipmentId), out var formulas)) formulas.Clear();
    }
    public void Clear(in Guid sessionId)
    {
        Scavengers.Add(sessionId);
        foreach (var provider in Providers)
        {
            if (provider.Key.sessionId == sessionId)
            {
                Providers.Remove((sessionId, provider.Key.equipmentId), out var _);
                if (Links.Remove(provider.Key.equipmentId, out var link)) link.entity.Dispose();
            }
        }
        if (Mains.Remove(sessionId, out var title))
        {
            title.Entity.Close();
            title.Entity.Dispose();
        }
        Scavengers.Remove(sessionId);
    }
    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            foreach (var main in Mains) Clear(main.Key);
            disposedValue = true;
        }
    }
    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    ~DigitalSection() => Dispose(disposing: false);
    public required List<Guid> Scavengers { get; init; } = new();
    public required ICollectPromoter CollectPromoter { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IWorkshopRawdata WorkshopRawdata { get; init; }
    public required IBusinessManufactureWrapper BusinessManufacture { get; init; }
    public required ConcurrentDictionary<Guid, Title> Mains { get; init; } = new();
    public required ConcurrentDictionary<Guid, (string equipmentNo, Subscription entity)> Links { get; init; } = new();
    public required ConcurrentDictionary<(Guid sessionId, Guid equipmentId), List<Formula>> Providers { get; init; } = new();
}