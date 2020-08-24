using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenPigsFlyPileOfCoinsScript : MonoBehaviour
{
    /// <summary>
    /// The coin displays corresponding to the cumulative performances throughout the game
    /// </summary>
    public Sprite pileSprite;
    /// <summary>
    /// The rate at which the pile grows every half second
    /// </summary>
    public float pileGrowthRate;

    /// <summary>
    /// The SpriteRenderer for this object, so that we don't have to keep calling it and wasting execution time
    /// </summary>
    private SpriteRenderer rend;
    /// <summary>
    /// Vector2 representation of height and width of camera
    /// </summary>
    private Vector2 cameraSize;
    /// <summary>
    /// The current percentage of the normal scale that the pile sprite will be
    /// </summary>
    private float scalePercentage = INITIAL_SCALE_PERCENTAGE;
    /// <summary>
    /// The scale percentage that each stage starts at
    /// </summary>
    private const float INITIAL_SCALE_PERCENTAGE = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        rend = this.GetComponent<SpriteRenderer>();
        rend.sprite = pileSprite;

        float cameraHeight = Camera.main.orthographicSize;
        cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);

        RePositionReScale();
    }

    /// <summary>
    /// Changes the display based on the ratio of the points the player has accumulated and the total possible points the player could have accumulated
    /// </summary>
    /// <param name="possiblePointPercent">Ratio of the points the player has accumulated to the total possible points</param>
    public void changeDisplay(float possiblePointPercent)
    {
        //Debug.Log("Success rate: " + possiblePointPercent);
        if (possiblePointPercent < 0.15)
        {
            //do nothing
        }
        else if (possiblePointPercent < 0.33)
        {
            scalePercentage += pileGrowthRate;
        }
        else if (possiblePointPercent < 0.66)
        {
            scalePercentage += 2 * pileGrowthRate;
        }
        else
        {
            scalePercentage += 3 * pileGrowthRate;
        }

        RePositionReScale();
    }

    /// <summary>
    /// Sets a proper scale and position for the sprite as it grows, called
    /// every half second by changeDisplay and start function
    /// </summary>
    private void RePositionReScale()
    {
        Vector2 spriteSize = rend.sprite.bounds.size;

        float scale = cameraSize.x / spriteSize.x * scalePercentage;
        transform.localScale = new Vector2(scale, scale);

        //Note: x value stays the same while y value is a function of the half height of the sprite in relation to screen height
        float halfHeightDiff = (spriteSize.y * scale / 2 - cameraSize.y) * 0.95f;
        Vector3 matchBottomPos = new Vector3(transform.position.x, halfHeightDiff, 1);

        transform.position = matchBottomPos;
    }

    public void ResetScalePercentage()
    {
        scalePercentage = INITIAL_SCALE_PERCENTAGE;
        RePositionReScale();
    }
}
