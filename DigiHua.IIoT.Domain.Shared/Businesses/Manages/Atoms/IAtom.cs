namespace IIoT.Domain.Shared.Businesses.Manages.Atoms;
public interface IAtom : ITacticExpert
{
    Task<bool> IsExistAsync(string name);
}