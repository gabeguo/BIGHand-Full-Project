using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class HappyTimesGameController : GenericGameController
{
    public HappyTimesBorderScript border;
    public GameObject frownyFacePrefab;

    private DateTime sessionDate;
    private LoopTimer restTimer, spawnTimer, currentTimer;
    private List<int> spawnPattern; // 0 = neither, 1 = left, 2 = right, 3 = both
    //private int[,] handStageCollisions = new int[2, 4];
    private int[,] numTriggers = new int[2, NUM_STAGES];    //number of times user has gripped-and-released
    private readonly float[] difficultySpawnRates = { 2.0f, 1.6f, 1.2f };
    private readonly float[] difficultyWaitTimes = { 2, 2, 2 };
    private readonly int[] difficultySectionFaces = { 3, 5, 7 };
    private float maxX;
    private float maxY;
    private int currentStage;
    private int currentSection;
    private int leftFaces, rightFaces;

    private const int SP_NONE = 0, SP_LEFT = 1, SP_RIGHT = 2, SP_BOTH = 3;
    private const int NUM_SECTIONS_PER_STAGE = 3, NUM_STAGES = 4, DATA_POINTS = 30;
    private const float PERCENTAGE_AWAY_FROM_CENTER = 0.5f;

    public HappyTimesStageChangeScript stageChangeScript;

    //for database
    private List<IGameSessionData> dataList = new List<IGameSessionData>();
    public GenericDataDisplayManager dataDisplay;
    private const string TABLE_NAME = "happy";

    private int leftHits = 0, rightHits = 0;   //the number of times a face was caught on left/right side in the current stage
    private int possibleLeft = 0, possibleRight = 0;  //the possible number of hits we could get on left and right

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        sessionDate = DateTime.Now;
        maxX = Camera.main.orthographicSize * Camera.main.aspect;
        maxY = Camera.main.orthographicSize + 0.5f * frownyFacePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        DoStageSetup();
    }

    ///<summary>
    /// -> Spawns frowny face at rate determined by level and stage (and initializes them too)
    /// -> Progresses stage when appropriate
    /// </summary>
    void Update()
    {
        currentTimer.Update(Time.deltaTime);
    }

    /// <summary>
    /// Spawns the frowny face.
    /// </summary>
    /// <param name="left">Spawn a left face?</param>
    /// <param name="right">Spawn a right face?</param>
    private void SpawnFrownyFace(bool left, bool right)
    {
        if (left)
        {
            GameObject leftFace = Instantiate(frownyFacePrefab, new Vector3(-maxX * PERCENTAGE_AWAY_FROM_CENTER, maxY, 1), Quaternion.identity);
            leftFace.GetComponent<HappyTimesFrownyController>().Initialize(this, border);
            leftFaces += 1;
            //HappyTimesSmartFeedback.AddTotalPossibleLeftGrip();
        }
        if (right)
        {
            GameObject rightFace = Instantiate(frownyFacePrefab, new Vector3(+maxX * PERCENTAGE_AWAY_FROM_CENTER, maxY, 1), Quaternion.identity);
            rightFace.GetComponent<HappyTimesFrownyController>().Initialize(this, border);
            rightFaces += 1;
            //HappyTimesSmartFeedback.AddTotalPossibleRightGrip();
        }
    }

    private void NextStage()
    {
        RecordStageData();
        if (++currentStage >= NUM_STAGES) {
            EndGame();
        } else {
            DoStageSetup();
            stageChangeScript.SetUIText("Stage " + (currentStage + 1), false);
            stageChangeScript.TriggerStageChange();
        }
    }

    /// <summary>
    /// Records data locally for the current stage and resets counter valuesity
    /// </summary>
    private void RecordStageData()
    {
        int leftTrigs = Mathf.Max(numTriggers[_GlobalVariables.LEFT_INDEX, currentStage], 1);
        int rightTrigs = Mathf.Max(numTriggers[_GlobalVariables.RIGHT_INDEX, currentStage], 1);

        float leftAvgGripTime = border.GetLeftTime() / leftTrigs;
        float rightAvgGripTime = border.GetRightTime() / rightTrigs;
        Dictionary<string, object> dataDict = new Dictionary<string, object>()
        {
            { "game", TABLE_NAME },
            { "userId", _GlobalVariables.dataRep.GetCurrentUser().GetUserId() },
            { "level", _GlobalVariables.difficulty },
            { "stage", currentStage },
            { "date", sessionDate.ToUnixEpochTime() },
            {"leftGrips", numTriggers[_GlobalVariables.LEFT_INDEX, currentStage]},
            {"leftHits", leftHits},
            {"leftFaces", leftFaces},
            {"rightGrips", numTriggers[_GlobalVariables.RIGHT_INDEX, currentStage]},
            {"rightHits", rightHits},
            {"rightFaces", rightFaces },
            {"leftAvgGripTime", leftAvgGripTime},
            {"rightAvgGripTime", rightAvgGripTime}
        };
        HappySessionData happyData = new HappySessionData(dataDict);
        _GlobalVariables.dataRep.AddData(happyData);
        dataList.Add(happyData);

        ResetVars();
    }

    /// <summary>
    /// Resets the stage variables: leftHits, rightHits,
    /// plus leftTime and rightTime in border script
    /// numTriggers does not need to be reset, because it is an array
    /// </summary>
    /// Precondition: Called every time stage changes
    private void ResetVars()
    {
        leftHits = 0;
        rightHits = 0;
        leftFaces = 0;
        rightFaces = 0;
        border.ResetGripTimes();
    }

    protected new void EndGame()
    {
        base.EndGame();
        string[] graphDisplays = { "leftAvgGripTime", "rightAvgGripTime" };
        string[] graphTitles = { "Average Grip-Release Time For Left Hand Per Session",
            "Average Grip-Release Time For Right Hand Per Session" };
        string[] tableCols1 = { "leftGrips", "leftHits", "leftFaces" };
        string[] tableCols2 = { "rightGrips", "rightHits", "rightFaces" };
        IGameSessionData[] dataArray = dataList.ToArray();

        StringData[] tables =
        {
            new StringData(dataArray, tableCols1, format: "0"),
            new StringData(dataArray, tableCols2, format: "0"),
            new StringData(dataArray, graphDisplays, unit: "s")
        };

        dataDisplay.AddTableView(tables);
        dataDisplay.AddTextView(HappyTimesSmartFeedback.LevelFinish());
        dataDisplay.AddGraph(_GlobalVariables.dataRep.GetSessionData(TABLE_NAME, DATA_POINTS),
            graphDisplays, titles: graphTitles, difficulty: _GlobalVariables.difficulty);
        dataDisplay.InitializeDisplays();
    }

    protected override int GetNumStars()
    {
        //float pointPercentage = HappyTimesSmartFeedback.GetPercentageScore();
        if (dataList.Count <= 0)
            return 0;

        //TODO: Calculate this the right way
        float avgPointPercentage = dataList.Select((x) => float.Parse(
            x.GetStringRepresentation("leftFaces") + x.GetStringRepresentation("rightFaces"))).Sum(
            (x) => x) / dataList.Count;

        Debug.Log("floatPercentage: " + avgPointPercentage);
        if (avgPointPercentage >= 0.67f) {
          return 3;
        } else if (avgPointPercentage >= 0.33f) {
          return 2;
        } else {
          return 1;
        }
    }

    protected int[] GetStageScoringValues()
    {
        return new int[2];
    }

    private void DoStageSetup()
    {
        //Debug.Log("Difficulty: " + _GlobalVariables.difficulty);
        float waitTime = difficultyWaitTimes[_GlobalVariables.difficulty];
        float spawnTime = difficultySpawnRates[_GlobalVariables.difficulty];
        float spawnPeriodTime = spawnTime * (difficultySectionFaces[_GlobalVariables.difficulty] + 1);
        //Added  1 to section faces number to make final "spawn" actually be the switch to restTimer

        currentSection = 0;

        spawnPattern = GetSpawnPattern();
        //Debug.Log("spawnPattern: " + spawnPattern.ToString());
        restTimer = new LoopTimer(SwitchTimer, waitTime);
        spawnTimer = new LoopTimer(SwitchTimer, SpawnNextFace, spawnPeriodTime, spawnTime);

        currentTimer = restTimer;
        currentTimer.Reset();
    }

    public void SpawnNextFace()
    {
        int spawnPatternIndex = spawnPattern.Count - 1;
        if (spawnPatternIndex < 0)
            return;

        int sides = spawnPattern[spawnPatternIndex];
        spawnPattern.RemoveAt(spawnPatternIndex);

        switch (sides)
        {
            case SP_BOTH:
                SpawnFrownyFace(true, true);
                return;
            case SP_LEFT:
                SpawnFrownyFace(true, false);
                return;
            case SP_RIGHT:
                SpawnFrownyFace(false, true);
                return;
            default:
                return;
        }
    }

    public void SwitchTimer()
    {
        if (currentTimer == restTimer) {
            currentTimer = spawnTimer;
            currentSection += 1;

            if (currentSection >= NUM_SECTIONS_PER_STAGE) {
                NextStage();
                return;
            }
        } else {
            currentTimer = restTimer;
        }

        currentTimer.Reset();
    }

    /// <summary>
    /// Adds one to the number of times the user has triggered the boundary.
    /// </summary>
    /// <param name="handKey">Hand key.</param>
    public void BoundaryTrigger(int handKey)
    {
        numTriggers[handKey, currentStage] += 1;
    }

    /// <summary>
    /// Increments the number of faces caught on the corresponding side
    /// </summary>
    /// <param name="isLeft">If set to <c>true</c> is left.</param>
    public void IncrementFacesCaught(bool isLeft)
    {
        if (isLeft)
        {
            leftHits++; //may be unnecessary
            //HappyTimesSmartFeedback.AddLeftHit();
        }
        else
        {
            rightHits++;    //may be unnecessary
            //HappyTimesSmartFeedback.AddRightHit();
        }
    }

    public void IncrementPossibleNumFaces(bool isLeft) {
      if (isLeft) possibleLeft++;
      else possibleRight++;
    }

    private List<int> GetSpawnPattern()
    {
        int faces = difficultySectionFaces[_GlobalVariables.difficulty];

        switch (currentStage)
        {
            case SP_LEFT - 1:
                return FillList(() => SP_LEFT, faces);
            case SP_RIGHT - 1:
                return FillList(() => SP_RIGHT, faces);
            case SP_BOTH - 1:
                return FillList(() => SP_BOTH, faces);
            default:
                return RandomSequence(faces);
        }
    }

    private List<int> FillList(Func<int> function, int count)
    {
        List<int> l = new List<int>();

        for (int i = 0; i < 2 * count; i++)
            l.Add(function());

        return l;
    }

    private List<int> RandomSequence(int count) {
        List<int> l = new List<int>();

        for (int i = 0; i <= count; i++)
        {
            l.Add(UnityEngine.Random.Range(SP_LEFT, SP_BOTH + 1));
        }

        return l;
    }

	private new void OnDestroy()
	{
        base.OnDestroy();
        HappyTimesSmartFeedback.ResetVars();
	}
}
