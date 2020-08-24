using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HappyTimesFrownyController : MonoBehaviour
{
    private const float TRAVEL_TIME = 3.0f;
    private const int PRE_TRIGGER_STAGE = 0, POST_TRIGGER_START_STAGE = 1, POST_TRIGGER_END_STAGE = 2;

    private HappyTimesGameController gameController;
    private HappyTimesBorderScript border;
    private Vector3 initialPosition;
    private Vector3? endPosition = null;
    private bool[] stageTruthMap = { false, true, false };
    private bool isLeft;
    private bool doneChecking;  //true when smiley face
    private int triggerStage;
    private float elapsedTime;

    public Sprite[] badSprites, goodSprites;

    private SpriteRenderer rend;

    public void Initialize(HappyTimesGameController gc, HappyTimesBorderScript bs)
    {
        //Debug.Log("Frowny face initialized");
        gameController = gc;
        border = bs;

        isLeft = transform.position.x < 0;
        initialPosition = transform.position;

        float viewPortHeight = 2 * Camera.main.orthographicSize;
        rend = GetComponent<SpriteRenderer>();
        float spriteHeight = rend.sprite.bounds.size.y;
        endPosition = initialPosition - new Vector3(0, viewPortHeight + spriteHeight, 0);

        rend.sprite = badSprites[Random.Range(0, badSprites.Length)];

        gameController.IncrementPossibleNumFaces(isLeft);
    }

    /// <summary>
    /// Returns without functioning of the game is paused or the endposition has not been set, used
    /// priamrily to progress the position of the gameObject on the screen
    /// </summary>
    void Update()
    {
        if (Time.timeScale == 0 || endPosition == null)
            return;

        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(initialPosition, (Vector3)endPosition, elapsedTime / TRAVEL_TIME);

        if (elapsedTime >= TRAVEL_TIME)
            Destroy(gameObject);
    }

    private bool GetThisSideActive()
    {
        return isLeft ? border.GetLeftActive() : border.GetRightActive();
    }

    /// <summary>
    /// Changes sprite to smiley face.
    /// Also adds one to count of successes in happy times game controller.
    /// </summary>
    private void SetToSmiley()
    {
        rend.sprite = goodSprites[Random.Range(0, goodSprites.Length)];
        doneChecking = true;
        gameController.IncrementFacesCaught(isLeft);
        //border.hitAudio.Play();
    }

	  private void OnTriggerEnter2D(Collider2D collider)
    {
        //assume that collider is border
        collider.gameObject.GetComponent<HappyTimesBorderScript>().SetIsSideColliding(isLeft, true);
        //Debug.Log("Collision");
        if (GetThisSideActive())
            triggerStage = PRE_TRIGGER_STAGE;
        else
            triggerStage = POST_TRIGGER_START_STAGE;

        border.SetIsSmiley(this.isLeft, false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (doneChecking)
            return;

        if (triggerStage >= POST_TRIGGER_END_STAGE + 1) {
            //collision.gameObject.GetComponent<HappyTimesBorderScript>().SetIsSideColliding(isLeft, false);
            SetToSmiley();
        } else if (GetThisSideActive() == stageTruthMap[triggerStage]) { //TODO: meaning of this line? Seems to be causing errors
            triggerStage += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collision.gameObject.GetComponent<HappyTimesBorderScript>().SetIsSideColliding(isLeft, false);
        if (!doneChecking)
        { //never became smiley, with no possibility whatsoever
          border.missAudio.Play();
        }
        border.SetIsSmiley(this.isLeft, false);
    }
}
