using StatusCodes = Opc.Ua.StatusCodes;

namespace IIoT.Domain.Functions.Experts;
public abstract class DigitalExpert : IDigitalExpert, IDisposable
{
    readonly IRegisterTrigger _registerTrigger;
    readonly ICollectPromoter _collectPromoter;
    readonly IWorkshopRawdata _workshopRawdata;
    protected DigitalExpert(IRegisterTrigger registerTrigger, ICollectPromoter collectPromoter, IWorkshopRawdata workshopRawdata)
    {
        _registerTrigger = registerTrigger;
        _collectPromoter = collectPromoter;
        _workshopRawdata = workshopRawdata;
    }
    public Subscription? AddSubscription(in IWorkshopRawdata.Title title)
    {
        if (Session is not null) return new(Session.DefaultSubscription)
        {
            Priority = 100,
            PublishingEnabled = true,
            DisplayName = title.ToJson(),
            PublishingInterval = default,
            LifetimeCount = uint.MaxValue,
            KeepAliveCount = uint.MaxValue,
            MaxNotificationsPerPublish = uint.MaxValue
        };
        return null;
    }
    public void RemoveSubscription(in Subscription subscription, in MonitoredItem[] monitors)
    {
        if (Session is not null)
        {
            for (int i = default; i < monitors.Length; i++) monitors[i].Notification -= (item, @event) => { };
            subscription.RemoveItems(monitors);
            subscription.Delete(silent: true);
            Session.RemoveSubscription(subscription);
            subscription.Dispose();
        }
    }
    public void RemoveSubscriptions(in ConcurrentDictionary<string, (Subscription subscription, MonitoredItem[] monitors)> nodes)
    {
        foreach (var node in nodes) RemoveSubscription(node.Value.subscription, node.Value.monitors);
        nodes.Clear();
    }
    public MonitoredItem[] AddMonitoredItems(in Subscription subscription, in (IWorkshopRawdata.EaiType type, string key, string path)[] parameters)
    {
        var title = subscription.DisplayName.ToObject<IWorkshopRawdata.Title>();
        List<MonitoredItem> monitoredItems = new();
        foreach (var (type, key, path) in parameters)
        {
            MonitoredItem monitoredItem = new()
            {
                AttributeId = Attributes.Value,
                SamplingInterval = 500,
                StartNodeId = new NodeId(path),
                DisplayName = new Parameter
                {
                    ParameterKey = key,
                    Process = type
                }.ToJson()
            };
            monitoredItem.Notification += async (monitoredItem, @event) =>
            {
                var parameter = monitoredItem.DisplayName.ToObject<Parameter>();
                try
                {
                    if (@event.NotificationValue is MonitoredItemNotification notification)
                    {
                        switch (parameter.Process)
                        {
                            case IWorkshopRawdata.EaiType.Information:
                                {
                                    var converter = _registerTrigger.ChangeStatus(parameter.ParameterKey);
                                    var value = Convert.ToString(notification.Value.WrappedValue.Value);
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        var status = _registerTrigger.AsStatus(value, converter);
                                        if (status is not IEquipment.Status.Unused)
                                        {
                                            await _workshopRawdata.InsertAsync(title, status);
                                        }
                                        else
                                        {
                                            _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
                                            {
                                                Title = nameof(DigitalExpert).Joint(nameof(AddMonitoredItems)).Joint(nameof(IWorkshopRawdata.EaiType.Information)),
                                                Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}",
                                                Detail = notification.Value.WrappedValue.TypeInfo.BuiltInType.ToString(),
                                                Trace = value
                                            });
                                        }
                                    }
                                }
                                break;

                            case IWorkshopRawdata.EaiType.Parameter:
                                {
                                    var value = Convert.ToString(notification.Value.WrappedValue.Value);
                                    if (float.TryParse(value, out var dataValue))
                                    {
                                        await _workshopRawdata.InsertAsync(title, new[]
                                        {
                                            new IWorkshopRawdata.Parameter.Meta
                                            {
                                                DataNo = parameter.ParameterKey,
                                                DataValue = dataValue
                                            }
                                        });
                                    }
                                    else
                                    {
                                        _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
                                        {
                                            Title = nameof(DigitalExpert).Joint(nameof(AddMonitoredItems)).Joint(nameof(IWorkshopRawdata.EaiType.Parameter)),
                                            Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}".Joint($"{nameof(parameter.ParameterKey)}:{parameter.ParameterKey}", ",\u00A0"),
                                            Detail = notification.Value.WrappedValue.TypeInfo.BuiltInType.ToString(),
                                            Trace = value ?? "empty content"
                                        });
                                    }
                                }
                                break;

                            case IWorkshopRawdata.EaiType.Production:
                                {
                                    var value = Convert.ToString(notification.Value.WrappedValue.Value);
                                    if (int.TryParse(value, out var count))
                                    {
                                        await _workshopRawdata.InsertAsync(title, new[]
                                        {
                                            new IWorkshopRawdata.Production.Meta
                                            {
                                                DispatchNo = string.Empty,
                                                BatchNo = string.Empty,
                                                Output = count
                                            }
                                        });
                                    }
                                    else
                                    {
                                        _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
                                        {
                                            Title = nameof(DigitalExpert).Joint(nameof(AddMonitoredItems)).Joint(nameof(IWorkshopRawdata.EaiType.Production)),
                                            Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}",
                                            Detail = notification.Value.WrappedValue.TypeInfo.BuiltInType.ToString(),
                                            Trace = value ?? "empty content"
                                        });
                                    }
                                }
                                break;

                            default:
                                throw new Exception("::> Incorrect service EaiType");
                        }
                    }
                }
                catch (Exception e)
                {
                    _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
                    {
                        Title = nameof(DigitalExpert).Joint(nameof(AddMonitoredItems)),
                        Burst = $"{nameof(title.EquipmentNo)}:{title.EquipmentNo}".Joint($"{nameof(parameter.Process)}:{parameter.Process}", ",\u00A0"),
                        Detail = e.Message
                    });
                }
            };
            monitoredItems.Add(monitoredItem);
        }
        subscription.AddItems(monitoredItems);
        if (Session is not null)
        {
            Session.AddSubscription(subscription);
            subscription.Create();
            return monitoredItems.ToArray();
        }
        return Array.Empty<MonitoredItem>();
    }
    public ReferenceDescriptionCollection BrowserDetailNode(NodeId nodeId)
    {
        BrowseDescriptionCollection browses = new()
        {
            new()
            {
                NodeId = nodeId,
                IncludeSubtypes = true,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.Aggregates,
                NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType |
                NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType),
                ResultMask = (uint)BrowseResultMask.All
            },
            new()
            {
                NodeId = nodeId,
                IncludeSubtypes = true,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View |
                NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType),
                ResultMask = (uint)BrowseResultMask.All
            }
        };
        ReferenceDescriptionCollection? references = GetReferenceDescriptionCollection(browses);
        ReferenceDescriptionCollection? GetReferenceDescriptionCollection(BrowseDescriptionCollection browseDescriptions)
        {
            try
            {
                if (Session is null) return null;
                ReferenceDescriptionCollection referenceDescriptionCollections = new();
                BrowseDescriptionCollection browseDescriptionCollections = new();
                while (browseDescriptions.Count > 0)
                {
                    Session.Browse(requestHeader: default, view: default,
                    requestedMaxReferencesPerNode: default, nodesToBrowse: browseDescriptions,
                    results: out BrowseResultCollection? browseResultCollections,
                    diagnosticInfos: out DiagnosticInfoCollection? diagnosticInfoCollections);
                    ClientBase.ValidateResponse(browseResultCollections, browseDescriptions);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollections, browseDescriptions);
                    ByteStringCollection? continuationPoints = new();
                    for (var item = 0; item < browseDescriptions.Count; item++)
                    {
                        if (StatusCode.IsBad(browseResultCollections[item].StatusCode))
                        {
                            if (browseResultCollections[item].StatusCode == StatusCodes.BadNoContinuationPoints)
                            {
                                browseDescriptionCollections.Add(browseDescriptions[item]);
                            }
                            continue;
                        }
                        if (browseResultCollections[item].References.Count is 0)
                        {
                            continue;
                        }
                        referenceDescriptionCollections.AddRange(browseResultCollections[item].References);
                        if (browseResultCollections[item].ContinuationPoint is not null)
                        {
                            continuationPoints.Add(browseResultCollections[item].ContinuationPoint);
                        }
                    }
                    ByteStringCollection? revisedContiuationPoints = new();
                    while (continuationPoints.Count > 0)
                    {
                        Session.BrowseNext(requestHeader: default, releaseContinuationPoints: true,
                        continuationPoints: continuationPoints, results: out browseResultCollections,
                        diagnosticInfos: out diagnosticInfoCollections);
                        ClientBase.ValidateResponse(browseResultCollections, continuationPoints);
                        ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollections, continuationPoints);
                        for (var item = 0; item < continuationPoints.Count; item++)
                        {
                            if (StatusCode.IsBad(browseResultCollections[item].StatusCode))
                            {
                                continue;
                            }
                            if (browseResultCollections[item].References.Count is 0)
                            {
                                continue;
                            }
                            referenceDescriptionCollections.AddRange(browseResultCollections[item].References);
                            if (browseResultCollections[item].ContinuationPoint is not null)
                            {
                                revisedContiuationPoints.Add(browseResultCollections[item].ContinuationPoint);
                            }
                        }
                        revisedContiuationPoints = continuationPoints;
                    }
                    browseDescriptions = browseDescriptionCollections;
                }
                return referenceDescriptionCollections;
            }
            catch (Exception e)
            {
                _collectPromoter.OnLatest(new ICollectPromoter.CollectiveEventArgs()
                {
                    Title = nameof(DigitalExpert).Joint(nameof(BrowserDetailNode)),
                    Detail = Endpoint.Joint(e.Message)
                });
                return null;
            }
        }
        return references!;
    }
    public ReferenceDescriptionCollection BrowserNode(NodeId nodeId) => new Browser(Session).Browse(nodeId);
    public DataValue ReadNode(NodeId nodeId)
    {
        ReadValueIdCollection readValueIdCollections = new()
        {
            new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value
            }
        };
        if (Session is not null)
        {
            Session.Read(maxAge: default, requestHeader: default,
            results: out DataValueCollection dataValueCollections,
            diagnosticInfos: out DiagnosticInfoCollection diagnosticInfoCollections,
            nodesToRead: readValueIdCollections, timestampsToReturn: TimestampsToReturn.Neither);
            ClientBase.ValidateResponse(dataValueCollections, readValueIdCollections);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollections, readValueIdCollections);
            return dataValueCollections[default];
        }
        return new();
    }
    public IEnumerable<DataValue> ReadNodes(NodeId[] nodeIds)
    {
        ReadValueIdCollection readValueIdCollections = new();
        for (var item = 0; item < nodeIds.Length; item++) readValueIdCollections.Add(new()
        {
            NodeId = nodeIds[item],
            AttributeId = Attributes.Value
        });
        if (Session is not null)
        {
            Session.Read(requestHeader: default, maxAge: default,
            timestampsToReturn: TimestampsToReturn.Neither, nodesToRead: readValueIdCollections,
            results: out DataValueCollection dataValueCollections,
            diagnosticInfos: out DiagnosticInfoCollection diagnosticInfoCollections);
            ClientBase.ValidateResponse(dataValueCollections, readValueIdCollections);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollections, readValueIdCollections);
            return dataValueCollections;
        }
        return Enumerable.Empty<DataValue>();
    }
    public Task<IEnumerable<DataValue>> ReadNodesAsync(NodeId[] nodeIds)
    {
        ReadValueIdCollection readValueIds = new();
        for (var item = 0; item < nodeIds.Length; item++) readValueIds.Add(new()
        {
            NodeId = nodeIds[item],
            AttributeId = Attributes.Value
        });
        TaskCompletionSource<IEnumerable<DataValue>> taskCompletionSource = new();
        if (Session is not null)
        {
            Session.BeginRead(requestHeader: default, maxAge: default, timestampsToReturn: TimestampsToReturn.Neither,
            nodesToRead: readValueIds, callback: callback =>
            {
                var response = Session.EndRead(result: callback, results: out DataValueCollection dataValueCollections,
                diagnosticInfos: out DiagnosticInfoCollection diagnosticInfoCollections);
                try
                {
                    CheckReturnValue(response.ServiceResult);
                    taskCompletionSource.TrySetResult(dataValueCollections);
                }
                catch (Exception e)
                {
                    taskCompletionSource.TrySetException(e);
                }
            },
            asyncState: null);
            return taskCompletionSource.Task;
        }
        return Task.FromResult(Enumerable.Empty<DataValue>());
    }

    /// <summary>
    /// 0:NodeClass  1:Value  2:AccessLevel  3:DisplayName  4:Description
    /// </summary>
    public DataValue[] ReadNodeAttributes(IEnumerable<NodeId> nodeIds)
    {
        ReadValueIdCollection readValueIds = new();
        foreach (var nodeId in nodeIds)
        {
            readValueIds.Add(new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.NodeClass
            });
            readValueIds.Add(new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value
            });
            readValueIds.Add(new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.AccessLevel
            });
            readValueIds.Add(new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.DisplayName
            });
            readValueIds.Add(new()
            {
                NodeId = nodeId,
                AttributeId = Attributes.Description
            });
        }
        if (Session is not null)
        {
            Session.Read(maxAge: default, requestHeader: default, nodesToRead: readValueIds,
            timestampsToReturn: TimestampsToReturn.Neither, results: out DataValueCollection dataValues,
            diagnosticInfos: out DiagnosticInfoCollection diagnostics);
            ClientBase.ValidateResponse(dataValues, readValueIds);
            ClientBase.ValidateDiagnosticInfos(diagnostics, readValueIds);
            return dataValues.ToArray();
        }
        return Array.Empty<DataValue>();
    }
    public T ReadNode<T>(string nodeId) => (T)ReadNode(new(nodeId)).Value;
    public Task<T> ReadNodeAsync<T>(string nodeId)
    {
        ReadValueIdCollection readValueIdCollection = new()
        {
            new()
            {
                NodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value
            }
        };
        var taskCompletionSource = new TaskCompletionSource<T>();
        Session?.BeginRead(maxAge: default, requestHeader: default, timestampsToReturn: TimestampsToReturn.Neither,
        nodesToRead: readValueIdCollection, callback: callback =>
        {
            var response = Session.EndRead(result: callback, results: out DataValueCollection dataValueCollections,
            diagnosticInfos: out DiagnosticInfoCollection diagnosticInfoCollections);
            try
            {
                if (!StatusCode.IsGood(response.ServiceResult))
                {
                    throw new Exception(string.Format("Invalid response from the server. (Response MessageStatus: {0})", response.ServiceResult));
                }
                if (!StatusCode.IsGood(dataValueCollections[default].StatusCode))
                {
                    throw new Exception(string.Format("Invalid response from the server. (Response MessageStatus: {0})", dataValueCollections[default].StatusCode));
                }
                taskCompletionSource.TrySetResult((T)dataValueCollections[default].Value);
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        },
        asyncState: default);
        return taskCompletionSource.Task;
    }
    public IEnumerable<T> ReadNodes<T>(string[] tags)
    {
        List<T> results = new();
        ReadValueIdCollection readValueIds = new();
        for (var item = 0; item < tags.Length; item++) readValueIds.Add(new()
        {
            NodeId = new NodeId(tags[item]),
            AttributeId = Attributes.Value
        });
        if (Session is not null)
        {
            Session.Read(maxAge: default, requestHeader: default, nodesToRead: readValueIds,
            timestampsToReturn: TimestampsToReturn.Neither, results: out DataValueCollection dataValues,
            diagnosticInfos: out DiagnosticInfoCollection diagnostics);
            ClientBase.ValidateResponse(dataValues, readValueIds);
            ClientBase.ValidateDiagnosticInfos(diagnostics, readValueIds);
            foreach (var dataValue in dataValues) results.Add((T)dataValue.Value);
        }
        return results;
    }
    public Task<IEnumerable<T>> ReadNodesAsync<T>(string[] tags)
    {
        ReadValueIdCollection readValueIds = new();
        for (var item = 0; item < tags.Length; item++) readValueIds.Add(new()
        {
            NodeId = new NodeId(tags[item]),
            AttributeId = Attributes.Value
        });
        TaskCompletionSource<IEnumerable<T>> taskCompletionSource = new();
        Session?.BeginRead(maxAge: default, requestHeader: default, timestampsToReturn: TimestampsToReturn.Neither,
        nodesToRead: readValueIds, callback: callback =>
        {
            var response = Session.EndRead(result: callback, results: out DataValueCollection dataValues,
            diagnosticInfos: out DiagnosticInfoCollection diagnostics);
            try
            {
                List<T> results = new();
                CheckReturnValue(response.ServiceResult);
                foreach (var collection in dataValues)
                {
                    results.Add((T)collection.Value);
                }
                taskCompletionSource.TrySetResult(results);
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        }, asyncState: default);
        return taskCompletionSource.Task;
    }
    public bool IsWriteableNode(NodeId nodeId)
    {
        ReadValueIdCollection readValueIds = new()
        {
            new( )
            {
                NodeId = nodeId,
                AttributeId = Attributes.AccessLevel
            }
        };
        Session!.Read(requestHeader: default, maxAge: default, timestampsToReturn: TimestampsToReturn.Neither,
        nodesToRead: readValueIds, results: out DataValueCollection dataValues,
        diagnosticInfos: out DiagnosticInfoCollection diagnostics);
        ClientBase.ValidateResponse(dataValues, readValueIds);
        ClientBase.ValidateDiagnosticInfos(diagnostics, readValueIds);
        var dataValue = dataValues[default];
        if (dataValue.WrappedValue == Variant.Null) return true;
        return (byte)dataValue.WrappedValue.Value is not 1;
    }
    public bool WriteNode<T>(string tag, T value)
    {
        WriteValue writeValue = new()
        {
            NodeId = new NodeId(tag),
            AttributeId = Attributes.Value
        };
        writeValue.Value.Value = value;
        writeValue.Value.StatusCode = StatusCodes.Good;
        writeValue.Value.ServerTimestamp = DateTime.MinValue;
        writeValue.Value.SourceTimestamp = DateTime.MinValue;
        WriteValueCollection writeValues = new()
        {
            writeValue
        };
        Session!.Write(requestHeader: default, nodesToWrite: writeValues,
        results: out StatusCodeCollection statusCodes,
        diagnosticInfos: out DiagnosticInfoCollection diagnostics);
        ClientBase.ValidateResponse(statusCodes, writeValues);
        ClientBase.ValidateDiagnosticInfos(diagnostics, writeValues);
        if (StatusCode.IsBad(statusCodes[default]))
        {
            throw new ServiceResultException(statusCodes[default]);
        }
        return !StatusCode.IsBad(statusCodes[default]);
    }
    public Task<bool> WriteNodeAsync<T>(string nodeId, T value)
    {
        WriteValue writeValue = new()
        {
            NodeId = new NodeId(nodeId),
            AttributeId = Attributes.Value,
        };
        writeValue.Value.Value = value;
        writeValue.Value.StatusCode = StatusCodes.Good;
        writeValue.Value.ServerTimestamp = DateTime.MinValue;
        writeValue.Value.SourceTimestamp = DateTime.MinValue;
        WriteValueCollection nodesToWrites = new()
        {
            writeValue
        };
        TaskCompletionSource<bool> taskCompletionSource = new();
        Session!.BeginWrite(requestHeader: default, nodesToWrite: nodesToWrites, callback: callback =>
        {
            var response = Session.EndWrite(result: callback, results: out StatusCodeCollection statusCodes,
            diagnosticInfos: out DiagnosticInfoCollection diagnostics);
            try
            {
                ClientBase.ValidateResponse(statusCodes, nodesToWrites);
                ClientBase.ValidateDiagnosticInfos(diagnostics, nodesToWrites);
                taskCompletionSource.SetResult(StatusCode.IsGood(statusCodes[default]));
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        },
        asyncState: default);
        return taskCompletionSource.Task;
    }
    public bool WriteNodes(string[] nodeIds, object[] values)
    {
        WriteValueCollection nodesToWrites = new();
        for (var item = 0; item < nodeIds.Length; item++)
        {
            if (item < values.Length)
            {
                WriteValue writeValue = new()
                {
                    NodeId = new(nodeIds[item]),
                    AttributeId = Attributes.Value
                };
                writeValue.Value.Value = values[item];
                writeValue.Value.StatusCode = StatusCodes.Good;
                writeValue.Value.ServerTimestamp = DateTime.MinValue;
                writeValue.Value.SourceTimestamp = DateTime.MinValue;
                nodesToWrites.Add(writeValue);
            }
        }
        Session!.Write(requestHeader: default, nodesToWrite: nodesToWrites,
        results: out StatusCodeCollection statusCodes,
        diagnosticInfos: out DiagnosticInfoCollection diagnostics);
        ClientBase.ValidateResponse(statusCodes, nodesToWrites);
        ClientBase.ValidateDiagnosticInfos(diagnostics, nodesToWrites);
        bool result = true;
        foreach (var statusCode in statusCodes)
        {
            if (StatusCode.IsBad(statusCode))
            {
                result = false;
                break;
            }
        }
        return result;
    }
    void BeginWatchdog(in Session session) => session.KeepAlive += async (session, @event) =>
    {
        if (@event.Status is not null && ServiceResult.IsNotGood(@event.Status) && Connected)
        {
            ClearSession();
            Connected = false;
            bool succeeded = default;
            PeriodicTimer periodic = new(TimeSpan.FromSeconds(10));
            while (await periodic.WaitForNextTickAsync())
            {
                try
                {
                    Session = await CreateAsync(Configuration!);
                    foreach (var node in Nodes)
                    {
                        await node.Value.subscription.DeleteAsync(silent: true);
                        Session.AddSubscription(node.Value.subscription);
                        await node.Value.subscription.CreateAsync();
                    }
                    BeginWatchdog(Session);
                    succeeded = true;
                }
                catch (Exception)
                {
                    ClearSession();
                }
                if (succeeded) periodic.Dispose();
            }
            Connected = true;
        }
    };
    async Task<ApplicationConfiguration> BeginFunctionAsync()
    {
        CertificateValidator certificateValidator = new();
        certificateValidator.CertificateValidation += (validator, validationEvent) =>
        {
            if (ServiceResult.IsGood(validationEvent.Error))
            {
                validationEvent.Accept = true;
            }
            else if (validationEvent.Error.StatusCode.Code is StatusCodes.BadCertificateUntrusted)
            {
                validationEvent.Accept = true;
            }
            else
            {
                throw new Exception(string.Format("Certificate validation error： {0}: {1}", validationEvent.Error.Code, validationEvent.Error.AdditionalInfo));
            }
        };
        await certificateValidator.Update(new SecurityConfiguration()
        {
            AutoAcceptUntrustedCertificates = true,
            RejectSHA1SignedCertificates = false,
            MinimumCertificateKeySize = 1024,
        });
        ApplicationConfiguration configuration = new()
        {
            DisableHiResClock = true,
            ApplicationType = ApplicationType.Client,
            CertificateValidator = certificateValidator,
            ServerConfiguration = new ServerConfiguration
            {
                MaxMessageQueueSize = 100000,
                MaxSubscriptionCount = 100000,
                MaxPublishRequestCount = 100000,
                MaxNotificationQueueSize = 100000
            },
            SecurityConfiguration = new SecurityConfiguration
            {
                MinimumCertificateKeySize = 1024,
                RejectSHA1SignedCertificates = false,
                SuppressNonceValidationErrors = true,
                AutoAcceptUntrustedCertificates = true,
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\My"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\Root"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\Root"
                }
            },
            TransportQuotas = new TransportQuotas
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
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = Timeout.Infinite,
                MinSubscriptionLifetime = Timeout.Infinite
            },
            ApplicationName = ClientId
        };
        await configuration.Validate(ApplicationType.Client);
        return configuration;
    }
    Task<Session> CreateAsync(ApplicationConfiguration configuration)
    {
        return Session.Create(configuration: configuration, sessionName: configuration.ApplicationName,
        updateBeforeConnect: default, checkDomain: default, sessionTimeout: 100 * 1000,
        endpoint: new(collection: default, CoreClientUtils.SelectEndpoint(Endpoint, useSecurity: false), EndpointConfiguration.Create(configuration)),
        identity: Username == string.Empty && Password == string.Empty ? new UserIdentity(new AnonymousIdentityToken()) : new(Username, Password),
        preferredLocales: new[]
        {
            Language
        });
    }
    static void CheckReturnValue(StatusCode status)
    {
        if (!StatusCode.IsGood(status)) throw new Exception(string.Format("Invalid response from the server. (Response MessageStatus: {0})", status));
    }
    void ClearSession()
    {
        if (Session is not null)
        {
            Session.Close(5);
            Session?.Dispose();
            Session = null;
        }
    }
    void Initializer()
    {
        RemoveSubscriptions(Nodes);
        Nodes.Clear();
        ClearSession();
    }
    void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing) Initializer();
            Disposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public record struct Parameter
    {
        public string ParameterKey { get; set; }
        public IWorkshopRawdata.EaiType Process { get; set; }
    }
    bool Disposed { get; set; }
    Session? Session { get; set; }
    ApplicationConfiguration? Configuration { get; set; }
    public required string ClientId { get; set; }
    public required string Endpoint { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool Connected { get; set; }
    public ConcurrentDictionary<string, (Subscription subscription, MonitoredItem[] monitors)> Nodes { get; set; } = new();
}