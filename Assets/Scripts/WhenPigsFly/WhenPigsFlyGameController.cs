using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class WhenPigsFlyGameController : MonoBehaviour
{
    private DateTime sessionDate;
    /// <summary>
    /// The game object which the pig must follow
    /// </summary>
    public GameObject target;
    /// <summary>
    /// The pig.
    /// </summary>
    public GameObject pig;
    /// <summary>
    /// The script that is attached to the stage change screen game object
    /// </summary>
    public WhenPigsFlyStageChangeScript stageChangeScript;
    /// <summary>
    /// The game object whcih represents the canvas that displays final scores
    /// </summary>
    public GenericGameOverScript gameOverScript;
    /// <summary>
    /// The pile of coins.
    /// </summary>
    public WhenPigsFlyCashDisplayScript cashController;

    /// <summary>
    /// The amount of time the target has to reach each point
    /// </summary>
    public float targetChangeInterval;

    /// <summary>
    /// The starting point for the current target lerp
    /// </summary>
    private Vector2 currentOriginPos;
    /// <summary>
    /// The destination point for the current target lerp
    /// </summary>
    private Vector2 currentTargetPos;
    /// <summary>
    /// The half dimensions of the viewport
    /// </summary>
    private Vector2 viewport;
    /// <summary>
    /// The time elapsed since the last target point change
    /// </summary>
    private float elapsedTime;
    /// <summary>
    /// The current stage that the user is on
    /// </summary>
    private int stage;
    /// <summary>
    /// The list of positions that the target will traverse through this stage
    /// </summary>
    private Queue<Vector2> stagePositions;
    /// <summary>
    /// Whether or not the stage is active (basically a second check on the update cycle)
    /// </summary>
    private bool stageActive;
    /// <summary>
    /// The number of stages that are included in a given level file
    /// </summary>
    private const int NUM_STAGES = 4;

    /// <summary>
    /// The points accrued and total points possible for both the current one second score and the total score for the
    /// current stage
    /// </summary>
    private int totalPoints = 0, oneSecondPoints = 0, totalPossiblePoints = 0, oneSecondPossiblePoints = 0, totalPossibleLevelPoints = 0;
    /// <summary>
    /// The amount of time elapsed since the last one second scoring period, reset to zero every time a one second period ends
    /// </summary>
    private float pointTimeElapsed = 0f;
    /// <summary>
    /// A list of the points that are accrued in each stage
    /// Deprecated?
    /// </summary>
    //private List<int> pointsList;

    /// <summary>
    /// The position of the coin in the last frame
    /// </summary>
    private Vector3 previousCoinPos;

    private AudioSource audioSrc;

    private const int DATA_POINTS = 30;
    private const string TABLE_NAME = "pigs";

    private List<IGameSessionData> dataList = new List<IGameSessionData>();

    public GenericDataDisplayManager dataDisplay;

    /// <summary>
    /// Used to initialize values such as viewport size and set the first target
    /// </summary>
    void Start()
    {
        //WhenPigsFlySmartFeedbackProcessor.ResetVars();

        //signal that the main thread is active to prevent shutdown of arduino communication thread
        _GlobalVariables.mainThreadActive = true;
        //get camera dimensions
        float camHeight = Camera.main.orthographicSize;
        float camWidth = Camera.main.aspect * camHeight;
        viewport = new Vector2(camWidth, camHeight);
        sessionDate = DateTime.Now;
        //initializes levels
        WhenPigsFlyLevelInitializer.WriteLevelsToJSON();

        //initialize stage related variables
        stage = 0;
        SetStage();

        audioSrc = GetComponent<AudioSource>();

        previousCoinPos = target.transform.position;
    }

    /// <summary>
    /// OnDestroy is only used to signal that the current game controller on the main thread is no longer being used,
    /// in case of a system shutdown, this will signal the arduino communication thread should shut itself down
    /// </summary>
	private void OnDestroy()
	{
        _GlobalVariables.mainThreadActive = false;
        WhenPigsFlySmartFeedbackProcessor.ResetVars();
	}

	/// <summary>
	/// Used to control target movement progress
	/// </summary>
	void Update()
    {
        //if the stage is not currently active(at a stage change screen), then don't execute update loop
        if (!stageActive)
            return;

        UpdateScore();

        //increased elapsed time for larger lerping
        elapsedTime += Time.deltaTime;

        //if elapsed time has surpassed the allotted target move time
        if (elapsedTime > targetChangeInterval)
        {
            //once the stage positions queue is depleted, move on to the next stage
            if (stagePositions.Count <= 0)
            {
                RecordStageData();

                stage += 1;
                stageActive = false;

                //if new stage is still within the acceptable range
                if (stage < NUM_STAGES)
                {
                    TriggerStageChangeCanvas();
                    return;
                }
                else
                {
                    ShowGameOverScreen();
                    return;
                }
            }
            //we only want to set the new target after we've checked if we even have another one
            SetNewTarget();
        }
        ChangeCoinPos();
    }

    /// <summary>
    /// Also resets previousCoinPos to target.transform.position
    /// </summary>
    /// <returns>The direction the coin has moved since the last frame</returns>
    private Vector2 CoinDirection()
    {
        Vector2 direction = target.transform.position - previousCoinPos;
        previousCoinPos = target.transform.position;
        return direction;
    }

    /// <summary>
    /// Updates info about current score, based on user's proximity to the coin
    /// Precondition: Called from Update()
    /// </summary>
    private void UpdateScore()
    {
        //increase elapsed time for one second interval
        pointTimeElapsed += Time.deltaTime;

        //increment total possible points
        oneSecondPossiblePoints += 3;

        if (pointTimeElapsed >= 0.5f)
        {
            totalPoints += oneSecondPoints;
            totalPossibleLevelPoints += oneSecondPossiblePoints;
            totalPossiblePoints += oneSecondPossiblePoints;

            int cashToSpawn = (int)((float)oneSecondPoints / (float)oneSecondPossiblePoints * 3);

            //audioSrc.volume = ((float)oneSecondPoints) / oneSecondPossiblePoints;
            for (int i = 0; i < cashToSpawn; i++) {
                cashController.SpawnCash();
            }
            //audioSrc.Play();

            //now reset points
            Vector2 pigLoc = pig.transform.position;
            Vector2 targetLoc = target.transform.position;
            List<int> dirs = new List<int>();
            if (pigLoc.x > targetLoc.x)
                dirs.Add(WhenPigsFlySmartFeedbackProcessor.RIGHT);
            else if (pigLoc.x < targetLoc.x)
                dirs.Add(WhenPigsFlySmartFeedbackProcessor.LEFT);
            if (pigLoc.y > targetLoc.y)
                dirs.Add(WhenPigsFlySmartFeedbackProcessor.UP);
            else if (pigLoc.y < targetLoc.y)
                dirs.Add(WhenPigsFlySmartFeedbackProcessor.DOWN);

            int misses = oneSecondPossiblePoints - oneSecondPoints;
            WhenPigsFlySmartFeedbackProcessor.AddCoinMisses(dirs.ToArray(), misses, oneSecondPossiblePoints);
            oneSecondPoints = 0;
            oneSecondPossiblePoints = 0;

            pointTimeElapsed = 0f;
        }
    }

    /// <summary>
    /// Adds current stage points to a list
    /// Deprecated???
    /// </summary>
    /*
    private void RecordPointsValues()
    {
        pointsList.Add(totalPoints);
    }
    */

    public void ResetPointsVars()
    {
        totalPoints = 0;
        totalPossiblePoints = 0;
    }

    /// <summary>
    /// Moves the coin along the path
    /// Precondition: Called every Update() cycle
    /// </summary>
    private void ChangeCoinPos()
    {
        //calculate lerped position for coin
        float distancePercentage = elapsedTime / targetChangeInterval;
        Vector2 newTargPos = Vector2.Lerp(currentOriginPos, currentTargetPos, distancePercentage);
        //use the variable above so we can set the z value without 2 calls to transform.position
        //z = -1 means it will render over the pig, z = 1 means it will render under, z = 0 is undefined behavior
        target.transform.position = new Vector3(newTargPos.x, newTargPos.y, -0.5f);
    }

    /// <summary>
    /// Triggers the stage change canvas.
    /// Precondition: Called when current stage is over
    /// </summary>
    private void TriggerStageChangeCanvas()
    {
        audioSrc.Play();
        string text1 = "- Stage " + (stage + 1) + " -";
        stageChangeScript.SetUIText(text1);
        stageChangeScript.TriggerStageChange();
    }

    /// <summary>
    /// Shows the game over screen.
    /// Precondition: Current stage has ended, and there are no stages left
    /// </summary>
    private void ShowGameOverScreen()
    {
        audioSrc.Play();
        Time.timeScale = 0f;
        //WhenPigsFlySmartFeedbackProcessor.ScaleParameters(totalPossibleLevelPoints);
        gameOverScript.SetStars(GetNumStars());

        string genCol1 = "coinPercentage";
        string graphTitle1 = "Cash Collected Per Session";
        IGameSessionData[] dataArray = dataList.ToArray();
        StringData table = new StringData(dataArray, genCol1, mult: 1000, format: "0.00", unit: "$", altTite: "Cash Collected");

        dataDisplay.AddTableView(table);
        dataDisplay.AddTextView(WhenPigsFlySmartFeedbackProcessor.LevelFinish());
        dataDisplay.AddGraph(_GlobalVariables.dataRep.GetSessionData(TABLE_NAME, DATA_POINTS),
            genCol1, title: graphTitle1, difficulty: _GlobalVariables.difficulty);

        gameOverScript.gameObject.SetActive(true);
        dataDisplay.InitializeDisplays();
    }

    private int GetNumStars()
    {
        float avgPointPercentage = dataList.Select(t => float.Parse(t.GetStringRepresentation("coinPercentage"))).Average(m => m);

        Debug.Log("Point Percentage: " + avgPointPercentage);

        if (avgPointPercentage > 0.66f)
        {
            return 3;
        }
        else if (avgPointPercentage > 0.33f)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Called by stage change canvas script after scene change screen has been passed
    /// </summary>
    public void SetStage()
    {
        //reset cash pile
        //cashController.ClearCash();
        //get targer position queue for new stage
        stagePositions = WhenPigsFlyLevelInitializer.GetLevelPointsQueue(_GlobalVariables.difficulty, stage);
        //reset coin to beginning position of the queue
        target.transform.position = ConvertToTruePoint(stagePositions.Dequeue());
        //set the next targer for lerping
        SetNewTarget();
    }

    /// <summary>
    /// Used to establish a new point for the target to travel to
    /// </summary>
    void SetNewTarget()
    {
        stageActive = true;
        //Reset elapsed time
        elapsedTime = 0;
        //Reset positions for lerping
        currentOriginPos = target.transform.position;
        currentTargetPos = ConvertToTruePoint(stagePositions.Dequeue());
    }

    /// <summary>
    /// Used to convert simple percentage vectors stored in the level files to proper
    /// viewport units
    /// </summary>
    /// <param name="v">The vector to be converted</param>
    /// <returns>A proper, usable, vector</returns>
    private Vector2 ConvertToTruePoint(Vector2 v)
    {
        Vector2 result = new Vector2();
        result.x = v.x * viewport.x;
        result.y = v.y * viewport.y;
        return result;
    }

    public void IncrementOneSecondPoints()
    {
        oneSecondPoints += 1;
    }

    private void RecordStageData()
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>()
        {
            { "game", TABLE_NAME },
            { "userId", _GlobalVariables.dataRep.GetCurrentUser().GetUserId() },
            { "level", _GlobalVariables.difficulty },
            { "stage", stage },
            { "date", sessionDate.ToUnixEpochTime() },
            { "coinPercentage", totalPoints * 1.0f / totalPossiblePoints}
        };
        PigsSessionData pigData = new PigsSessionData(dataDict);
        _GlobalVariables.dataRep.AddData(pigData);
        dataList.Add(pigData);

        ResetPointsVars();
    }
}
