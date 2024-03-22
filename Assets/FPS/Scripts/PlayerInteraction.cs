using System;
using UnityEngine;
public class PlayerInteraction : MonoBehaviour
{
    public static event EventHandler OnPlayerNearBossRoom;
    public static event EventHandler OnPlayerFarFromBossRoom;

    private GameObject bossInstance;
    private GameObject player;
    private float moveDistance = 2.0f;
    private float interactionDistance = 2.2f;
    private bool isPlayerNearBossRoom = false;
    public void SetReferences(GameObject bossInstance, GameObject player)
    {
        this.bossInstance = bossInstance;
        this.player = player;
    }
    private void Update()
    {
        float distanceToPlayer = GetDistanceToPlayer();
        if (isPlayerNearBossRoom && Input.GetKeyDown(KeyCode.F))
        {
            TeleportPlayerToBossRoom();
        }
        if (distanceToPlayer <= interactionDistance && !isPlayerNearBossRoom)
        {
            isPlayerNearBossRoom = true;
            OnPlayerNearBossRoom?.Invoke(this, EventArgs.Empty);
        }
        else if (distanceToPlayer > interactionDistance && isPlayerNearBossRoom)
        {
            isPlayerNearBossRoom = false;
            OnPlayerFarFromBossRoom?.Invoke(this, EventArgs.Empty);
        }
    }
    private float GetDistanceToPlayer()
    {
        Collider collider = GetComponent<Collider>();
        Vector3 closestPoint = collider.ClosestPoint(player.transform.position);
        return Vector3.Distance(closestPoint, player.transform.position);
    }
    private void TeleportPlayerToBossRoom()
    {
        Vector3 directionToBoss = bossInstance.transform.position - player.transform.position;
        directionToBoss.y = 0;

        player.transform.position += directionToBoss.normalized * moveDistance;
    }
}