using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchZoneScript : MonoBehaviour
{
    GameObject opposingPaddle;
    public bool isLeft;

    // Start is called before the first frame update
    void Start()
    {
        float camHeight = Camera.main.orthographicSize;
        float horizontalPosition = Camera.main.aspect * camHeight;

        if (isLeft)
            horizontalPosition = horizontalPosition * -1;

        transform.position = new Vector2(horizontalPosition, 0);

        BoxCollider2D coll = gameObject.AddComponent<BoxCollider2D>();
        coll.size = new Vector2(0.1f, camHeight * 2);
        coll.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
