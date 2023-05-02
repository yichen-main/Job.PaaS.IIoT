using static IIoT.Domain.Shared.Businesses.Manages.Users.IUser;

namespace IIoT.Station.Apis.Edifices.Foundations;

[Authorize(), EnableCors, ApiExplorerSettings(GroupName = nameof(IReduxService.Domain.Interface))]
public class Users : ControllerBase
{
    [HttpGet(Name = nameof(UpperUserFoundationAsync))]
    public async ValueTask<IActionResult> UpperUserFoundationAsync([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entities = await BusinessFoundation.User.ListAsync();
                var uppers = entities.Select(item => new Upper
                {
                    Id = item.Id,
                    GroupType = item.GroupType,
                    GroupTransl = Terminology[item.GroupType.ToString()],
                    LicenseType = item.LicenseType,
                    LicenseTransl = Terminology[item.LicenseType.ToString()],
                    OperateType = item.OperateType,
                    OperateTransl = Terminology[item.OperateType.ToString()],
                    Username = item.Username,
                    Account = item.Account,
                    Creator = item.Creator,
                    CreateTime = item.CreateTime
                });
                if (!string.IsNullOrEmpty(query.Search)) uppers = uppers.Where(item => new[]
                {
                    item.Username,
                    item.Account
                }.Any(item => item.Contains(query.Search)));
                Pages<Upper> results = new(uppers, query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(UpperUserFoundationAsync), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        results.PageSize,
                        query.Search
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        results.PageSize,
                        query.Search
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        results.PageSize,
                        query.Search
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        results.PageSize,
                        query.Search
                    }
                });
                return Ok(results);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpGet("informations", Name = nameof(GetUserFoundationAsync))]
    public async ValueTask<IActionResult> GetUserFoundationAsync(Guid id, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var entity = await ReduxService.UserInfoAsync(User);
                return Ok(new Row
                {
                    Id = entity.Id,
                    GroupType = entity.GroupType,
                    GroupTransl = Terminology[entity.GroupType.ToString()],
                    LicenseType = entity.LicenseType,
                    LicenseTransl = Terminology[entity.LicenseType.ToString()],
                    OperateType = entity.OperateType,
                    OperateTransl = Terminology[entity.OperateType.ToString()],
                    Username = entity.Username,
                    Account = entity.Account,
                    Creator = entity.Creator,
                    CreateTime = entity.CreateTime
                });
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPost(Name = nameof(InsertUserFoundationAsync))]
    public async ValueTask<IActionResult> InsertUserFoundationAsync([FromHeader] Header header, [FromBody] UserInsert body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var id = Guid.NewGuid();
                await BusinessFoundation.User.AddAsync(new Entity
                {
                    Id = id,
                    GroupType = body.GroupType,
                    LicenseType = body.LicenseType,
                    Username = body.Username,
                    Account = body.Account,
                    Password = FoundationTrigger.UseEncryptAES(body.Password),
                    OperateType = body.OperateType,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                }, Morse.Passer);
                return CreatedAtRoute(nameof(GetUserFoundationAsync), new
                {
                    id
                }, default);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.Account).To<Entity>())) =>
                        Fielder["db.repeat.input.information", nameof(Entity.Account)],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpPut(Name = nameof(UpdateUserFoundationAsync))]
    public async ValueTask<IActionResult> UpdateUserFoundationAsync([FromHeader] Header header, [FromBody] UserUpdate body)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                await BusinessFoundation.User.UpdateAsync(new()
                {
                    Id = body.Id,
                    GroupType = body.GroupType,
                    LicenseType = body.LicenseType,
                    Username = body.Username,
                    Account = string.Empty,
                    Password = FoundationTrigger.UseEncryptAES(body.Password),
                    OperateType = body.OperateType,
                    Creator = body.Creator,
                    CreateTime = DateTime.UtcNow
                });
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        var item when item.Contains(ReduxService.MakeUniqueIndexTag(TableName<Entity>(), nameof(Entity.Account).To<Entity>())) =>
                        Fielder["db.repeat.input.information", nameof(Entity.Account)],
                        _ => e.Message
                    }
                });
            }
        }
    }

    [AllowAnonymous, HttpPost("authentications", Name = nameof(AuthenticationAsync))]
    public async ValueTask<IActionResult> AuthenticationAsync([FromForm] VerifyInsert body)
    {
        try
        {
            var result = await AuthenticateService.LoginAsync(body.Account, body.Password);
            if (result is not null)
            {
                return Ok(new VerifyRow
                {
                    AccessToken = result
                });
            }
            return Unauthorized();
        }
        catch (Exception e)
        {
            return NotFound(new ProblemResult
            {
                Message = e.Message switch
                {
                    _ => e.Message
                }
            });
        }
    }

    [HttpDelete($$"""{{{nameof(identifier)}}}""", Name = nameof(DeleteUserFoundationAsync))]
    public async ValueTask<IActionResult> DeleteUserFoundationAsync(string identifier, [FromHeader] Header header)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                var ids = ReduxService.ConvertGuid(identifier);
                if (ids.Any()) await ClearerEvent.UserAsync(ids);
                return NoContent();
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpGet("group-types", Name = nameof(ListUserFoundationGroupType))]
    public IActionResult ListUserFoundationGroupType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(IUser.Group))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(IUser.Group), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListUserFoundationGroupType), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        pageSize = results.PageSize
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        pageSize = results.PageSize
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        pageSize = results.PageSize
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        pageSize = results.PageSize
                    }
                });
                return Ok(results);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }

    [HttpGet("license-types", Name = nameof(ListUserFoundationLicenseType))]
    public IActionResult ListUserFoundationLicenseType([FromHeader] Header header, [FromQuery] Query query)
    {
        using (CultureHelper.Use(header.Language ?? RunnerText.Organization.Language))
        {
            try
            {
                List<Enumerate> groups = new();
                foreach (int item in Enum.GetValues(typeof(License))) groups.Add(new()
                {
                    TypeNo = item,
                    TypeName = Terminology[Enum.GetName(typeof(License), item) ?? string.Empty]
                });
                Pages<Enumerate> results = new(groups.OrderBy(item => item.TypeNo), query.PageNumber, query.PageSize);
                ReduxService.AddPage(nameof(ListUserFoundationLicenseType), Url, Response.Headers, results, new()
                {
                    PreviousPage = new
                    {
                        pageNumber = ReduxService.UpperPage(results.CurrentPage),
                        pageSize = results.PageSize
                    },
                    NextPage = new
                    {
                        pageNumber = ReduxService.DownPage(results.CurrentPage),
                        pageSize = results.PageSize
                    },
                    FirstPage = new
                    {
                        pageNumber = Mark.Found,
                        pageSize = results.PageSize
                    },
                    LastPage = new
                    {
                        pageNumber = results.TotalPage,
                        pageSize = results.PageSize
                    }
                });
                return Ok(results);
            }
            catch (Exception e)
            {
                return NotFound(new ProblemResult
                {
                    Message = e.Message switch
                    {
                        _ => e.Message
                    }
                });
            }
        }
    }
    public sealed class Query : Satchel { }
    public readonly record struct Upper
    {
        public required Guid Id { get; init; }
        public required IUser.Group GroupType { get; init; }
        public required string GroupTransl { get; init; }
        public required License LicenseType { get; init; }
        public required string LicenseTransl { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string Username { get; init; }
        public required string Account { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public readonly record struct Row
    {
        public required Guid Id { get; init; }
        public required IUser.Group GroupType { get; init; }
        public required string GroupTransl { get; init; }
        public required License LicenseType { get; init; }
        public required string LicenseTransl { get; init; }
        public required Operate OperateType { get; init; }
        public required string OperateTransl { get; init; }
        public required string Username { get; init; }
        public required string Account { get; init; }
        public required string Creator { get; init; }
        public required DateTime CreateTime { get; init; }
    }
    public sealed class UserInsert
    {
        public required IUser.Group GroupType { get; init; }
        public required License LicenseType { get; init; }
        public required Operate OperateType { get; init; }
        public required string Username { get; init; }
        public required string Account { get; init; }
        public required string Password { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<UserInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.GroupType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(GroupType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(GroupType)]);
                    RuleFor(item => item.LicenseType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(LicenseType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(LicenseType)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.Username).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Username)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(Username), ProhibitSign]);
                    RuleFor(item => item.Account).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Account)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(Account), ProhibitSign]);
                    RuleFor(item => item.Password).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Password)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class UserUpdate
    {
        public required Guid Id { get; init; }
        public required IUser.Group GroupType { get; init; }
        public required License LicenseType { get; init; }
        public required Operate OperateType { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string Creator { get; init; }
        public sealed class Validator : AbstractValidator<UserInsert>
        {
            public Validator(IStringLocalizer<Fielder> localizer)
            {
                using (CultureHelper.Use(RunnerText.Organization.Language))
                {
                    RuleFor(item => item.GroupType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(GroupType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(GroupType)]);
                    RuleFor(item => item.LicenseType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(LicenseType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(LicenseType)]);
                    RuleFor(item => item.OperateType).NotEmpty().WithMessage(localizer["field.cannot.be.default", nameof(OperateType)])
                        .IsInEnum().WithMessage(localizer["field.invalid.enumeration.value", nameof(OperateType)]);
                    RuleFor(item => item.Username).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Username)])
                        .Must(item => string.IsNullOrWhiteSpace(item) || !item.Contains(ProhibitSign)).WithMessage(localizer["field.with.prohibition.sign", nameof(Username), ProhibitSign]);
                    RuleFor(item => item.Password).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Password)]);
                    RuleFor(item => item.Creator).NotEmpty().WithMessage(localizer["field.cannot.be.empty", nameof(Creator)]);
                }
            }
        }
    }
    public sealed class VerifyInsert
    {
        public required string Account { get; init; }
        public required string Password { get; init; }
    }
    public readonly record struct VerifyRow
    {
        public required string AccessToken { get; init; }
    }
    public required IStringLocalizer<Fielder> Fielder { get; init; }
    public required IStringLocalizer<Terminology> Terminology { get; init; }
    public required IClearerEvent ClearerEvent { get; init; }
    public required IReduxService ReduxService { get; init; }
    public required IRegisterTrigger RegisterTrigger { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
    public required IAuthenticateService AuthenticateService { get; init; }
    public required IBusinessFoundationWrapper BusinessFoundation { get; init; }
}