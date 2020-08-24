using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonCloudController : MonoBehaviour
{
    private BalloonGameController gameController;

    public void InitGC(BalloonGameController gameControllerParam)
    {
        gameController = gameControllerParam.GetComponent<BalloonGameController>();
    }
    
    void OnTriggerEnter2D(Collider2D c)
    {
        BalloonPlayerController bpc = c.gameObject.GetComponent<BalloonPlayerController>();
        if (bpc != null)
            bpc.ObstacleHit();

        //Player and building vectors unnecessary as we dont use that data for clouds
        gameController.RecordCollision(new Vector2(), new Vector2(), true);
    }
}
