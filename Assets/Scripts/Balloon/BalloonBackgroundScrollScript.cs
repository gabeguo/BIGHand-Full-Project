using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonBackgroundScrollScript : MonoBehaviour
{
    public Sprite slice;
    public int offset;

    private const int NUM_SLICES = 12;
    private const float Z_POS = 2;
    private const float TIME_TO_TRAVEL = 1000;
    private const float OVERLAP_AMOUNT = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Vector2 cameraSize, spriteSize, spriteScale;
    private Vector3 leftMost, rightMost;
    private float horizontalSpeed;
    private float modifiedSpriteSize;

    // Start is called before the first frame update
    void Start()
    {
        float cameraHeight = Camera.main.orthographicSize * 2;
        cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = slice;

        spriteSize = spriteRenderer.sprite.bounds.size;
        modifiedSpriteSize = spriteSize.x - OVERLAP_AMOUNT;

        AdjustScale();
        CalculateEndPoints();
        CalculateSpeed();
        GoToStartPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale <= 0)
            return;

        transform.position -= new Vector3(horizontalSpeed, 0, 0);
        Vector3 pos = transform.position;
        if (pos.x <= leftMost.x)
        {
            Vector3 newPos = new Vector3(CustomFloor(rightMost.x + (pos.x - leftMost.x)), 0, Z_POS);
            transform.position = newPos;
        }
    }
    
    private void AdjustScale()
    {
        spriteScale = transform.localScale;
        spriteScale *= cameraSize.y / spriteSize.y;
        transform.localScale = spriteScale;
    }

    private void GoToStartPosition()
    {
        //Debug.Log("Left Most: " + leftMost.x);
        //Debug.Log("Sprite Size: " + spriteSize.x);
        //Debug.Log("Sprite Scale: " + spriteScale.x);
        float newXValue = CustomFloor(leftMost.x + ((offset + 1) * modifiedSpriteSize * spriteScale.x));
        //Debug.Log("New X Value: " + newXValue);
        transform.position = new Vector3(newXValue, 0, Z_POS);
    }

    private void CalculateEndPoints()
    {
        leftMost = new Vector3((-cameraSize.x - (spriteSize.x * spriteScale.x)) / 2, 0, Z_POS);
        rightMost = new Vector3(leftMost.x + (NUM_SLICES) * modifiedSpriteSize * spriteScale.x, 0, Z_POS);
    }

    private void CalculateSpeed()
    {
        horizontalSpeed = (rightMost.x - leftMost.x) / TIME_TO_TRAVEL;
    }

    private float CustomFloor(float val)
    {
        //int integerVal = (int)(val * 1000);
        //return integerVal / 1000f;
        return val;
    }
}
