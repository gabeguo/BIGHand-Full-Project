using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenPigsFlyPigScript : MonoBehaviour
{

    /// <summary>
    /// Sprite renderer for pig; stored so we don't have to search every time we want it
    /// </summary>
    private SpriteRenderer spriteRend;

    /// <summary>
    /// Number in range (0.0, 1.0]
    /// that max force is multiplied by
    /// <c>maxForce * strengthFactor</c> is the force it takes to reach the edge
    /// Created so that users don't get too tired
    /// Can be changed based on difficulty
    /// </summary>
    public float strengthFactor;

    /// <summary>
    /// Prevents pig from going off screen at corners
    /// </summary>
    private const float scaleFactor = 0.9f;

    /// <summary>
    /// Prevents necessity of force greater than some percentage of the maximum
    /// </summary>
    private const float forceCeiling = 0.3f;

    /// <summary>
    /// Amount of frames new direction must be held for before a flip occurs
    /// </summary>
    private const int necessaryDirCountValue = 2;

    /// <summary>
    /// Maximum forces needed in each hand to reach the edges
    /// calculated by <c>strengthFactor * _GlobalVariables.MaxGripStrength[i]</c>
    /// </summary>
    private float maxLeftForce, maxRightForce;

    /// <summary>
    /// Maximum x and y positions
    /// </summary>
    private float xMax, yMax;

    /// <summary>
    /// The x position in the previous frame
    /// </summary>
    private float prevX;

    /// <summary>
    /// The minimum change in x position before pig flips position
    /// </summary>
    private float dirThreshold;

    /// <summary>
    /// Number of frames since last direction change
    /// </summary>
    private int directionCounter;

    // Start is called before the first frame update
    void Start()
    {
        spriteRend = gameObject.GetComponent<SpriteRenderer>();

        //force needed to reach edge
        maxLeftForce = strengthFactor * _GlobalVariables.maxGripStrength[0];
        maxRightForce = strengthFactor * _GlobalVariables.maxGripStrength[1];

        //max pos
        yMax = Camera.main.orthographicSize;
        xMax = yMax * Screen.width / Screen.height;

        prevX = -xMax;  //start x
        dirThreshold = xMax * 0.005f;

        if (strengthFactor < 0 || strengthFactor > 1)
        {
            throw new System.ArgumentException("Invalid strength factor");
        }
    }

    // Update is called once per frame
    /// <summary>
    /// Calculates and sets new position based on force input
    /// </summary>
    void Update()
    {
        //Don't update on pause
        if (Time.timeScale == 0)
        {
            return;
        }

        float leftForce = _GlobalVariables.leftForce;   //controls y pos
        float rightForce = _GlobalVariables.rightForce; //controls x pos

        float yForcePercent = Mathf.Min(maxLeftForce * forceCeiling, leftForce) / (maxLeftForce * forceCeiling);
        float xForcePercent = Mathf.Min(maxRightForce * forceCeiling, rightForce) / (maxRightForce * forceCeiling);

        //calculate new position
        float x = scaleFactor * (xMax * 2 * xForcePercent - xMax);
        float y = scaleFactor * (yMax * 2 * yForcePercent - yMax);

        //detect horizontal direction of movement
        float dx = x - prevX;
        bool flipped = spriteRend.flipX;
        if ((flipped && dx > dirThreshold) || (!flipped && dx < -dirThreshold))
        {
            if (++directionCounter >= necessaryDirCountValue)
            {
                spriteRend.flipX = !flipped;
                directionCounter = 0;
            }
        }
        else
        {
            directionCounter = 0;
        }

        //set new 3D position
        transform.position = Vector3.Lerp(transform.position, new Vector3(x, y, -1), 0.1f);

        //store previous x position
        prevX = x;
    }
}
