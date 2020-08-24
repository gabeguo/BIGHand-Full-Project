using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginController : MonoBehaviour
{
    //input fields used to identify the user
    public TextMeshProUGUI usernameInput, passwordInput;
    public static bool repExists;

    private void Start()
    {
        if (!repExists)
        {
            //_GlobalVariables.dataRep = new LocalDatabaseRepository();
            _GlobalVariables.dataRep = new SqliteDatabaseRepository();
            repExists = true;
        }
    }

    //attempts a login with the current form credentials
    public void LogUserIn()
    {
        string nameText = usernameInput.text;
        if (!_GlobalVariables.dataRep.SignUserIn(usernameInput.text, passwordInput.text))
        {
            OutlineInputs();
            return;
        }

        _GlobalVariables.userId = _GlobalVariables.dataRep.GetCurrentUser().GetUserName();
        SceneManager.LoadScene("GripCalibration");
    }

    private void OutlineInputs()
    {
        usernameInput.outlineWidth = 0.1f;
        usernameInput.outlineColor = Color.red;
        passwordInput.outlineWidth = 0.1f;
        passwordInput.outlineColor = Color.red;
    }

    public void GoToCreationScreen()
    {
        SceneManager.LoadScene("CreateUserScene");
    }
}
