using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

namespace RGN.Modules.SignIn
{
    public class EmailSignInModule : BaseModule<EmailSignInModule>, IRGNModule
    {
        private IRGNRolesCore rgnCore;
        private RGNDeepLink _rgnDeepLink;

        public static void InitializeWindowsDeepLink()
        {
#if UNITY_STANDALONE_WIN
            if (WindowsDeepLinks.IsCustomUrlRegistered()) { return; }
            WindowsDeepLinks.StartHandling();
#endif
        }

        public void SetRGNCore(IRGNRolesCore rgnCore)
        {
            this.rgnCore = rgnCore;
        }
        public void Init()
        {
            _rgnDeepLink = new RGNDeepLink();
            _rgnDeepLink.Init(rgnCore);
            _rgnDeepLink.TokenReceived += OnTokenReceived;
        }
        public void Dispose()
        {
            if (_rgnDeepLink != null)
            {
                _rgnDeepLink.TokenReceived -= OnTokenReceived;
                _rgnDeepLink.Dispose();
                _rgnDeepLink = null;
            }
        }

        public void TryToSignIn()
        {
            _rgnDeepLink.OpenURL();
        }
        private async void OnTokenReceived(string token)
        {
            rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Token received: " + token);
            if (string.IsNullOrEmpty(token))
            {
                rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.Unknown);
            }
            else
            {
                await rgnCore.ReadyMasterAuth.SignInWithCustomTokenAsync(token);
                rgnCore.SetAuthCompletion(EnumLoginState.Success, EnumLoginError.Ok);
            }
        }

        public void TryToSignIn(string email, string password, bool tryToLinkToCurrentAccount = false)
        {
            if (tryToLinkToCurrentAccount)
            {
                TryToLink(email, password);
            }
            else
            {
                TryToSingInWithoutLink(email, password);
            }
        }
        public void SendPasswordResetEmail(string email)
        {
            rgnCore.ReadyMasterAuth.SendPasswordResetEmailAsync(email).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    rgnCore.Dependencies.Logger.LogError("[EmailSignInModule]: SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(rgnCore.Dependencies.Logger, task.Exception);
                    rgnCore.Dependencies.Logger.LogError("[EmailSignInModule]: SendPasswordResetEmailAsync encountered an error: " +
                                   task.Exception);
                    return;
                }

                SignOut();
                rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Password reset email sent successfully.");
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
        public void SignOut()
        {
            rgnCore.SignOutRGN();
        }

        private void TryToLink(string email, string password)
        {
            rgnCore.Dependencies.Logger.Log("EmailSignInModule]: TryToSignIn(" + email + ", " + string.IsNullOrEmpty(password) + ")");
            var credential = rgnCore.ReadyMasterAuth.emailAuthProvider.GetCredential(email, password);

            rgnCore.ReadyMasterAuth.CurrentUser.LinkAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    rgnCore.Dependencies.Logger.LogWarning("EmailSignInModule]: LinkAndRetrieveDataWithCredentialAsync was cancelled");
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(rgnCore.Dependencies.Logger, task.Exception);
                    FirebaseAccountLinkException firebaseAccountLinkException = task.Exception.InnerException as FirebaseAccountLinkException;
                    if (firebaseAccountLinkException != null && firebaseAccountLinkException.ErrorCode == (int)AuthError.CredentialAlreadyInUse)
                    {
                        rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.AccountAlreadyLinked);
                        return;
                    }

                    FirebaseException firebaseException = task.Exception.InnerException as FirebaseException;

                    if (firebaseException != null)
                    {
                        EnumLoginError loginError = (AuthError)firebaseException.ErrorCode switch {
                            AuthError.EmailAlreadyInUse => EnumLoginError.AccountAlreadyLinked,
                            AuthError.ProviderAlreadyLinked => EnumLoginError.AccountAlreadyLinked,
                            AuthError.RequiresRecentLogin => EnumLoginError.AccountNeedsRecentLogin,
                            _ => EnumLoginError.Unknown
                        };

                        rgnCore.SetAuthCompletion(EnumLoginState.Error, loginError);
                        return;
                    }

                    rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.Unknown);
                    return;
                }

                rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: LinkWith Email/Password Successful. " + rgnCore.ReadyMasterAuth.CurrentUser.UserId + " ");

                rgnCore.SetAuthCompletion(EnumLoginState.Success, EnumLoginError.Ok);
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
        private void TryToSingInWithoutLink(string email, string password)
        {
            rgnCore.ReadyMasterAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    rgnCore.Dependencies.Logger.LogWarning("EmailSignInModule]: SignInWithEmailAndPasswordAsync was cancelled");
                    SignOut();
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(rgnCore.Dependencies.Logger, task.Exception);
                    SignOut();
                    rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.Unknown);
                    return;
                }

                rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Email/Password, signed in");
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
