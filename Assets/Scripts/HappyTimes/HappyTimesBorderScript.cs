using UnityEngine;

public class HappyTimesBorderScript : MonoBehaviour
{
    public HappyTimesGameController gameCont;
    public SpriteRenderer rend;
    public Sprite[] borderSprites;

    private const float FORCE_PERCENTAGE_FOR_TRIGGER = 0.01f;
    private const float BORDER_HEIGHT_PERCENTAGE = 0.27f;
    private const int LEFT_SPRITE = 1, RIGHT_SPRITE = 2, BOTH_SPRITE = 3, NONE_SPRITE = 0;

    private float leftBound, rightBound;
    private bool leftActive, rightActive;

    private bool leftFaceColliding = false, rightFaceColliding = false; //whether there is currently a face on this side of the border

    private bool leftValidAtAllTimes = false, rightValidAtAllTimes = false; //from grip time to release time, if the face was continuously on there for every moment

    private bool leftSmiley = false, rightSmiley = false; //whether the face on the left or right side is smiling yet

    private float leftTime = 0, rightTime = 0;  //the time the user has spent gripping each respective grip

    public AudioSource hitAudio, missAudio;

    // Start is called before the first frame update
    void Start()
    {
        leftBound = FORCE_PERCENTAGE_FOR_TRIGGER * _GlobalVariables.maxGripStrength[_GlobalVariables.LEFT_INDEX];
        rightBound = FORCE_PERCENTAGE_FOR_TRIGGER * _GlobalVariables.maxGripStrength[_GlobalVariables.RIGHT_INDEX];

        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        Vector2 spriteSize = rend.sprite.bounds.size;

        Vector2 scale = transform.localScale;

        float xScale = scale.x * cameraSize.x / spriteSize.x;
        float yScale = scale.y * cameraSize.y / spriteSize.y * BORDER_HEIGHT_PERCENTAGE;
        transform.localScale = new Vector2(xScale, yScale);

        float borderHeight = cameraHeight * (-0.5f + 1.5f * BORDER_HEIGHT_PERCENTAGE);
        transform.position = new Vector3(0, borderHeight, 1.5f);
        //setup size and position of border
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale < 1) return;

        bool newSpriteNeeded = CheckLeft() || CheckRight();

        if (leftActive)
        {
            leftTime += Time.deltaTime;
            if (!leftFaceColliding)
            {
              leftValidAtAllTimes = false;
            }
        }
        if (rightActive)
        {
            rightTime += Time.deltaTime;
            if (!rightFaceColliding)
            {
              rightValidAtAllTimes = false;
            }
        }

        if (newSpriteNeeded)
            SetSprite();
    }

    //check if left hand has changed state, and record data about collisions with face, and play sound effects
    private bool CheckLeft()
    {
      bool newSpriteNeeded = false;

      //check left
      bool prevLeftActive = leftActive;
      //Debug.Log("left force: " + _GlobalVariables.leftForce);
      //Debug.Log("Left force needed: " + leftBound);
      leftActive = _GlobalVariables.leftForce >= leftBound;

        if (leftActive != prevLeftActive)
        {   //sprite changed
            newSpriteNeeded = true;

            if (leftActive) //started to grip left
            {
                leftValidAtAllTimes = true;
                HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.LEFT,
                    HappyTimesSmartFeedback.SQUEEZE, leftFaceColliding);
            }
            else
            {   //on releasing
                HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.LEFT,
                    HappyTimesSmartFeedback.RELEASE, leftFaceColliding);
                if (leftValidAtAllTimes && !leftSmiley) { //successful grip-release - make smiley once
                    hitAudio.Play();
                    leftSmiley = true;
                //TODO: other clauses can be removed if we don't want double failure sounds
                } else if (!leftSmiley && leftFaceColliding) {  //early grip
                    missAudio.Play();
                //Debug.Log("not left smile and left face collide");
                } else if (!leftFaceColliding) {  //late release
                    missAudio.Play();
                //Debug.Log("not left face collide");
              }
              gameCont.BoundaryTrigger(_GlobalVariables.LEFT_INDEX);  //count number of grip-and-release

              leftValidAtAllTimes = false;  //reset
          }
      }

      return newSpriteNeeded;
    }

    //checks for state change and collisions in right hand, and play sound effects
    private bool CheckRight()
    {
      bool newSpriteNeeded = false;

      //check right
      bool prevRightActive = rightActive;
      //Debug.Log("Right force: " + _GlobalVariables.rightForce);
      //Debug.Log("Right force needed: " + rightBound);
      rightActive = _GlobalVariables.rightForce >= rightBound;

        if (rightActive != prevRightActive)
        {   //sprite changed
            newSpriteNeeded = true;

            if (rightActive)  //started to grip right
            {
                rightValidAtAllTimes = true;
                HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.RIGHT,
                    HappyTimesSmartFeedback.SQUEEZE, rightFaceColliding);
            }
            else
            {   //releasing
                HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.RIGHT,
                    HappyTimesSmartFeedback.RELEASE, rightFaceColliding);
                if (rightValidAtAllTimes && !rightSmiley) { //success for first time
                    hitAudio.Play();
                    rightSmiley = true;
                } else if (!rightSmiley && rightFaceColliding) {  //early grip
                    missAudio.Play();
                } else if (!rightFaceColliding) { //late release
                    missAudio.Play();
                }
                gameCont.BoundaryTrigger(_GlobalVariables.RIGHT_INDEX); //count number of grip-and-release

                rightValidAtAllTimes = false; //reset
            }
        }

      return newSpriteNeeded;
    }

    private void SetSprite()
    {
        //Debug.Log("Setting sprite");
        if (rightActive && leftActive)
            rend.sprite = borderSprites[BOTH_SPRITE];
        else if (leftActive)
            rend.sprite = borderSprites[LEFT_SPRITE];
        else if (rightActive)
            rend.sprite = borderSprites[RIGHT_SPRITE];
        else
            rend.sprite = borderSprites[NONE_SPRITE];
    }

    public bool GetLeftActive()
    {
        return leftActive;
    }

    public bool GetRightActive()
    {
        return rightActive;
    }

    /// <summary>
    /// Resets leftTime and rightTime to 0.
    /// </summary>
    /// Precondition: Called by gameController every time a stage changes
    public void ResetGripTimes()
    {
        leftTime = 0;
        rightTime = 0;
    }
    public float GetLeftTime()
    {
        return leftTime;
    }
    public float GetRightTime()
    {
        return rightTime;
    }

    public void SetIsSmiley(bool isLeft, bool isSmiley) {
      if (isLeft) leftSmiley = isSmiley;
      else rightSmiley = isSmiley;
    }

    /// <summary>
    /// Postcondition: [side]FaceColliding = isSideColliding, side is left or right, based on isLeft
    /// </summary>
    /// <param name="isLeft">Whether to change status of left or right side</param>
    /// <param name="isSideColliding">Whether this side is currently colliding with a frowny face</param>
    public void SetIsSideColliding(bool isLeft, bool isSideColliding)
    {
        if (isLeft)
        {
            leftFaceColliding = isSideColliding;
            HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.LEFT,
                HappyTimesSmartFeedback.NEITHER, leftFaceColliding);
        }
        else
        {
            rightFaceColliding = isSideColliding;
            HappyTimesSmartFeedback.TriggerEvent(HappyTimesSmartFeedback.RIGHT,
                HappyTimesSmartFeedback.NEITHER, rightFaceColliding);
        }
    }
}
