using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CreateUserController : MonoBehaviour
{
    public TextMeshProUGUI username, password, confirmation;

    public void CreateUser()
    {
        if (!password.text.Equals(confirmation.text))
            OutlineInputs();

        if (!_GlobalVariables.dataRep.CreateUser(username.text, password.text))
            OutlineInputs();
        else
            SceneManager.LoadScene("Login");
    }

    private void OutlineInputs()
    {
        username.outlineWidth = 0.1f;
        username.outlineColor = Color.red;
        password.outlineWidth = 0.1f;
        password.outlineColor = Color.red;
        confirmation.outlineWidth = 0.1f;
        confirmation.outlineColor = Color.red;
    }

    public void CancelCreation()
    {
        SceneManager.LoadScene("Login");
    }
}
