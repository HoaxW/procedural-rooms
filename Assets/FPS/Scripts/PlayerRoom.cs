using UnityEngine;

internal class PlayerRoom
{
    private Vector2Int bottomLeftAreaCorner;
    private Vector2Int topRightAreaCorner;
    private GameObject player;

    public PlayerRoom(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, GameObject player)
    {
        this.bottomLeftAreaCorner = bottomLeftAreaCorner;
        this.topRightAreaCorner = topRightAreaCorner;
        this.player = player;
    }
    public void MovePlayerToCenterOfThisRoom()
    {
        Vector3 playerPosition = new Vector3(
            (bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2f,
            0,
            (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2f
        );
        player.transform.position = playerPosition;
    }
}