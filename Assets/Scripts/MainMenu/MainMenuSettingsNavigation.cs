using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for buttons on side panel to load scenes on click.
/// </summary>
public class MainMenuSettingsNavigation : MonoBehaviour
{
	/// <summary>
	/// The name of the scene this button will load
	/// </summary>
	public string sceneName;

    /// <summary>
    /// Switches the scene to <c>sceneName</c>
    /// </summary>
    public void SwitchScene()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
    }
}
