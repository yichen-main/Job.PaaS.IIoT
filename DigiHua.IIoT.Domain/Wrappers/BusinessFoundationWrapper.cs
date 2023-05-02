using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace IIoT.Domain.Wrappers;

[Dependency(ServiceLifetime.Singleton)]
file sealed class BusinessFoundationWrapper : IBusinessFoundationWrapper
{
    public IAtom Atom => new Atom();
    public IUser User => new Businesses.Manages.Users.User(NpgsqlUtility, FoundationTrigger);
    public IUserVerification UserVerification => new UserVerification(NpgsqlUtility);
    public required INpgsqlUtility NpgsqlUtility { get; init; }
    public required IFoundationTrigger FoundationTrigger { get; init; }
}