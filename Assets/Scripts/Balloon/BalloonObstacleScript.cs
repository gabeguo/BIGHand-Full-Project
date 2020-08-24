using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonObstacleScript : MonoBehaviour
{
    /// <summary>
    /// The game object attached to the game controller script
    /// </summary>
    private BalloonGameController gameController;
    /// <summary>
    /// The sprite renderer used by the game object that this script
    /// is attached to, used for size readings
    /// </summary>
    SpriteRenderer rend;
    /// <summary>
    /// The position that the sprite will spawn at [Right side of screen,
    /// a couple of pixels off from the edge so it's completely out of view]
    /// </summary>
    private Vector3 startPos;
    /// <summary>
    /// The position that the sprite will end at, in most cases this is going to
    /// be startPos only mirrored horizontally
    /// </summary>
    private Vector3 endPos;
    /// <summary>
    /// The amount of time elapsed since the object has been spawned in, used for lerping
    /// </summary>
    private float elapsedTime;
    /// <summary>
    /// The time that the game object should take to travel across the screen
    /// </summary>
    private float travelTime;
    /// <summary>
    /// The height of the top of the building
    /// </summary>
    private float totalBuildingHeight;
    /// <summary>
    /// Whether or not the game controller has decided to spawn this game object
    /// </summary>
    private bool spawned;

    public BalloonCloudController cloudController;

    /// <summary>
    /// Game loop start function, just used for some basic initialization logic
    /// </summary>
    void Awake()
    {
        spawned = false;
        rend = gameObject.GetComponent<SpriteRenderer>();
        transform.position = CalculatePositionOutsideBounds();
    }

    /// <summary>
    /// Used to set spawned field to true and initialize start and end positions for lerping across the screen
    /// </summary>
    /// <param name="topHeight">The height that the top of this obstacle should reach (counting the bottom of the viewport as 0)</param>
    /// <param name="timeToTraverseScreen">The amount of time it should take this obstacle to lerp across the entire viewport horizontally</param>
    public void Spawn(float topHeight, float timeToTraverseScreen, GameObject gameControllerParam)
    {
        transform.parent = gameControllerParam.transform;
        gameController = gameControllerParam.GetComponent<BalloonGameController>();
        cloudController.InitGC(gameController);
        totalBuildingHeight = topHeight;

        startPos = CalculateStartPos(topHeight);
        //endPos is essentially just a horizontal flip of startPos because (0,0) is at center in Unity
        endPos = new Vector3(-startPos.x, startPos.y, 1);
        travelTime = timeToTraverseScreen;

        spawned = true;
    }

    /// <summary>
    /// Game loop update function, used to progress lerp once spawned field is set to true
    /// </summary>
    void Update()
    {
        //Do not execute if timeScale is 0 (game is paused) or the gameObject has not been spawned yet
        if (!spawned || Time.timeScale == 0)
            return;

        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / travelTime);

        //Once the lerp is complete, the obstacle is off the screen and can be safely destroyed
        if (elapsedTime >= travelTime)
            Destroy(gameObject);
    }

    /// <summary>
    /// Registers collisions with the balloon and sends collision data back to game controller for recording
    /// </summary>
    /// <param name="c">Information about the collision that occured</param>
    void OnTriggerEnter2D(Collider2D c)
    {
        BalloonPlayerController bpc = c.gameObject.GetComponent<BalloonPlayerController>();
        if (bpc != null)
            bpc.ObstacleHit();

        float xValue = transform.position.x;
        float yValue = -1f * Camera.main.orthographicSize + totalBuildingHeight / 2.0f;
        Vector2 mockBuildingPos = new Vector2(xValue, yValue);

        gameController.RecordCollision(mockBuildingPos, c.gameObject.transform.position, false);
    }

    /// <summary>
    /// Calculates the start position of the game object before the desired game height is known, this is simply so that the obstacle
    /// is not in the viewport until the spawn function is triggered and a proper position is set
    /// </summary>
    /// <returns>The temporary position that the obstacle should be placed</returns>
    private Vector3 CalculatePositionOutsideBounds()
    {
        float yValue = 0;

        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float spriteWidth = rend.sprite.bounds.size.x * transform.localScale.x;

        //Add some constant to make sure texture doesn't accidentally clip on to scene
        float xValue = cameraWidth + spriteWidth + 5;

        return new Vector3(xValue, yValue, 1);
    }

    /// <summary>
    /// Once the desired height is known (via the spawn method) this function should be called to determine the true
    /// starting position for the obstacle
    /// </summary>
    /// <param name="topHeight">The height that the top of the obstacle should reach</param>
    /// <returns>The position that the obstacle should be placed at to achieve the desired height</returns>
    private Vector3 CalculateStartPos(float topHeight)
    {
        float xValue = transform.position.x;

        float cameraHeight = Camera.main.orthographicSize;
        float spriteHeight = rend.sprite.bounds.size.y * transform.localScale.y;

        // --NOTE: May instead want to change local scale property to make size malfunctions impossible
        if (topHeight > spriteHeight)
            throw new System.InvalidOperationException("This sprite is too small for the required height");

        float spriteHeightDifference = spriteHeight - topHeight;
        float yValue = (spriteHeight / 2) - cameraHeight - spriteHeightDifference;

        return new Vector3(xValue, yValue, 1);
    }

	private void OnDestroy()
	{
        gameController.IncrementDestroyedObstacleCounter();
	}
}
