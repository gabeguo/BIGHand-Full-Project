using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrateMaxForceIcon : MonoBehaviour
{
    private bool reset = false;
    private float elapsedTime = 0;
    private Quaternion q1, q2, q3;

    private void Start()
    {
        q1 = Quaternion.Euler(0f, 0f, 0f);
        q2 = Quaternion.Euler(0f, 0f, 180.1f);
        q3 = Quaternion.Euler(0f, 0f, 179.9f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(q1, q2, elapsedTime);
        elapsedTime += Time.deltaTime;

        if (elapsedTime > 1f)
        {
            elapsedTime = 0;
            Quaternion temp = q1;

            if (reset)
            {
                q1 = q2;
                q2 = q3;
                q3 = temp;
                reset = false;
            }
            else
            {
                q1 = q3;
                q3 = q2;
                q2 = temp;
                reset = true;
            }
        }
    }
}
