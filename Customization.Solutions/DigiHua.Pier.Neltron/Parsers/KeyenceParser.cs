namespace Pier.Neltron.Parsers;
internal sealed class KeyenceParser
{
    readonly IRemoteManufactureWrapper _remoteManufacture;
    public KeyenceParser(IRemoteManufactureWrapper remoteManufacture)
    {
        _remoteManufacture = remoteManufacture;
    }
    internal async ValueTask PushAsync(Setup setup, string branchPath, (string fileName, DateTime dateTime, Dictionary<string, string> texts)[] entities)
    {
        await Task.Delay(1000);
        foreach (var (fileName, dateTime, texts) in entities)
        {
            List<Task> results = new();
            //foreach (var equipment in equipments.Where(item => item.Id != string.Empty))
            //{
            //    if (equipment.Status.KeyId != string.Empty)
            //    {
            //        if (texts.TryGetValue(equipment.Status.KeyId, out var status))
            //        {
            //            //results.Add(_remoteStrategyWrapper.Equipment.SendInformationAsync(equipment.Id, new()
            //            //{
            //            //    Status = status.Split(setup.Global.ContentDelimiter)[equipment.Status.Index]
            //            //}));
            //        }
            //        else
            //        {
            //            new OriginateMedium.CollectiveEventArgs()
            //            {
            //                Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                Detail = "[Status.KeyId] does not exist"
            //            }.OnLatest();
            //        }
            //    }
            //    List<IParameterRemote.Information> parameters = new();
            //    {
            //        foreach (var item in equipment.Parameters.Where(item => item.DataNo != string.Empty && item.DataValue.KeyId != string.Empty))
            //        {
            //            if (texts.TryGetValue(item.DataValue.KeyId, out var dataValue))
            //            {
            //                if (double.TryParse(dataValue.Split(',')[item.DataValue.Index], out var value))
            //                {
            //                    parameters.Add(new()
            //                    {
            //                        DataNo = item.DataNo,
            //                        DataValue = value
            //                    });
            //                }
            //                else
            //                {
            //                    new OriginateMedium.CollectiveEventArgs()
            //                    {
            //                        Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                        Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                        Detail = "[DataValue] is not an number type"
            //                    }.OnLatest();
            //                }
            //            }
            //            else
            //            {
            //                new OriginateMedium.CollectiveEventArgs()
            //                {
            //                    Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                    Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                    Detail = "[DataValue.KeyId] does not exist"
            //                }.OnLatest();
            //            }
            //        }
            //        if (parameters.Any()) results.Add(_remoteStrategyWrapper.Parameter.SendInformationAsync(equipment.Id, parameters));
            //    }
            //    List<IProductionRemote.Information> productions = new();
            //    {
            //        foreach (var item in equipment.Productions)
            //        {
            //            IProductionRemote.Information production = new()
            //            {
            //                Channel = item.Channel
            //            };
            //            if (item.Counter.KeyId != string.Empty)
            //            {
            //                if (texts.TryGetValue(item.Counter.KeyId, out var countValue))
            //                {
            //                    if (int.TryParse(countValue.Split(',')[item.Counter.Index], out var count))
            //                    {
            //                        production.Counter = count;
            //                    }
            //                    else
            //                    {
            //                        new OriginateMedium.CollectiveEventArgs()
            //                        {
            //                            Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                            Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                            Detail = "[Counter] is not an number type"
            //                        }.OnLatest();
            //                    }
            //                }
            //                else
            //                {
            //                    new OriginateMedium.CollectiveEventArgs()
            //                    {
            //                        Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                        Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                        Detail = "[Counter.KeyId] does not exist"
            //                    }.OnLatest();
            //                }
            //            }
            //            if (item.CycleTime.KeyId != string.Empty)
            //            {
            //                if (texts.TryGetValue(item.CycleTime.KeyId, out var cycleTimeValue))
            //                {
            //                    if (int.TryParse(cycleTimeValue.Split(',')[item.CycleTime.Index], out var cycleTime))
            //                    {
            //                        production.CycleTime = cycleTime;
            //                    }
            //                    else
            //                    {
            //                        new OriginateMedium.CollectiveEventArgs()
            //                        {
            //                            Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                            Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                            Detail = "[CycleTime] is not an number type"
            //                        }.OnLatest();
            //                    }
            //                }
            //                else
            //                {
            //                    new OriginateMedium.CollectiveEventArgs()
            //                    {
            //                        Title = nameof(KeyenceParser).Joint(nameof(PushAsync)),
            //                        Burst = $"{nameof(fileName)}:{fileName}, equipmentId:{equipment.Id}",
            //                        Detail = "[CycleTime.KeyId] does not exist"
            //                    }.OnLatest();
            //                }
            //            }
            //            productions.Add(production);
            //        }
            //        if (productions.Any()) results.Add(_remoteStrategyWrapper.Production.SendInformationAsync(equipment.Id, productions));
            //    }
            //}
            //await Task.WhenAll(results);
            //{
            //    var tag = fileName.Replace(setup.Global.Extension, string.Empty).Split(',')[default];
            //    var path = string.Join(PathChip, new string[]
            //    {
            //        setup.Global.SourcePath,
            //        branchPath is "\\" or "/" ? string.Empty : branchPath
            //    });
            //    DirectoryInfo directory = new(path);
            //    {
            //        directory.Create();
            //        directory.CreateSubdirectory(string.Join(PathChip, new string[]
            //        {
            //            setup.Global.BackupFolderName, tag
            //        }));
            //    }
            //    var source = string.Join(PathChip, new string[]
            //    {
            //        path, fileName
            //    });
            //    if (File.Exists(source))
            //    {
            //        if (setup.Global.RemoveFiles)
            //        {
            //            File.Delete(source);
            //        }
            //        else
            //        {
            //            File.Move(source, string.Join(PathChip, new string[]
            //            {
            //                path, setup.Global.BackupFolderName, tag, fileName
            //            }));
            //        }
            //    }
            //}
        }
    }
    internal async IAsyncEnumerable<(string factoryNo, DateTime dateTime, string[][] equipments)> RealAsync(Setup.SetupMain main)
    {
        using PhysicalFileProvider provider = new(main.RootPath);
        foreach (var folder in main.Folders)
        {
            foreach (var item in provider.GetDirectoryContents(folder.FactoryNo).Where(item => item.Name.Contains(".csv")).Select(item => item.AsFileInfoToDeserialize()))
            {
                var (fileName, texts) = await item;
                var names = fileName.Replace(".csv", string.Empty).Split("_");
                if (DateTime.TryParseExact(names[1], main.Timestamp, default, DateTimeStyles.None, out var dateTime))
                {
                    yield return (folder.FactoryNo, dateTime, texts.Values.ToArray().Split(250).ToArray());
                }
                else
                {
                    //new ICollectPromoter.CollectiveEventArgs()
                    //{
                    //    Title = nameof(KeyenceParser).Joint(nameof(RealAsync)),
                    //    Burst = $"{nameof(fileName)}:{fileName}",
                    //    Detail = "date format does not match specified"
                    //}.OnLatest();
                }
            }
        }
    }
    internal sealed class Setup
    {
        [YamlMember(ApplyNamingConventions = false)] public SetupGlobal Global { get; init; } = new();
        [YamlMember(ApplyNamingConventions = false)] public SetupIndex Index { get; init; } = new();
        [YamlMember(ApplyNamingConventions = false)] public SetupConvert Convert { get; init; } = new();
        [YamlMember(ApplyNamingConventions = false)] public SetupMain Main { get; init; } = new();
        internal sealed class SetupGlobal
        {
            [YamlMember(ApplyNamingConventions = false)] public bool Enable { get; init; }
            [YamlMember(ApplyNamingConventions = false)] public int Frequency { get; init; } = 10;
        }
        internal sealed class SetupIndex
        {
            [YamlMember(ApplyNamingConventions = false)] public SetupEquipment Equipment { get; init; } = new();
            [YamlMember(ApplyNamingConventions = false)] public SetupProduce Produce { get; init; } = new();
            internal sealed class SetupEquipment
            {
                [YamlMember(ApplyNamingConventions = false)] public int Status { get; init; }
                [YamlMember(ApplyNamingConventions = false)] public int GroupName { get; init; }
                [YamlMember(ApplyNamingConventions = false)] public int AlarmCode { get; init; }
                [YamlMember(ApplyNamingConventions = false)] public int AlarmDetail { get; init; }
            }
            internal sealed class SetupProduce
            {
                [YamlMember(ApplyNamingConventions = false)] public int TotalCount { get; init; }
                [YamlMember(ApplyNamingConventions = false)] public int DefectCount { get; init; }
            }
        }
        internal sealed class SetupConvert
        {
            [YamlMember(ApplyNamingConventions = false)] public string Run { get; init; } = "2";
            [YamlMember(ApplyNamingConventions = false)] public string Idle { get; init; } = "1";
            [YamlMember(ApplyNamingConventions = false)] public string Error { get; init; } = "3";
            [YamlMember(ApplyNamingConventions = false)] public string Settings { get; init; } = "5,8,10";
            [YamlMember(ApplyNamingConventions = false)] public string Shutdown { get; init; } = "0";
            [YamlMember(ApplyNamingConventions = false)] public string Repair { get; init; } = "4";
            [YamlMember(ApplyNamingConventions = false)] public string Maintenance { get; init; } = "9";
            [YamlMember(ApplyNamingConventions = false)] public string Hold { get; init; } = "6,7";
        }
        internal sealed class SetupMain
        {
            [YamlMember(ApplyNamingConventions = false)] public string RootPath { get; init; } = "D:\\";
            [YamlMember(ApplyNamingConventions = false)] public string Timestamp { get; init; } = "yyMMddHHmm";
            [YamlMember(ApplyNamingConventions = false)] public SetupBackup Backup { get; init; } = new();
            [YamlMember(ApplyNamingConventions = false)] public Folder[] Folders { get; init; } = new[] { new Folder() };
            internal sealed class SetupBackup
            {
                [YamlMember(ApplyNamingConventions = false)] public bool Deletion { get; init; }
                [YamlMember(ApplyNamingConventions = false)] public string FolderName { get; init; } = "Backups";
            }
            internal sealed class Folder
            {
                [YamlMember(ApplyNamingConventions = false)] public string FactoryNo { get; init; } = nameof(Neltron);
                [YamlMember(ApplyNamingConventions = false)] public Equipment[] Equipments { get; init; } = new[] { new Equipment() };
                internal sealed class Equipment
                {
                    [YamlMember(ApplyNamingConventions = false)] public string Name { get; init; } = string.Empty;
                    [YamlMember(ApplyNamingConventions = false)] public string BinaryNo { get; init; } = string.Empty;
                }
            }
        }
    }
}