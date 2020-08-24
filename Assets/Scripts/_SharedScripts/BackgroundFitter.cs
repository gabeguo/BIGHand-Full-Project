using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFitter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        Vector2 scale = transform.localScale;

        transform.localScale = new Vector2(scale.x * cameraSize.x / spriteSize.x, scale.y * cameraSize.y / spriteSize.y);

        //float halfWidthDiff = (spriteRenderer.sprite.bounds.size.x * scale.x - cameraHeight * Camera.main.aspect) / 2;
        //float halfHeightDiff = (spriteRenderer.sprite.bounds.size.y * scale.y - cameraHeight) / 2;
        //Vector3 matchBottomPos = new Vector3(halfWidthDiff, halfHeightDiff, 2);

        //transform.position = matchBottomPos;
    }
}
