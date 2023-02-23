using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.SignIn;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    internal sealed class SignInUpExample : IInitializable
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _tryToSignInButton;
        [SerializeField] private Button _signOutButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private TextMeshProUGUI _userInfoText;

        public override Task InitAsync()
        {
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

        private void OnBackButtonClick()
        {
            Debug.Log("OnBackButtonClick");
        }
        private void OnTryToSignInButtonClick()
        {
            _canvasGroup.interactable = false;
            EmailSignInModule.I.TryToSignIn();
        }
        private void OnSignOutButtonClick()
        {
            EmailSignInModule.I.SignOut();
        }
        private void OnAuthStateChanged(EnumLoginState state, EnumLoginError error)
        {
            _canvasGroup.interactable = true;
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
