using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPlayerController : MonoBehaviour
{
    public BalloonGameController gameCont;
    public float xPercentage;
    private bool useLeftHand = true;
    private SpriteRenderer rend;
    private Vector2 viewPort;
    private LoopTimer flashTimer;
    private float spriteHeight = 0;
    private float xPosition = 0;
    private AudioSource audioSrc;

    private void Start()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Camera.main.aspect;
        viewPort = new Vector2(width, height);
        xPosition = width * xPercentage - width / 2;

        rend = GetComponent<SpriteRenderer>();
        spriteHeight = rend.sprite.bounds.size.y;
        spriteHeight *= transform.localScale.y;

        flashTimer = new LoopTimer(MakeVisible, SwitchVisibility, 0.50f, 0.1f);

        audioSrc = GetComponent<AudioSource>();
    }

    public void SwitchHands()
    {
        useLeftHand = !useLeftHand;
    }

    void Update()
    {
        if (Time.timeScale == 0)
            return;

        flashTimer.Update(Time.deltaTime);

        float forceRatio = 0;
        if (useLeftHand)
        {
            gameCont.RegisterForce(_GlobalVariables.LEFT_INDEX, (int)_GlobalVariables.leftForce);
            forceRatio = Mathf.Min(_GlobalVariables.leftForce / (_GlobalVariables.maxGripStrength[0] * 1.05f), 1f);
        }
        else
        {
            gameCont.RegisterForce(_GlobalVariables.RIGHT_INDEX, (int)_GlobalVariables.rightForce);
            forceRatio = Mathf.Min(_GlobalVariables.rightForce / (_GlobalVariables.maxGripStrength[1] * 1.05f), 1f);
        }

        float adjustedHeightRange = viewPort.y - spriteHeight;
        float targetHeight = forceRatio * adjustedHeightRange - adjustedHeightRange / 2;
        Vector2 target = new Vector2(xPosition, targetHeight);

        transform.position = Vector2.Lerp(transform.position, target, 0.1f);
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
