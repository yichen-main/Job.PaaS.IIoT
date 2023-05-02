namespace IIoT.Domain.Shared.Functions.Mediums;
public static class TextMedium
{
    public static async ValueTask WriteLineAsync(this string text, string fullPath)
    {
        await using FileStream fileStream = new(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        await using StreamWriter streamWriter = new(fileStream);
        await streamWriter.WriteLineAsync(text);
    }
    public static async ValueTask<(string fileName, Dictionary<string, string> texts)> AsFileInfoToDeserialize(
    this IFileInfo fileInfo, int keyIndex = default, string annotationSign = "#", string delimiter = ",")
    {
        await using var stream = fileInfo.CreateReadStream();
        using StreamReader streamReader = new(stream);
        var original = await streamReader.ReadToEndAsync();
        {
            stream.Close();
            streamReader.Close();
            Dictionary<string, string> dicResult = new();
            foreach (var content in original.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (!content.StartsWith(annotationSign))
                {
                    var key = string.Empty;
                    var analyze = content.Split(delimiter);
                    if (analyze.Any())
                    {
                        List<string> values = new();
                        for (var item = 0; item < analyze.Length; item++)
                        {
                            if (item == keyIndex)
                            {
                                key = analyze[item];
                            }
                            else
                            {
                                values.Add(analyze[item]);
                            }
                        }
                        if (dicResult.TryGetValue(key, out var resValue))
                        {
                            dicResult[key] = string.Join(",", values);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(key)) dicResult.Add(key, string.Join(",", values));
                        }
                    }
                }
            }
            return (fileInfo.Name, dicResult);
        }
    }
    public static IEnumerable<T[]> Split<T>(this T[] arrays, int size)
    {
        for (var i = 0; i < arrays.Length / size + 1; i++) yield return arrays.Skip(i * size).Take(size).ToArray();
    }
    public static T? ToObject<T>(this string content) => JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
    public static T? ToObject<T>(this byte[] contents) => JsonSerializer.Deserialize<T>(contents, new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
    public static string ToJson<T>(this T @object, bool indented = false) => JsonSerializer.Serialize(@object, typeof(T), new JsonSerializerOptions
    {
        WriteIndented = indented,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
    public static T WriteXmlDeserializer<T>(this string path)
    {
        return (T)new XmlSerializer(typeof(T)).Deserialize(new FileStream(path ?? string.Empty, FileMode.Open))!;
    }
    public static string WriteXmlFile(this object entity)
    {
        using StringWriter stringWriter = new();
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);
        XmlSerializer xmlSerializer = new(entity.GetType());
        xmlSerializer.Serialize(XmlWriter.Create(stringWriter, new()
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true,
        }), entity, namespaces);
        return stringWriter.ToString();
    }
    public static string ToSerializerXml(this object @object, bool isOmitXmlDeclaration, bool isIndent = default)
    {
        try
        {
            XmlWriterSettings xmlWriterSettings = new()
            {
                OmitXmlDeclaration = isOmitXmlDeclaration,
                Indent = isIndent,
                Encoding = Encoding.UTF8
            };
            using MemoryStream memoryStream = new();
            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
            {
                XmlSerializerNamespaces namespaces = new();
                namespaces.Add(string.Empty, string.Empty);
                XmlSerializer xmlSerializer = new(@object.GetType());
                xmlSerializer.Serialize(xmlWriter, @object, namespaces);
            }
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}