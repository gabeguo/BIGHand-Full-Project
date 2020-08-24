using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBCloserScript : MonoBehaviour
{
    private static bool created;
    private bool shouldKillConnection;

    void Awake()
    {
        if (!created)
        {
            shouldKillConnection = true;
            created = true;
            DontDestroyOnLoad(gameObject);
        }

    }

    void OnDestroy()
    {
        if (shouldKillConnection)
        {
            _GlobalVariables.dataRep.CloseConnection();
            Debug.Log("Closer Dead.");
        }
    }
}
