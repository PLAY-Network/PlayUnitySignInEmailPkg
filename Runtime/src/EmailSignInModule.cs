using System.Threading.Tasks;

namespace RGN.Modules.SignIn
{
    [Attributes.GeneratorExclude]
    public class EmailSignInModule : BaseModule<EmailSignInModule>, IRGNModule
    {
        public async void TryToSignIn()
        {
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
            
            RGNCore.I.Dependencies.WebForm.SignIn(OnSignInWebFormRedirect, idToken);
        }
        
        private async void OnSignInWebFormRedirect(bool cancelled, string token)
        {
            if (cancelled)
            {
                RGNCore.IInternal.SetAuthState(EnumLoginState.Error, EnumLoginResult.Cancelled);
                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Login cancelled");
                return;
            }

            _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Token received: " + token);

            if (string.IsNullOrEmpty(token))
            {
                RGNCore.IInternal.SetAuthState(EnumLoginState.Error, EnumLoginResult.Unknown);
            }
            else
            {
                RGNCore.IInternal.SignOutRGN(false);
                await _rgnCore.ReadyMasterAuth.SignInWithCustomTokenAsync(token);
            }
        }

        internal void TryToSignIn(string email, string password)
        {
            TryToSingInWithoutLink(email, password);
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
            RGNCore.IInternal.SignOutRGN();
        }

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

                string email = "not logged in";
                if (_rgnCore.MasterAppUser != null)
                {
                    email = _rgnCore.MasterAppUser.Email;
                }
                _rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Email/Password, the user successfully signed in: " + email);
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
