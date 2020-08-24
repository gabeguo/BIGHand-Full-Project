using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public float startVelocity;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        float xComponent = startVelocity * Mathf.Sin(65 * Mathf.Deg2Rad);
        float yComponent = startVelocity * Mathf.Cos(65 * Mathf.Deg2Rad);

        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(xComponent, yComponent);
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.magnitude > 0)
            rb.velocity = rb.velocity / rb.velocity.magnitude * startVelocity;
    }
}
