using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenPigsFlyCashDisplayScript : MonoBehaviour
{
    public GameObject cashPrefab;
    public GameObject bankSprite;
    public float bankWidthPercent;

    public BoxCollider2D leftCol, rightCol, botCol;
    private Vector2 cashSpawnLeftMost;
    private float cashSpawnMaxDist;
    private float cameraHalfHeight, cameraHalfWidth;

    private const float CASH_Z = 1f;
    private const float BOTTOM_PERCENT = 0.95f;
    private const float TOP_SPAWN_PERCENTAGE = 1.1f;
    private const float SPAWNABLE_WIDTH_PERCENTAGE = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = Camera.main.aspect * cameraHalfHeight;

        float spawnX = cameraHalfWidth * (1 - 2 * bankWidthPercent * SPAWNABLE_WIDTH_PERCENTAGE);
        float spawnY = cameraHalfHeight * TOP_SPAWN_PERCENTAGE;
        cashSpawnLeftMost = new Vector2(spawnX, spawnY);
        cashSpawnMaxDist = bankWidthPercent * 2 * SPAWNABLE_WIDTH_PERCENTAGE;

        PositionColliders();
        PositionBank();
    }

    private void PositionColliders()
    {
        botCol.offset = new Vector2(0, -cameraHalfHeight * BOTTOM_PERCENT);
        botCol.size = new Vector2(cameraHalfWidth * 2, 0.1f);

        leftCol.offset = new Vector2(cameraHalfWidth * (1 - 2 * bankWidthPercent), 0);
        leftCol.size = new Vector2(0.1f, 2 * cameraHalfHeight);

        rightCol.offset = new Vector2(cameraHalfWidth, 0);
        rightCol.size = new Vector2(0.1f, 2 * cameraHalfHeight);
    }

    private void PositionBank()
    {
        SpriteRenderer sr = bankSprite.GetComponent<SpriteRenderer>();
        float targetX = cameraHalfWidth * 2 * bankWidthPercent;
        float bankX = sr.sprite.bounds.size.x;
        float bankY = sr.sprite.bounds.size.y;
        float scaleFactor = targetX / bankX;
        float targetY = scaleFactor * bankY;

        Vector2 newScale = new Vector2(scaleFactor, scaleFactor);
        bankSprite.transform.localScale = newScale;

        float newXPos = cameraHalfWidth - (targetX / 2);
        float newYPos = -cameraHalfHeight + (targetY / 2);
        bankSprite.transform.position = new Vector2(newXPos, newYPos);
    }

    public void SpawnCash()
    {
        Vector3 spawnPos = cashSpawnLeftMost;
        spawnPos.x += Random.Range(0f, cashSpawnMaxDist);
        spawnPos.z = CASH_Z;

        Instantiate(cashPrefab, spawnPos, Quaternion.identity, transform);
    }

    public void ClearCash()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
