using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginState : AState
{
    [Header("Login Canvas")]
    public GameObject loginCanvas;

    [Header("Login UI")]
    public TMPro.TMP_InputField userNameInputField;
    public Button loginButton;

    [Header("AFab Player Login")]
    private PlayFabLoginHandler loginHandler;

    //Handler controls account registrations and login
    private PlayFabLoginHandler playFabLoginHandler;

    //Player Login details
    private string userName;

    private void Awake()
    {
        playFabLoginHandler = new PlayFabLoginHandler();
    }

    public override void Enter(AState from)
    {
        loginCanvas.gameObject.SetActive(true);
        loginButton.enabled = true;
        userNameInputField.enabled = true;
    }

    public override void Exit(AState to)
    {
        loginButton.enabled = false;
        userNameInputField.enabled = false;
        loginCanvas.gameObject.SetActive(false);
    }

    public override string GetName()
    {
        return "Login";
    }

    public override void Tick()
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserNameFieldChange(string userName)
    {
        this.userName = userName;
        Debug.Log(userName);
    }

    public void RegisterAndLogin()
    {
        //Handle the registration and login
        if(!string.IsNullOrEmpty(userName))
            playFabLoginHandler.LoginWithCustomID(userName);

    }

    public void GoToLoadOut()
    {
        manager.SwitchState("Loadout");
    }
}
