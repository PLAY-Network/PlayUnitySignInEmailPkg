using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.SignIn;
using RGN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    public sealed class SignInUpExample : IUIScreen
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _tryToSignInButton;
        [SerializeField] private Button _signOutButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _loadingIndicator;

        [SerializeField] private TextMeshProUGUI _userInfoText;

        public override Task InitAsync(IRGNFrame rgnFrame)
        {
            base.InitAsync(rgnFrame);
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            _backButton.gameObject.SetActive(false);
            _backButton.onClick.AddListener(OnBackButtonClick);
            _tryToSignInButton.onClick.AddListener(OnTryToSignInButtonClick);
            _signOutButton.onClick.AddListener(OnSignOutButtonClick);
            UpdateUserInfoText();
            RGNCore.I.AuthenticationChanged += OnAuthStateChanged;
            return Task.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClick);
            _tryToSignInButton.onClick.RemoveListener(OnTryToSignInButtonClick);
            _signOutButton.onClick.RemoveListener(OnSignOutButtonClick);
            RGNCore.I.AuthenticationChanged -= OnAuthStateChanged;
        }
        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            _backButton.gameObject.SetActive(true);
        }

        private void OnTryToSignInButtonClick()
        {
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            EmailSignInModule.I.TryToSignIn();
        }
        private void OnSignOutButtonClick()
        {
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            EmailSignInModule.I.SignOut();
        }
        private void OnAuthStateChanged(EnumLoginState state, EnumLoginError error)
        {
            switch (state)
            {
                case EnumLoginState.NotLoggedIn:
                    ToastMessage.I.Show("Not Logged In");
                    break;
                case EnumLoginState.Success:
                    string messageSuffix = string.Empty;
                    if (RGNCore.I.AuthorizedProviders == EnumAuthProvider.Guest)
                    {
                        messageSuffix = " As Guest";
                    }
                    else if (RGNCore.I.AuthorizedProviders == EnumAuthProvider.Email)
                    {
                        messageSuffix = " with " + RGNCore.I.MasterAppUser.Email;
                    }
                    ToastMessage.I.ShowSuccess("Successfully Logged In" + messageSuffix);
                    _canvasGroup.interactable = true;
                    _loadingIndicator.SetEnabled(false);
                    break;
                case EnumLoginState.Error:
                    ToastMessage.I.ShowError("Login Error: " + error);
                    break;
            };
            UpdateUserInfoText();
        }
        private void UpdateUserInfoText()
        {
            if (RGNCore.I.MasterAppUser == null)
            {
                _userInfoText.text = "User is not logged in";
                return;
            }
            var user = RGNCore.I.MasterAppUser;
            var sb = new System.Text.StringBuilder();
            sb.Append("Email: ").AppendLine(user.Email);
            sb.Append("Id: ").AppendLine(user.UserId);
            sb.Append("AuthorizedProviders: ").AppendLine(RGNCore.I.AuthorizedProviders.ToString());
            _userInfoText.text = sb.ToString();
        }
    }
}
