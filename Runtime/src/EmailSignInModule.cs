using System;
using System.Threading;
using System.Threading.Tasks;
using RGN.ImplDependencies.Core.Auth;
using RGN.Modules.SignIn.DeviceFlow;
using RGN.Modules.SignIn.Models;

namespace RGN.Modules.SignIn
{
    [Attributes.GeneratorExclude]
    public class EmailSignInModule : BaseModule<EmailSignInModule>, IRGNModule
    {
        private Func<CancellationToken, Task<ISignInWithDeviceCodeIntent>> _signInWithDeviceCodePatch;

        public async void TryToSignIn(RGNEmailSignInCallbackDelegate callback = null)
        {
            LogAnalyticsEventAsync("in_game_login_attempt");
            if (_rgnCore.AuthorizedProviders.HasFlag(EnumAuthProvider.Email))
            {
                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Already logged in with email");
                RGNCore.IInternal.SetAuthState(EnumLoginState.Success, EnumLoginResult.Ok);
                return;
            }

            RGNCore.IInternal.SetAuthState(EnumLoginState.Processing, EnumLoginResult.None);

            string idToken = string.Empty;
            if (_rgnCore.MasterAppUser != null)
            {
                idToken = await _rgnCore.MasterAppUser.TokenAsync(false);
            }
            
            RGNCore.I.Dependencies.WebForm.SignIn(OnEmailSignInWebFormRedirectCallback, idToken);

            async void OnEmailSignInWebFormRedirectCallback(bool cancelled, string refreshToken)
            {
                if (cancelled)
                {
                    RGNCore.IInternal.SetAuthState(EnumLoginState.Error, EnumLoginResult.Cancelled);
                    callback?.Invoke(false);
                    _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Login cancelled");
                    return;
                }

                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Refresh token received: " + refreshToken);

                if (string.IsNullOrEmpty(refreshToken))
                {
                    RGNCore.IInternal.SetAuthState(EnumLoginState.Error, EnumLoginResult.Unknown);
                    callback?.Invoke(false);
                }
                else
                {
                    IUserTokensPair userTokensPair = await _rgnCore.ReadyMasterAuth.RefreshTokensAsync(refreshToken);
                    _rgnCore.ReadyMasterAuth.SetUserTokens(userTokensPair.IdToken, userTokensPair.RefreshToken);
                    callback?.Invoke(true);
                }
            }
        }

        public async Task<ISignInWithDeviceCodeIntent> SignInWithDeviceCodeAsync(CancellationToken cancellationToken = default)
        {
            if (_signInWithDeviceCodePatch != null)
            {
                return await _signInWithDeviceCodePatch(cancellationToken);
            }

            SignInWithDeviceCodeIntent signInWithDeviceCodeIntent = new SignInWithDeviceCodeIntent(_rgnCore);
#if UNITY_WEBGL
            signInWithDeviceCodeIntent.SetImmediateMode(true);
#endif
            await signInWithDeviceCodeIntent.RequestDeviceCodeAsync(cancellationToken);
            return signInWithDeviceCodeIntent;
        }

        public void SendPasswordResetEmail(string email)
        {
            _rgnCore.ReadyMasterAuth.SendPasswordResetEmailAsync(email).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    _rgnCore.Dependencies.Logger.LogError("[EmailSignInModule]: SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(_rgnCore.Dependencies.Logger, task.Exception);
                    _rgnCore.Dependencies.Logger.LogError("[EmailSignInModule]: SendPasswordResetEmailAsync encountered an error: " +
                                   task.Exception);
                    return;
                }

                SignOut();
                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Password reset email sent successfully.");
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void SignOut()
        {
            LogAnalyticsEventAsync("in_game_log_out");
            RGNCore.IInternal.SignOut();
        }

        internal void TryToSignIn(string email, string password)
        {
            TryToSingInWithoutLink(email, password);
        }

        internal void PatchSignInWithDeviceCodeFunction(Func<CancellationToken, Task<ISignInWithDeviceCodeIntent>> patchFunc = null)
            => _signInWithDeviceCodePatch = patchFunc;

        private void TryToSingInWithoutLink(string email, string password)
        {
            _rgnCore.ReadyMasterAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    _rgnCore.Dependencies.Logger.LogWarning("EmailSignInModule]: SignInWithEmailAndPasswordAsync was cancelled");
                    SignOut();
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(_rgnCore.Dependencies.Logger, task.Exception);
                    SignOut();
                    RGNCore.IInternal.SetAuthState(EnumLoginState.Error, EnumLoginResult.Unknown);
                    return;
                }
                
                if (_rgnCore.MasterAppUser != null)
                {
                    email = _rgnCore.MasterAppUser.Email;
                }
                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Email/Password, the user successfully signed in: " + email);
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void LogAnalyticsEventAsync(string eventName)
        {
            if (_rgnCore.Dependencies.RGNAnalytics != null)
            {
                const string ANALYTIC_EVENTS_PARAMETERS = "{\"providers\": \"Email\"}";
                await _rgnCore.Dependencies.RGNAnalytics.LogEventAsync(eventName, ANALYTIC_EVENTS_PARAMETERS);
            }
            else
            {
                _rgnCore.Dependencies.Logger.Log($"Can not log '{eventName}' analytics event. RGN Analytics is not installed");
            }
        }
    }
}
