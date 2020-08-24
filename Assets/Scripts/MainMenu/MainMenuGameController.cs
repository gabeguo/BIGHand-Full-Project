using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using TMPro;

public class MainMenuGameController : MonoBehaviour
{
    //game object which encapsulates greeting text
    public GameObject greetingGO;

    //text element that greets the user
    private TextMeshProUGUI greetingText;

    // Start is called before the first frame update
    void Start()
    {
        //create and activate thread
        _GlobalVariables.mainThreadActive = true;

        //set greetingText
        Debug.Log("Hello, " + _GlobalVariables.userId);
        greetingText = greetingGO.GetComponent<TextMeshProUGUI>();
        greetingText.text = "Hi " + _GlobalVariables.userId;
    }

	private void OnDestroy()
	{
        _GlobalVariables.mainThreadActive = false;
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
