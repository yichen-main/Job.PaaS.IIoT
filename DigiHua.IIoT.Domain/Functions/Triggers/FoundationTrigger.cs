namespace IIoT.Domain.Functions.Triggers;
internal sealed class FoundationTrigger : IFoundationTrigger
{
    public string UseEncryptAES(in string text)
    {
        using var aes = Aes.Create();
        using var msEncrypt = new MemoryStream();
        using var encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(Morse.DigiHua.ToMd5()), aes.IV);
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt)) swEncrypt.Write(text);
        {
            var iv = aes.IV;
            var decryptedContent = msEncrypt.ToArray();
            var result = new byte[iv.Length + decryptedContent.Length];
            Buffer.BlockCopy(iv, default, result, default, iv.Length);
            Buffer.BlockCopy(decryptedContent, default, result, iv.Length, decryptedContent.Length);
            return Convert.ToBase64String(result);
        }
    }
    public string UseDecryptAES(in string text)
    {
        var iv = new byte[16];
        var fullCipher = Convert.FromBase64String(text);
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, default, iv, default, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, default, fullCipher.Length - iv.Length);
        {
            using var aes = Aes.Create();
            using var decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(Morse.DigiHua.ToMd5()), iv);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
    public string UseFormatXml(in string text)
    {
        StringBuilder stringBuilder = new();
        using StringWriter stringWriter = new(stringBuilder);
        using XmlTextWriter xmlTextWriter = new(stringWriter)
        {
            Formatting = Formatting.Indented,
            Indentation = 2,
            IndentChar = ' ',
        };
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(text ?? string.Empty);
        xmlDocument.WriteTo(xmlTextWriter);
        return stringBuilder.ToString();
    }
    public T? UseDeserializeXml<T>(in string text)
    {
        XmlSerializer @object = new(typeof(T));
        using TextReader reader = new StringReader(text ?? string.Empty);
        var result = @object.Deserialize(reader);
        return result is not null ? (T)result : default;
    }
    public string UseSerializerXml(in object entity, in bool indent = default)
    {
        using StringWriter stringWriter = new();
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);
        XmlSerializer serializer = new(entity.GetType());
        serializer.Serialize(XmlWriter.Create(stringWriter, new()
        {
            Indent = indent,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true,
        }), entity, namespaces);
        return stringWriter.ToString();
    }
    public IDictionary<string, object> UseJsonToDictionary(in string text)
    {
        Dictionary<string, object> results = new(StringComparer.Ordinal);
        foreach (var item in JsonSerializer.Deserialize<JsonObject>(text ?? string.Empty)!)
        {
            object @object;
            switch (item.Value)
            {
                case null:
                    @object = null!;
                    break;

                case JsonArray:
                    @object = item.Value.AsArray();
                    break;

                case JsonObject:
                    @object = item.Value.AsObject();
                    break;

                default:
                    var jsonValue = item.Value.AsValue();
                    if (item.Value.ToJsonString().StartsWith('"'))
                    {
                        if (jsonValue.TryGetValue<DateTime>(out var dateTime)) @object = dateTime;
                        else if (jsonValue.TryGetValue<Guid>(out var guid)) @object = guid;
                        else @object = jsonValue.GetValue<string>();
                    }
                    else @object = jsonValue.GetValue<decimal>();
                    break;
            }
            results.Add(item.Key, @object);
        }
        return results;
    }
    public IConfiguration InitialFile(in string path, in Extension type = Extension.Json, in bool change = true) => type switch
    {
        Extension.Text => new ConfigurationBuilder().AddIniFile(path, default, reloadOnChange: change).Build(),
        Extension.Yaml => new ConfigurationBuilder().AddYamlFile(path, default, reloadOnChange: change).Build(),
        _ => new ConfigurationBuilder().AddJsonFile(path, default, reloadOnChange: change).Build()
    };
    public T RefreshFile<T>(in T entity, in IConfiguration configuration)
    {
        configuration.Bind(entity);
        return entity;
    }
    public async ValueTask CreateFileAaync<T>(string path, T entity, Extension type = Extension.Json, bool cover = default)
    {
        try
        {
            if ((!System.IO.File.Exists(path) || cover) && entity is not null)
            {
                await using FileStream stream = new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                await using StreamWriter result = new(stream);
                switch (type)
                {
                    case Extension.Json:
                        await result.WriteLineAsync(entity.ToJson(indented: true));
                        break;

                    case Extension.Text:
                        await result.WriteLineAsync(entity.ToString());
                        break;

                    case Extension.Yaml:
                        await result.WriteLineAsync(new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build().Serialize(entity));
                        break;
                }
                await result.FlushAsync();
                await stream.FlushAsync();
                result.Close();
                stream.Close();
            }
        }
        catch (Exception e)
        {
            Log.Fatal(Morse.HistoryDefault, nameof(FoundationTrigger).Joint(nameof(FoundationTrigger.CreateFileAaync)), new
            {
                e.Message,
                e.StackTrace
            });
        }
    }
    public void MonitorFile(in Action<object, FileSystemEventArgs> action, in string path, in string name) => new FileSystemWatcher
    {
        Path = path,
        Filter = name,
        EnableRaisingEvents = true,
        IncludeSubdirectories = false,
        NotifyFilter = NotifyFilters.LastWrite
    }.Changed += new FileSystemEventHandler(action);
    public IEnumerable<TSource> Concat<TSource>(IEnumerable<TSource> originals, IEnumerable<TSource> merges)
    {
        foreach (var item in originals) yield return item;
        foreach (var item in merges) yield return item;
    }
    public IEnumerable<T> FindRepeat<T>(in T[] entities)
    {
        HashSet<T> hashset = new();
        return entities.Where(item => !hashset.Add(item));
    }
    public bool CheckParity<T>(in IEnumerable<T> fronts, in IEnumerable<T> backs) where T : notnull
    {
        Dictionary<T, int> result = new();
        foreach (T item in fronts)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(result, item);
            if (!Unsafe.IsNullRef(ref value)) value++;
            else result.Add(item, 1);
        }
        foreach (T item in backs)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(result, item);
            if (!Unsafe.IsNullRef(ref value)) value--;
            else return default;
        }
        return result.Values.All(item => item == default);
    }
    public void Clear(EventHandler? @event)
    {
        if (@event is not null) Array.ForEach(@event.GetInvocationList(), item => @event -= (EventHandler)item);
    }
}