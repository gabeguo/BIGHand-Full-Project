using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericGameController : MonoBehaviour
{
    public GenericGameOverScript gameOverScreen;
    // Start is called before the first frame update
    public void Start()
    {
        _GlobalVariables.mainThreadActive = true;
    }

	public void OnDestroy()
	{
        _GlobalVariables.mainThreadActive = false;
	}

	// Update is called once per frame
	void Update()
    {
        
    }

    protected void EndGame()
    {
        gameOverScreen.SetStars(GetNumStars());
        //gameOverScreen.SetStageScoringValues(GetStageScoringValues());
        //gameOverScreen.FetchSmartTips();
        Time.timeScale = 0;
        gameOverScreen.gameObject.SetActive(true);
    }

    protected abstract int GetNumStars();

    //protected abstract int[] GetStageScoringValues();
}
