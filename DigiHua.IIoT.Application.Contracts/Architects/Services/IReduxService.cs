namespace IIoT.Application.Contracts.Architects.Services;
public interface IReduxService
{
    ValueTask InitialAsync();
    ValueTask AddCommercialAsync(IBusinessFoundationWrapper wrapper);
    ValueTask AddCommercialAsync(IBusinessManufactureWrapper wrapper);
    Task<IUser.Entity> UserInfoAsync(ClaimsPrincipal principal);
    IEnumerable<Guid> ConvertGuid(in string text, in char symbol = ',');
    string MakeUniqueIndexTag(in string table, in string field);
    string MakeForeignKeyIndexTag(in string table, in string field);
    void CheckProhibitSign(in string text, in string fieldName);
    void CheckIpLocation(in string ip, in string fieldName);
    void CheckHttpUrl(in string url, in string fieldName);
    void CheckOpcUaUrl(in string url, in string fieldName);
    void AddPage<T>(in string name, in IUrlHelper helper, in IHeaderDictionary dictionaries, in Pages<T> pages, in Mark mark);
    int DownPage(in int count);
    int UpperPage(in int count);
    enum Domain
    {
        Application,
        Interface
    }
    readonly ref struct Head
    {
        public const string Language = "Accept-Language";
        public const string Pagination = "X-Pagination";
        public const string FrameOption = "X-Frame-Options";
        public const string Authorization = "Authorization";
        public const string AllowOrigin = "Access-Control-Allow-Origin";
        public const string AllowHeader = "Access-Control-Allow-Headers";
        public const string AllowMethod = "Access-Control-Allow-Methods";
        public const string ExposeHeader = "Access-Control-Expose-Headers";
        public const string AllowCredential = "Access-Control-Allow-Credentials";
    }
    readonly record struct Enumerate
    {
        public required int TypeNo { get; init; }
        public required string TypeName { get; init; }
    }
    public readonly record struct ProblemResult
    {
        public required string Message { get; init; }
    }
    readonly record struct Mark
    {
        public const int Found = 1;
        public const int MaxData = 10;
        public const int MaxSize = 500;
        public required object PreviousPage { get; init; }
        public required object NextPage { get; init; }
        public required object FirstPage { get; init; }
        public required object LastPage { get; init; }
    }
    abstract class Satchel
    {
        int Page = Mark.MaxData;
        public int PageSize
        {
            get => Page;
            set => Page = value > Mark.MaxSize ? Mark.MaxSize : value;
        }
        public int PageNumber { get; init; } = Mark.Found;
        public string? Search { get; init; }
    }
    sealed class Pages<T> : List<T>
    {
        public Pages(in IEnumerable<T> sources, in int currentPage, in int pageSize)
        {
            PageSize = pageSize;
            TotalCount = sources.Count();
            var totalPage = (TotalCount / PageSize) + (TotalCount % PageSize == default ? default : Mark.Found);
            var page = totalPage > Mark.MaxSize ? Mark.MaxSize : totalPage;
            {
                CurrentPage = currentPage;
                TotalPage = page == default ? Mark.Found : page;
                AddRange(sources.Skip((CurrentPage - Mark.Found) * PageSize).Take(PageSize));
            }
        }
        public int PageSize { get; private set; }
        public int TotalPage { get; private set; }
        public int TotalCount { get; private set; }
        public int CurrentPage { get; private set; }
    }
    MqttServer Transport { get; init; }
}