using System;
using System.Threading;
using System.Threading.Tasks;
using RGN.ImplDependencies.Core.Auth;
using RGN.ImplDependencies.Engine;
using RGN.ImplDependencies.WebForm;

namespace RGN.Modules.SignIn.DeviceFlow
{
    public class SignInWithDeviceCodeIntent : ISignInWithDeviceCodeIntent
    {
        private readonly IRGNCore _rgnCore;
        private readonly Action<string> _openUrlAction;

        private string _deviceCode;
        private bool _deviceCodeRequesting;

        private const float POLLING_INTERVAL_SEC = 3f;

        public SignInWithDeviceCodeIntent(IRGNCore rgnCore, Action<string> openUrlAction)
        {
            _rgnCore = rgnCore;
            _openUrlAction = openUrlAction;
        }

        public async Task RequestDeviceCodeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _deviceCodeRequesting = true;
                _deviceCode = (await _rgnCore.Dependencies.ReadyMasterAuth
                    .RequestOAuthDeviceCodeAsync(cancellationToken)).deviceCode;
            }
            finally
            {
                _deviceCodeRequesting = false;
            }
        }

        public async Task<bool> ContinueInBrowserAsync(CancellationToken cancellationToken = default)
        {
            while (_deviceCodeRequesting)
            {
                await Task.Yield();
            }

            if (string.IsNullOrEmpty(_deviceCode))
            {
                await RequestDeviceCodeAsync(cancellationToken);
            }

            IWebForm webFormService = _rgnCore.Dependencies.WebForm;
            ITime timeService = _rgnCore.Dependencies.Time;

            if (_rgnCore is RGNCore rgnCore)
            {
                rgnCore.SetAuthState(EnumLoginState.Processing, EnumLoginResult.None);
            }

            string idToken = _rgnCore.MasterAppUser != null
                ? await _rgnCore.MasterAppUser.TokenAsync(false, cancellationToken)
                : string.Empty;

            webFormService.SignInWithDeviceCode(_deviceCode, idToken, _openUrlAction);

            do
            {
                PollTokenWithDeviceCodeResponse pollResponse = await _rgnCore.Dependencies.ReadyMasterAuth
                    .PollTokenWithDeviceCodeAsync(_deviceCode, cancellationToken);

                switch (pollResponse.status)
                {
                    case "completed":
                        IUserTokensPair userTokens = await _rgnCore.Dependencies.ReadyMasterAuth
                            .RefreshTokensAsync(pollResponse.token, cancellationToken);
                        _rgnCore.Dependencies.ReadyMasterAuth
                            .SetUserTokens(userTokens.IdToken, userTokens.RefreshToken);
                        return true;
                    case "expired":
                        return false;
                }

                float delayStartTime = timeService.time;
                while (timeService.time < delayStartTime + POLLING_INTERVAL_SEC)
                {
                    await Task.Yield();
                }
            }
            while (true);
        }
    }
}
