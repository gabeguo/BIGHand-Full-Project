using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenPigsFlyTargetTriggerScript : MonoBehaviour
{
    public int circleNumber;

    private WhenPigsFlyGameController gameController;
    private CapsuleCollider2D capsule;

    public void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<WhenPigsFlyGameController>();
        capsule = GetComponent<CapsuleCollider2D>();
        float aspect = 1.0f / Camera.main.aspect;
        float width = circleNumber / 3.0f;
        capsule.size = new Vector2(width, aspect * width);
    }

	private void OnTriggerStay2D(Collider2D collision)
	{
        Debug.Log("Collision");
        gameController.IncrementOneSecondPoints();
	}
}
