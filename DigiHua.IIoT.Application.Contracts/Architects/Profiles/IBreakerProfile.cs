namespace IIoT.Application.Contracts.Architects.Profiles;
public interface IBreakerProfile
{
    ValueTask BuildInstall();
    ValueTask BuildRemove();
    static string Name => nameof(Morse.DigiHua).Joint(nameof(IIoT)).Joint("Master");
    static string Banner => nameof(Morse.DigiHua).Joint(nameof(IIoT)).Joint("Station");
}