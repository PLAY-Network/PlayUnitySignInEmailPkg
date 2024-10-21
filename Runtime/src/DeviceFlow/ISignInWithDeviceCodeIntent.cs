using System.Threading;
using System.Threading.Tasks;

namespace RGN.Modules.SignIn.DeviceFlow
{
    public interface ISignInWithDeviceCodeIntent
    {
        Task<bool> ContinueInBrowserAsync(CancellationToken cancellationToken = default);
    }
}
