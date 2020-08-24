using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogOutScript : MonoBehaviour
{
    public void LogOut()
    {
        _GlobalVariables.dataRep.SignOut();
        SceneManager.LoadScene("Login");
    }
}
