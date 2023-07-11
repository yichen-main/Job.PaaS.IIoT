﻿namespace IIoT.Domain.Infrastructure.Postgres;
public static class NpgsqlExpansion
{
    public const string CurrentSign = "id";
    public static string Nameplate(this string address, in int port, in string database, in string username, in string password) => $"Server={address};Port={port};username={username};password={password};Database={database};enlist=true;Timeout=180;Command Timeout=180";
    public static FieldAttribute FieldInfo<T>(this string name) => typeof(T).GetProperty(name)!.GetCustomAttribute<FieldAttribute>()!;
    public static string To<T>(this string name) => typeof(T).GetProperty(name)!.GetCustomAttribute<FieldAttribute>()!.Name;
    public static string TableName<T>() => typeof(T).GetCustomAttribute<TableAttribute>()!.Name;
    public static string AddExcluded<T>(this string name) => $"{name.To<T>()}=EXCLUDED.{name.To<T>()}";
    public static string AddUpsert(this string insert, in string primaryKey) => $"{insert} ON CONFLICT ({primaryKey}) DO NOTHING";
    public static string AddUpsert(this string insert, in string primaryKey, in string condition) => $"{insert} ON CONFLICT ({primaryKey}) DO UPDATE SET {condition}";
    public static string AddObjectFilter(this StringBuilder builder, in string field, in string value) => builder.AppendFormat($"WHERE {field} = @{value}").ToString();
    public static string AddEqualFilter(this StringBuilder builder, in string field, in Guid value) => builder.AppendFormat($"WHERE {field} = '{value}'").ToString();
    public static string AddEqualFilter(this StringBuilder builder, in string field, in int value) => builder.AppendFormat($"WHERE {field} = '{value}'").ToString();
    public static string AddTitleFilter(this StringBuilder builder, in string dateTimeFormat)
    {
        if (!string.IsNullOrEmpty(dateTimeFormat))
        {
            var time = dateTimeFormat.Split("@");
            if (time.Length is 2)
            {
                builder.AppendFormat("WHERE ");
                builder = builder.TimestampFormat(time[0], time[1]);
            }
        }
        return builder.ToString();
    }
    public static string AddIntervalFilter(this StringBuilder builder, in string format, (string field, string value)[] filters, in string tag = "create_time", in int limit = 10000)
    {
        builder.AppendFormat("WHERE ");
        Array.ForEach(filters, filter =>
        {
            builder.AppendFormat($"{filter.field} = '{filter.value}' ");
            if (Array.IndexOf(filters, filter) != filters.Length - 1) builder.AppendFormat("AND ");
        });
        if (!string.IsNullOrEmpty(format))
        {
            var time = format.Split("@");
            if (time.Length is 2)
            {
                if (filters.Any()) builder.AppendFormat("AND ");
                builder = builder.TimestampFormat(time[0], time[1]).AppendFormat($"ORDER BY {tag} DESC LIMIT {limit}");
            }
        }
        return builder.ToString();
    }
    public static StringBuilder TimestampFormat(this StringBuilder builder, in string startTime, in string endTime, in string tag = "create_time")
    {
        return builder.AppendFormat($"{tag} BETWEEN {Format(startTime)} AND {Format(endTime)} ");
        static string Format(string dateTime) => $"TO_TIMESTAMP('{dateTime}'::timestamp AT TIME ZONE 'UTC','yyyy-MM-dd HH24:MI:SS')";
    }
    public static StringBuilder AddTotalCount<T>(this string field) => new($"SELECT COUNT({field}) FROM {TableName<T>()} ");
    public static string UseDelete(this string name) => $"DELETE FROM {name} WHERE {CurrentSign} = @{CurrentSign}";
    public sealed class TableInfo
    {
        public string[] Uniques { get; set; } = Array.Empty<string>();
        public (string tag, List<string> fields)[] Combos { get; set; } = Array.Empty<(string tag, List<string> fields)>();
        public (string field, string table, string key)[] Foreigns { get; set; } = Array.Empty<(string field, string table, string key)>();
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class TableAttribute : Attribute
    {
        public required string Name { get; init; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldAttribute : Attribute
    {
        public bool PK { get; set; }
        public required string Name { get; init; }
    }
    public static string ConnectionString { get; set; } = string.Empty;
}