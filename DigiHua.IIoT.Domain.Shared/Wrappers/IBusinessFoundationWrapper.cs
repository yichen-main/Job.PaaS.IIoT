namespace IIoT.Domain.Shared.Wrappers;
public interface IBusinessFoundationWrapper
{
    IAtom Atom { get; }
    IUser User { get; }
    IUserVerification UserVerification { get; }
}