using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MazeNewGameController : MonoBehaviour
{
    private DateTime sessionDate;
    public GenericGameOverScript gameOverScript;
    public GenericDataDisplayManager dataDisplay;

    /// <summary>
    /// Reference to the Map GameObject
    /// </summary>
    public MazeNewMapController map;
    /// <summary>
    /// Reference to the stage change screen game object
    /// </summary>
    public MazeStageChangeScript stageChangeOverlay;
    /// <summary>
    /// The script that is attached to our instance of the player object
    /// </summary>
    public MazePlayerControllerScript playerContScript;
    public AudioClip stageStartClip, stageEndClip;
    private AudioSource audioSrc;
    /// <summary>
    /// Time that was used this level/stage
    /// </summary>
    private float timeToComplete = 0f;
    /// <summary>
    /// Number of collisions this stage
    /// </summary>
    private int collisions = 0;

    private const string TABLE_NAME = "maze";
    /// <summary>
    /// Constant which defines the number of stages to go through whenever the game is played
    /// </summary>
    private const int NUM_STAGES = 2, DATA_POINTS = 30;
    /// <summary>
    /// Constant which defines the amount of time to be added for colliding with a wall
    /// </summary>
    private const float COLLISION_TIME = 1f;
    /// <summary>
    /// The current stage, should never be greater than NUM_STAGES
    /// </summary>
    private int currentStage = 0;
    /// <summary>
    /// The list of collisions per stage, stage is indicated by index
    /// </summary>
    //private List<int> collisionsList;
    /// <summary>
    /// The list of completion times for each stage, the stage is indicated by index
    /// </summary>
    //private List<float> completionTimesList;

    private List<IGameSessionData> dataList;
    /// <summary>
    /// 2-D array representing how many total seconds across all stages is equal to how many stars for each level
    /// ex)
    ///     [0, 0] = 16 means that on easy you need to complete all stages for a total time of less than 16 seconds to receive 3 stars
    ///     [0, 1] = 20 means that on easy you need to complete all stages for a total time of less than 20 seconds to receive 2 stars
    /// </summary>
    private int[,] starMap = { { 54, 78 }, { 90, 120 }, { 130, 164 } };

    /// <summary>
    /// Serves as an initialization point for the map and important variables
    /// </summary>
    void Start()
    {
        //MazeSmartFeedbackProcessor.ResetVars();
        //Used to signal to the communicator thread to continue functioning
        _GlobalVariables.mainThreadActive = true;
        audioSrc = GetComponent<AudioSource>();

        //Initialize collisions and completions variables
        //collisionsList = new List<int>();
        //completionTimesList = new List<float>();
        dataList = new List<IGameSessionData>();
        sessionDate = DateTime.Now;
        //Generate the first iteration of the map
        Debug.Log("Game controller start - Calling generate stage map");
        GenerateStageMap();
    }

    /// <summary>
    /// In this context, update serves only to keep track of the score/time
    /// </summary>
    private void Update()
    {
        timeToComplete += Time.deltaTime;
    }

    /// <summary>
    /// Will be passed as a delegate to collision cells so that collisions will properly
    /// add time to the user
    /// </summary>
    /// <param name="c">Collision parameter required by delegate definition</param>
    /// <returns>True always</returns>
    public bool AddTime(Collision2D c)
    {
        timeToComplete += COLLISION_TIME;
        collisions += 1;

        //visual indication of obstacle hit
        c.gameObject.GetComponent<MazePlayerControllerScript>().ObstacleHit();

        //Do angle calculations to figure out what side of the player the collision occurred on
        Vector2 pointCont = c.GetContact(0).point;
        Vector2 pointPlayer = c.gameObject.transform.position;
        float contAngle = WrapAngle(Vector2.Angle(new Vector2(0, 10), pointCont - pointPlayer));
        //Debug.Log("Contact Angle: " + contAngle);
        float angle = WrapAngle(c.gameObject.transform.rotation.eulerAngles.z);
        //Debug.Log("Player Angle: " + angle);
        float compositeAngle = WrapAngle(angle - contAngle);
        //Debug.Log("Composite Angle: " + compositeAngle);
        //need to check for stage like usual to record control changes correctly
        if (compositeAngle <= -45 && compositeAngle >= -135)
            MazeSmartFeedbackProcessor.Collision(MazeSmartFeedbackProcessor.LEFT, currentStage % 2);
        else if (compositeAngle < 45 && compositeAngle > -45)
            MazeSmartFeedbackProcessor.Collision(MazeSmartFeedbackProcessor.CENTER, currentStage % 2);
        else if (compositeAngle <= 135 && compositeAngle >= 45)
            MazeSmartFeedbackProcessor.Collision(MazeSmartFeedbackProcessor.RIGHT, currentStage % 2);

        return true;
    }

    /// <summary>
    /// Just used for wrapping angle values for comparison
    /// </summary>
    /// <param name="angle">Angle to be wrapped</param>
    /// <returns>New wrapped angle</returns>
    private float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;
        else if (angle <= -180)
            return angle + 360;

        return angle;
    }

    /// <summary>
    /// This function will be passed as a delegate to trigger cells so that when the player reaches the end
    /// of the map a proper scene change (or end of game) will occur
    /// </summary>
    /// <param name="c">Collision param required by delegate definition</param>
    /// <returns>True if successful, false if trigger has already been received</returns>
    public bool NextStage(Collider2D c)
    {
        //we check if the overlay is already active to prevent overincrementation of stage in the event that the player
        //triggers more than one trigger cell
        if (stageChangeOverlay.gameObject.activeSelf || gameOverScript.gameObject.activeSelf)
            return false;

        Debug.Log("Next stage function triggered");
        //increment the trigger then determine whether to progress to a new stage or to end the game session

        audioSrc.PlayOneShot(stageEndClip);
        RecordStageData();
        currentStage += 1;
        if (currentStage < NUM_STAGES)
        {
            //set the text boxes to display accurate information
            string text1 = "- Stage " + (currentStage + 1) + " -";
            //bool showIcon = false;
            //we only notify of a control change half way through when it occurs
            //if (currentStage == NUM_STAGES / 2)
            //{
                //playerContScript.SwitchControls();
                //showIcon = true;
            //}

            //set values in overlay canvas and trigger the fade in change
            stageChangeOverlay.SetUIText(text1, false);
            stageChangeOverlay.TriggerStageChange();
        }
        else
        {
            //set timescale to zero so that time is not added once the game is over
            Time.timeScale = 0;
            gameOverScript.SetStars(GetNumStars());

            string[] genDisplayCols = { "collisions", "time" };
            string[] graphTitles = { "Total Collisions Per Session", "Completion Time Per Session" };
            IGameSessionData[] dataArray = dataList.ToArray();

            StringData[] tables =
            {
                new StringData(dataArray, genDisplayCols[0], format: "0"),
                new StringData(dataArray, genDisplayCols[1], unit: "s")
            };

            dataDisplay.AddTableView(tables);
            dataDisplay.AddTextView(MazeSmartFeedbackProcessor.LevelFinish());
            dataDisplay.AddGraph(_GlobalVariables.dataRep.GetSessionData(TABLE_NAME, DATA_POINTS),
                genDisplayCols, titles: graphTitles, difficulty: _GlobalVariables.difficulty);
            //game over overlay script will set timescale back to one once it is terminated
            gameOverScript.gameObject.SetActive(true);
            dataDisplay.InitializeDisplays();
        }

        return true;
    }

    /// <summary>
    /// Calculates how many stars should be displayed on the game over screen
    /// </summary>
    /// <returns>The number of stars to display</returns>
    private int GetNumStars()
    {
        float totalTime = 0;

        foreach (float t in dataList.Select(t => float.Parse(t.GetStringRepresentation("time"))))
            totalTime += t;

        if (totalTime < starMap[_GlobalVariables.difficulty, 0])
        {
            return 3;
        }
        else if (totalTime < starMap[_GlobalVariables.difficulty, 1])
        {
            return 2;
        }

        return 1;
    }

    /// <summary>
    /// Records data locally for the current stage and resets counter valuesity
    /// </summary>
    private void RecordStageData()
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>()
        {
            { "game", TABLE_NAME },
            { "userId", _GlobalVariables.dataRep.GetCurrentUser().GetUserId() },
            { "level", _GlobalVariables.difficulty },
            { "stage", currentStage },
            { "date", sessionDate.ToUnixEpochTime() },
            { "time", timeToComplete },
            { "collisions", collisions }
        };
        MazeSessionData mazeData = new MazeSessionData(dataDict);
        _GlobalVariables.dataRep.AddData(mazeData);
        dataList.Add(mazeData);

        timeToComplete = 0;
        collisions = 0;
    }

    /// <summary>
    /// Causes a regeneration of the map with the current stage and difficulty settings
    /// </summary>
    public void GenerateStageMap()
    {
        Debug.Log("In generate stage map - Calling map generate map");
        playerContScript.SetPosRandomRotation(map.GenerateMap(_GlobalVariables.difficulty, currentStage));
        audioSrc.PlayOneShot(stageStartClip);
        playerContScript.MakeVisible();
    }

    /// <summary>
    /// Getter for currentStage variable
    /// </summary>
    /// <returns>Current stage</returns>
    public int GetStage()
    {
        return currentStage;
    }

    /// <summary>
    /// We use this method to signal to the communicator thread that the program may have just ended
    /// </summary>
    void OnDestroy()
    {
        _GlobalVariables.mainThreadActive = false;
        MazeSmartFeedbackProcessor.ResetVars();
    }
}
