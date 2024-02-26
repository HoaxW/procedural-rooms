using UnityEngine;

public class BossRoom
{
    private Vector2Int bottomLeftAreaCorner;
    private Vector2Int topRightAreaCorner;
    private GameObject bossPrefab;
    private Transform parentTransform;

    public BossRoom(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, GameObject bossPrefab, Transform dungeonTransform)
    {
        this.bottomLeftAreaCorner = bottomLeftAreaCorner;
        this.topRightAreaCorner = topRightAreaCorner;
        this.bossPrefab = bossPrefab;
        this.parentTransform = dungeonTransform;
    }
    public void SpawnBoss()
    {
        // Spawns the boss in the middle of the room
        Vector3 spawnPosition = new Vector3(
            (bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2f,
            0,
            (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2f
        );
        GameObject.Instantiate(bossPrefab, spawnPosition, Quaternion.identity, parentTransform);
    }
}