using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLoginHandler 
{
    public void OnSubmitLogin(string userName)
    {
        Debug.Log("Submitting login..." + userName);
        LoginWithCustomID(userName);
    }

    public void RegisterWithEmailRequest(string userName)
    {
        //TODO:Fails. Update if required.
        var registerRequest = new RegisterPlayFabUserRequest { Email = "neilsuttie@gmail.com", Password = "HelloWorld",  Username = userName };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, RegisterSuccess, RegisterFailure);
    }

    public void LoginWithCustomID(string customID)
    {
        var request = new LoginWithCustomIDRequest { CreateAccount = true, CustomId = customID };
        PlayFabClientAPI.LoginWithCustomID(request, LoginSuccess, LoginError);

    }
    public void LoginWithEmailRequest()
    {
        var request = new LoginWithEmailAddressRequest { Email = "neilsuttie@gmail.com", Password = "HelloWord" };
    }

    public void LoginSuccess(LoginResult result)
    {
        Debug.Log("Success");
    }

    public void LoginError(PlayFabError error)
    {
        Debug.Log("Login failed " + error.ErrorMessage);
    }


    public void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Success");
    }

    public void RegisterFailure(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage + " Failure");
    }
}
