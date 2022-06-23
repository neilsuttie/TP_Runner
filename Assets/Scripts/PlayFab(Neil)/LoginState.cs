using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;

public class LoginState : AState
{
    [Header("Login Canvas")]
    public GameObject loginCanvas;

    [Header("Login UI")]
    public TMPro.TMP_InputField userNameInputField;
    public Button loginButton;
    public Button playGameButton;

    [Header("Error Output")]
    public TMPro.TMP_Text errorOutput;

    //User entered display name
    private string displayName = null;
    private bool HaveSetUserName = false;
 
    //Handles player authentication and login
    private PlayFabAuthService authService;
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    #region UnityEventFunctions
    private void Awake()
    {
        //We will just assume silent authentication for the sake of the Demo and auto login on Awake
        authService = new PlayFabAuthService();
        authService.InfoRequestParams = InfoRequestParams;
        PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
        authService.Authenticate();
    }
    #endregion

    #region PlayFabAuthenticationEvents
    private void OnLoginSuccess(LoginResult success)
    {
        Debug.LogFormat("Player {0} Authenticated Successfully", success.PlayFabId);
        errorOutput.text = "Player Authenticated with server. Loading player Profile";

        //Create in PlayerData to detect new users
        PlayerData.IsNewUser = success.NewlyCreated;
        PlayerData.LoginUserData = success.InfoResultPayload.UserData;
        MusicPlayer.StartMusicSystem();
        
        //Load the user account details and data
        PlayFabManager.IsLoggedIn = true;
        PlayFabManager.LoadUserData();
        PlayFabManager.LoadAccountData();
        PlayFabManager.LoadTitleData();
     }

    private void OnDisplayAuthentication()
    {
        authService.Authenticate(Authtypes.Silent);
    }
    #endregion

    #region GameStateInterfaceFunctions
    public override void Enter(AState from)
    {
        loginCanvas.gameObject.SetActive(true);
        //Input field
        userNameInputField.enabled = true;
        //Keep the field deaactived until we fail to load a user account.
        userNameInputField.DeactivateInputField();
        
        //Disable login button until data is loaded
        loginButton.enabled = false;
 
        playGameButton.gameObject.SetActive(false);
    }

    public override void Exit(AState to)
    {
        loginCanvas.gameObject.SetActive(false);
        loginButton.enabled = false;
        loginButton.gameObject.SetActive(false);

        userNameInputField.DeactivateInputField();
        userNameInputField.enabled = false;

        playGameButton.enabled = false;
        playGameButton.gameObject.SetActive(false);
    }

    public override string GetName()
    {
        return "Login";
    }

    public override void Tick()
    {
        //If we don't have a name yet keep trying to load the accountInfo
        if(!HaveSetUserName)
            AttemptToSyncDisplayName();
    }
    #endregion

    #region NavigateGameStates
    public void GoToLoadOut()
    {
        GameManager.instance.SwitchState("Loadout");
    }
    #endregion

    #region DisplayNameHandlers

    private void AttemptToSyncDisplayName()
    {
        if (PlayFabManager.IsLoggedIn && PlayFabManager.IsAccountInfoLoaded)
        {
            if (!string.IsNullOrEmpty(PlayFabManager.UserDisplayName))
            {
                //If we manage to fetch a display name for the account update all fields and display a welcome message
                displayName = PlayFabManager.UserDisplayName;
                userNameInputField.text = displayName;
                errorOutput.text = $"Welcome back {displayName} press the Play button to continue";

                //Swap to the play button
                loginButton.gameObject.SetActive(false);

                playGameButton.enabled = true;
                playGameButton.gameObject.SetActive(true);

            }
            else
            {
                //otherwise prompt the user to pick a name
                userNameInputField.text = "Enter a username...";
                errorOutput.text = "New user account. Please enter a name to register";
                //Enable text field for player input
                userNameInputField.interactable = true;
            }

            HaveSetUserName = true;
        }
    }
    public void OnUserNameFieldChange(string displayName)
    {
        this.displayName = displayName;
        //Once the player has begun typing activate the login button. Assume in the real world we'd want some validation on the name
        loginButton.enabled = true;
     }

    public void CommitUserNameChange()
    {
        //Update the player name in the local player settings and on the back-end.
        PlayFabManager.SetUserDisplayName(displayName);
        //Set the name used in the local file
        PlayerData.instance.previousName = displayName;
        //Disable changes once locked in.
        userNameInputField.interactable = false;

        //Assume sucess for now
        errorOutput.text = $"Welcome {displayName} press the Play button to continue";

        //Show play button to allow the player to continue.
        loginButton.gameObject.SetActive(false);

        playGameButton.enabled = true;
        playGameButton.gameObject.SetActive(true);
    }
    #endregion
}
