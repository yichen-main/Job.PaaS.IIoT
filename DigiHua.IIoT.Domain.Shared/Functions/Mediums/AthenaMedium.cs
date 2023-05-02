using Newtonsoft.Json;

namespace IIoT.Domain.Shared.Functions.Mediums;
public record struct AthenaMedium
{
    public const string Energy = "energies";
    public const string Execution = "execution";
    public const string Parameter = "parameter";
    public const string SteadyDesk = "athena-steady";
    public const string ChangeObjects = "change_objects";
    public enum DayType
    {
        [Description("E03")] Electricity = 1,
        [Description("W03")] Liquid = 2,
        [Description("G03")] Gas = 3
    }
    public enum HourType
    {
        [Description("E02")] Electricity = 1,
        [Description("W02")] Liquid = 2,
        [Description("G02")] Gas = 3
    }
    public record struct Result
    {
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("sql_code")] public string SqlCode { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }
    public readonly record struct Organization
    {
        [JsonProperty("org_type_company")] public IEnumerable<Company> Companies { get; init; }
        [JsonProperty("org_type_region")] public IEnumerable<Region> Regions { get; init; }
        [JsonProperty("org_type_site")] public IEnumerable<Site> Sites { get; init; }
        public readonly record struct Company
        {
            [JsonProperty("company_no")] public string CompanyNo { get; init; }
            [JsonProperty("company_name")] public string CompanyName { get; init; }
            [JsonProperty("org_type_site")] public IEnumerable<Site> Sites { get; init; }
        }
        public readonly record struct Region
        {
            [JsonProperty("region_no")] public string RegionNo { get; init; }
            [JsonProperty("region_name")] public string RegionName { get; init; }
            [JsonProperty("region_type")] public string RegionType { get; init; }
        }
        public readonly record struct Site
        {
            [JsonProperty("site_no")] public string SiteNo { get; init; }
            [JsonProperty("site_name")] public string SiteName { get; init; }
        }
    }
}