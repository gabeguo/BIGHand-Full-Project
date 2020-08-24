using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class BalloonGameController : MonoBehaviour
{
    private DateTime sessionDate;
    public GenericGameOverScript gameOverScript;
    public GenericDataDisplayManager dataDisplay;

    /// <summary>
    /// The script attached to the stage transition canvas
    /// </summary>
    public BalloonStageChangeScript stageChangeScript;
    /// <summary>
    /// The script attached to the player game object
    /// </summary>
    public BalloonPlayerController playerScript;
    /// <summary>
    /// A list of the maximum heights for each of the building obstacles
    /// </summary>
    public List<float> buildingMaxHeights;
    /// <summary>
    /// A list of prefabs that correspond to the heights in buildingMaxHeights
    /// </summary>
    public List<GameObject> buildingPrefabs;
    /// <summary>
    /// Dictionary the matches up buildingPrefabs to buildingMaxHeights for quick lookups
    /// </summary>
    private Dictionary<float, GameObject> heightPrefabLookup;

    /// <summary>
    /// These max heights are for the spawn heights for different difficulty levels
    /// where the index is the difficulty level
    /// </summary>
    private float[] maxHeights = { 0.25f, 0.55f, 0.8f };

    /// <summary>
    /// A pattern of heights used for map generation in the current stage
    /// and level
    /// </summary>
    private float[] obstaclePattern;
    /// <summary>
    /// How many seconds pass between each obstacle spawn
    /// </summary>
    public float spawnInterval;
    /// <summary>
    /// The amount of time it takes for each obstacle to traverse across the entire screen
    /// </summary>
    public float obstacleTravelTime;
    /// <summary>
    /// The amount of time elapsed since the last obstacle spawn
    /// </summary>
    private float elapsedTime;
    /// <summary>
    /// The current stage
    /// </summary>
    private int currentStage = 0;
    /// <summary>
    /// Counts the number of destroyed obstacles, used for determining the end of each stage
    /// </summary>
    private int destroyedObstacleCounter = 0;
    /// <summary>
    /// Counts the number of obstacles that have been spawned, used to stop obstacle production
    /// once the stage pattern has been exhausted
    /// </summary>
    private int obstacleCounter = 0;

    /// <summary>
    /// Constant definition of the number stages in any given level
    /// </summary>
    private const int NUM_STAGES = 6, DATA_POINTS = 30;
    private const string TABLE_NAME = "balloon";

    private List<IGameSessionData> dataList;
    private int currentStageLeftCollisions, currentStageRightCollisions;
    private int maxLeft, maxRight;

    /// <summary>
    /// Called at the beginning of the game loop, used for initialization logic
    /// </summary>
    void Start()
    {
        _GlobalVariables.mainThreadActive = true;
        elapsedTime = 0f;
        dataList = new List<IGameSessionData>();
        sessionDate = DateTime.Now;

        if (buildingMaxHeights.Count != buildingPrefabs.Count)
            throw new System.MissingFieldException("Either the max heights field or the building prefabs field is missing" +
                " one or more items. They must be of equal length, check the balloon game controller.");

        heightPrefabLookup = new Dictionary<float, GameObject>();
        for (int i = 0; i < buildingMaxHeights.Count; i++)
            heightPrefabLookup.Add(buildingMaxHeights[i], buildingPrefabs[i]);

        obstaclePattern = GetStagePattern(0);
    }

    /// <summary>
    /// Called every frame of the update loop
    /// </summary>
    void Update()
    {
        //dont execute loop if the game is paused
        if (Time.timeScale == 0)
            return;

        //logic for obstacle spawning, stage changes, and game over triggers below
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= spawnInterval && obstacleCounter < obstaclePattern.Length)
        {
            //its time to spawn another obstacle, spawn and reset counter
            elapsedTime = 0f;
            SpawnNextObstacle(maxHeights[_GlobalVariables.difficulty] * obstaclePattern[obstacleCounter++]);
        }
        else if (destroyedObstacleCounter >= obstaclePattern.Length)
        {
            //we've destroyed all the obstacles in the scene
            //reset counters
            obstacleCounter = 0;
            destroyedObstacleCounter = 0;
            elapsedTime = 0f;

            RecordStageData();
            if (++currentStage >= NUM_STAGES)
            {
                //out of stages, trigger game over screen
                TriggerGameOver();
            }
            else
            {
                //stages still remain, trigger the next one
                TriggerStageChange();
            }
        }
    }

    /// <summary>
    /// Called by BalloonObstacleScript to signal the destruction of an obstacle
    /// </summary>
    public void IncrementDestroyedObstacleCounter()
    {
        destroyedObstacleCounter += 1;
    }

    private void RecordStageData()
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>()
        {
            { "game", TABLE_NAME },
            { "userId", _GlobalVariables.dataRep.GetCurrentUser().GetUserId() },
            { "level", _GlobalVariables.difficulty },
            { "stage", currentStage },
            { "date", sessionDate.ToUnixEpochTime() },
            { "leftCollisions", currentStageLeftCollisions },
            { "leftMaxStrength", maxLeft },
            { "rightCollisions", currentStageRightCollisions },
            { "rightMaxStrength", maxRight },
        };
        BalloonSessionData balloonData = new BalloonSessionData(dataDict);
        _GlobalVariables.dataRep.AddData(balloonData);
        dataList.Add(balloonData);

        currentStageLeftCollisions = 0;
        currentStageRightCollisions = 0;
        maxLeft = 0;
        maxRight = 0;
    }

    /// <summary>
    /// Called when a stage change is occurring
    /// </summary>
    private void TriggerStageChange()
    {
        //set up transition screen text element contents
        string mainText = "- Stage " + (currentStage + 1) + " - ";
        //string extraText = "Press <color=#0000CC>\"Enter\"</color> to Continue";
        bool showIcon = false;
        if (currentStage == 3)
        {
            showIcon = true;
            playerScript.SwitchHands();
        }

        //apply to screen and show
        stageChangeScript.SetUIText(mainText, showIcon);
        stageChangeScript.TriggerStageChange();
        obstaclePattern = GetStagePattern(currentStage);
    }

    /// <summary>
    /// Called once the stages have all been used and it is time to display
    /// final level statistics
    /// </summary>
    private void TriggerGameOver()
    {
        Time.timeScale = 0;
        gameOverScript.SetStars(GetNumStars());

        string graphCol1 = "leftMaxStrength";
        string graphCol2 = "rightMaxStrength";
        string graphTitle1 = "Maximum Strength With Left Hand Per Session";
        string graphTitle2 = "Maximum Strength With Right Hand Per Session";
        string tableCol1 = "leftCollisions";
        string tableCol2 = "rightCollisions";
        int[] leftStages = { 0, 1, 2 };
        int[] rightStages = { 3, 4, 5 };
        IGameSessionData[] dataArray = dataList.ToArray();

        StringData[] tables = {
            new StringData(dataArray.LooseRange(0, 3), tableCol1, format: "0"),
            new StringData(dataArray.LooseRange(3, 3), tableCol2, format: "0", offset: 3)
        };

        dataDisplay.AddTableView(tables);
        dataDisplay.AddTextView(BalloonSmartFeedback.LevelFinish());
        dataDisplay.AddGraph(_GlobalVariables.dataRep.GetSessionData(TABLE_NAME,
            DATA_POINTS), graphCol1, discludeStages: rightStages, title: graphTitle1, difficulty: _GlobalVariables.difficulty);
        dataDisplay.AddGraph(_GlobalVariables.dataRep.GetSessionData(TABLE_NAME,
            DATA_POINTS), graphCol2, discludeStages: leftStages, title: graphTitle2, difficulty: _GlobalVariables.difficulty);

        gameOverScript.gameObject.SetActive(true);

        Debug.Log("Balloon game controller, GOS Active: " + gameOverScript.gameObject.activeSelf);

        dataDisplay.InitializeDisplays();
    }

    /// <summary>
    /// Calculates how many stars to show after the level has concluded
    /// </summary>
    /// <returns>The number of stars to show</returns>
    private int GetNumStars()
    {
        int totalLeftColls = dataList.Select(t => int.Parse(t.GetStringRepresentation("leftCollisions"))).Sum(m => m);
        int totalRightColls = dataList.Select(t => int.Parse(t.GetStringRepresentation("rightCollisions"))).Sum(m => m);
        int totalColls = totalLeftColls + totalRightColls;
        if (totalColls <= 15) {
            return 3;
        } else if (totalColls <= 35) {
            return 2;
        }

        return 1;
    }

    /// <summary>
    /// Called when the game object is being destroyed, used for signaling to the 
    /// arduino communicator thread that the system could potentially shut down soon
    /// </summary>
    void OnDestroy()
    {  //do not keep leftovers; reset variables every time game is left
        _GlobalVariables.mainThreadActive = false;
        BalloonSmartFeedback.ResetVars();
    }

    /// <summary>
    /// Spawns an obstacle that reaches the desired height percentage
    /// </summary>
    /// <param name="desiredHeightPercentage">The height that the obstacle should be</param>
    private void SpawnNextObstacle(float desiredHeightPercentage)
    {
        float desiredActualHeight = desiredHeightPercentage * Camera.main.orthographicSize * 2;

        GameObject prefab = FetchPrefabForHeightPercentage(desiredHeightPercentage);
        GameObject newObstacle = Instantiate(prefab, new Vector3(0, 0, 1), Quaternion.identity);
        newObstacle.GetComponent<BalloonObstacleScript>().Spawn(desiredActualHeight, obstacleTravelTime, gameObject);
    }

    /// <summary>
    /// Gets the height in viewport units of a certain percentage of the screen height
    /// </summary>
    /// <param name="heightPercentage">The percentage of the viewport height to be converted</param>
    /// <returns>The corresponding height in viewport units</returns>
    private GameObject FetchPrefabForHeightPercentage(float heightPercentage)
    {
        float desiredHeight = Camera.main.orthographicSize * heightPercentage * 2;

        foreach (KeyValuePair<float, GameObject> kvp in heightPrefabLookup.OrderBy(h => h.Key))
        {
            if (kvp.Key >= desiredHeight)
                return kvp.Value;
        }

        return null;
    }

    /// <summary>
    /// Defines the obstacle height patterns for each stage
    /// </summary>
    /// <param name="stage">The stage to get the pattern for</param>
    /// <returns>The requested pattern</returns>
    private float[] GetStagePattern(int stage)
    {
        float[] result = new float[10];
        //mod 3 because pattern repeats for each hand
        stage = stage % 3;

        switch (stage)
        {
            case 0:
                //All half height
                for (int i = 0; i < 10; i++)
                {
                    result[i] = 0.5f;
                }

                return result;
            case 1:
                //Alternating 1/3 and 2/3
                for (int i = 0; i < 10; i += 2)
                {
                    result[i] = 0.33f;
                    result[i + 1] = 0.66f;
                }

                return result;
            default: //case 2:
                //From 0.10 to 1.00 in increments of 0.10
                for (int i = 1; i <= 10; i++)
                {
                    result[i - 1] = i * 0.1f;
                }

                return result;
        }
    }

    /// <summary>
    /// Called by BalloonObstacleScript when a collision occurs, used to help determine stars/score
    /// for the level as recording some aspects of the smart feedback data
    /// </summary>
    /// <param name="obstacle">The position of the obstacle that was hit</param>
    /// <param name="playerParam">The position of the player at the time of the collision</param>
    public void RecordCollision(Vector2 obstacle, Vector2 playerParam, bool cloud)
    {
        int hand;
        if (currentStage < 3) {
            hand = _GlobalVariables.LEFT_INDEX;
            currentStageLeftCollisions += 1;
        } else {
            hand = _GlobalVariables.RIGHT_INDEX;
            currentStageRightCollisions += 1;
        }

        if (cloud) {
            BalloonSmartFeedback.Collision(hand, BalloonSmartFeedback.CLOUD_COLLISION);
        } else {
            if (playerParam.x < obstacle.x)
                BalloonSmartFeedback.Collision(hand, BalloonSmartFeedback.LEFT_COLLISION);
            else
                BalloonSmartFeedback.Collision(hand, BalloonSmartFeedback.RIGHT_COLLISION);
        }
    }

    public void RegisterForce(int hand, int force)
    {
        switch (hand)
        {
            case _GlobalVariables.LEFT_INDEX:
                maxLeft = Mathf.Max(maxLeft, force);
                break;
            case _GlobalVariables.RIGHT_INDEX:
                maxRight = Mathf.Max(maxRight, force);
                break;
        }
    }
}
