using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

namespace RGN.Modules.SignIn
{
    public class EmailSignInModule : BaseModule<EmailSignInModule>, IRGNModule
    {
        private IRGNRolesCore rgnCore;

        public void SetRGNCore(IRGNRolesCore rgnCore)
        {
            this.rgnCore = rgnCore;
        }
        public void Init() { }
        public void Dispose() { }

        public void OnSignUpWithEmail(string email, string password)
        {
            rgnCore.Dependencies.Logger.Log("EmailSignInModule]: OnSignUpWithEmail(" + email + ", " + string.IsNullOrEmpty(password) + ")");
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
                    if (firebaseException != null && firebaseException.ErrorCode == (int)AuthError.EmailAlreadyInUse)
                    {
                        rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.AccountAlreadyLinked);
                        return;
                    }

                    if (firebaseException != null && firebaseException.ErrorCode == (int)AuthError.ProviderAlreadyLinked)
                    {
                        rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.AccountAlreadyLinked);
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

        public void OnSignInWithEmail(string email, string password)
        {
            rgnCore.ReadyMasterAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    rgnCore.Dependencies.Logger.LogWarning("EmailSignInModule]: SignInWithEmailAndPasswordAsync was cancelled");
                    SignOutFromEmail();
                    return;
                }

                if (task.IsFaulted)
                {
                    Utility.ExceptionHelper.PrintToLog(rgnCore.Dependencies.Logger, task.Exception);
                    SignOutFromEmail();
                    rgnCore.SetAuthCompletion(EnumLoginState.Error, EnumLoginError.Unknown);
                    return;
                }

                rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Email/Password, signed in");
            },
            TaskScheduler.FromCurrentSynchronizationContext());
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

                SignOutFromEmail();
                rgnCore.Dependencies.Logger.Log("[EmailSignInModule]: Password reset email sent successfully.");
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void SignOutFromEmail()
        {
            rgnCore.SignOutRGN();
        }
    }
}
