using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Class <c>PlayerControllerScript</c> defines behavior for Player GameObject.
/// </summary>
public class MazePlayerControllerScript : MonoBehaviour
{
    /// <summary>
    /// Reference to the GameController GameObject.
    /// </summary>
    public GameObject gameCont;
    /// <summary>
    /// Script attached to maze game controller
    /// </summary>
    private MazeNewGameController gameContScript;
    /// <summary>
    /// The conversion rate of force to velocity.
    /// </summary>
    public float velocityConversionRate;
    /// <summary>
    /// The conversion rate of force to angle.
    /// </summary>
    public float angleConversionRate;
    /// <summary>
    /// The rigid body component of the player object.
    /// </summary>
    private Rigidbody2D rb;
    /// <summary>
    /// The current percentage of maximum force applied to the left hand controller.
    /// </summary>
    private float leftForceRatio;
    /// <summary>
    /// The current percentage of maximum force applied to the right hand controller.
    /// </summary>
    private float rightForceRatio;
    /// <summary>
    /// The current velocity that the player is moving at.
    /// </summary>
    private float velocity;
    /// <summary>
    /// The player's angle relative to the normal.
    /// </summary>
    private float angle;
    /// <summary>
    /// Whether or not to switch hand grip inputs in the current stage
    /// </summary>
    private bool switchHands = false;
    /// <summary>
    /// Whether or not to start using player input data
    /// </summary>
    private bool hasReachedFloor = false;
    /// <summary>
    /// Controls when player sprite flashes
    /// </summary>
    private LoopTimer flashTimer;
    /// <summary>
    /// The sprite renderer
    /// </summary>
    private SpriteRenderer rend;
    private AudioSource audioSrc;

    /// <summary>
    /// The percentage of force that constitutes a floor or near floor
    /// </summary>
    private const float FLOOR_PERCENTAGE = 0.05f;
    private const float ACCELERATION_PERCENTAGE = 0.05f;

    /// <summary>
    /// Method <c>Awake</c> called by engine as part of startup process. Used to
    /// initialize important values.
    /// </summary>
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSrc = GetComponent<AudioSource>();
        gameContScript = gameCont.GetComponent<MazeNewGameController>();
        flashTimer = new LoopTimer(MakeVisible, SwitchVisibility, 0.6f, 0.1f);
        rend = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Used by outside entities to apply a grip input switch
    /// </summary>
    //public void SwitchControls()
    //{
        //switchHands = !switchHands;
    //}

    /// <summary>
    /// Method <c>Update</c> called by engine every update cycle. Used to check for keypresses or
    /// other input and update player status variables accordingly.
    /// </summary>
    public void Update()
    {
        //skip update loop if paused
        if (Time.timeScale == 0)
        {
            return;
        }

        //read percentage of maximum force input that is currently applied from arduino sensor
        //We multiply denominator of proportion by 0.5 to ceil at half force for more precision requirements
        leftForceRatio = Mathf.Min(_GlobalVariables.leftForce / (_GlobalVariables.maxGripStrength[0] * 0.5f), 1f);
        rightForceRatio = Mathf.Min(_GlobalVariables.rightForce / (_GlobalVariables.maxGripStrength[1] * 0.5f), 1f);

        if (!hasReachedFloor)
        {
            float appliedForce = _GlobalVariables.leftForce + _GlobalVariables.rightForce;
            
            if (appliedForce <= _GlobalVariables.maxGripStrength.Sum(m => m) * FLOOR_PERCENTAGE) {
                hasReachedFloor = true;
            } else {
                return;
            }
        }

        //if the switch trigger has been set then we just switch the values for left and right hand grips
        //if (switchHands)
        //{
            //float temp = leftForceRatio;
            //leftForceRatio = rightForceRatio;
            //rightForceRatio = temp;
        //}

        if (leftForceRatio >= ACCELERATION_PERCENTAGE && rightForceRatio >= ACCELERATION_PERCENTAGE) {
            float accelerationRatio = (leftForceRatio + rightForceRatio) / 2;
            velocity = accelerationRatio * velocityConversionRate;  //velocity is proportional to left force
            float xComponent = velocity * Mathf.Sin(angle * Mathf.Deg2Rad);
            float yComponent = velocity * Mathf.Cos(angle * Mathf.Deg2Rad);

            rb.velocity = new Vector2(-1 * xComponent, yComponent);
        } else {
            rb.velocity = new Vector2(0, 0);

            float angleAdditionLeft = leftForceRatio * angleConversionRate;
            angle += angleAdditionLeft;

            float angleAdditionRight = rightForceRatio * angleConversionRate;
            angle -= angleAdditionRight;

            angle %= 360;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }


        //if (gameContScript.GetStage() < 2)
            //MazeSmartFeedbackProcessor.PlayerTurn(angleAddition);
        //else
            //MazeSmartFeedbackProcessor.SwitchedPlayerTurn(angleAddition);

        Vector3 old = transform.position;
        transform.position = new Vector3(old.x, old.y, -2);

        //update the loop timer
        flashTimer.Update(Time.deltaTime);
    }

    public void SetPosRandomRotation(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, -2);
        angle = Random.Range(0f, 360f);
        transform.eulerAngles = new Vector3(0, 0, angle);
        rb.velocity = new Vector2();
        flashTimer.Pause();
        hasReachedFloor = false;
    }

    public void ObstacleHit()
    {
        audioSrc.Play();
        SwitchVisibility();
        flashTimer.Reset();
    }

    public void SwitchVisibility()
    {
        rend.enabled = !rend.enabled;
    }

    public void MakeVisible()
    {
        rend.enabled = true;
    }
}
