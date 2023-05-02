namespace IIoT.Domain.Businesses.Manages.Atoms;
internal sealed class Atom : TacticExpert, IAtom
{
    public Task<bool> IsExistAsync(string name) => ExistDatabaseAsync(name);
}