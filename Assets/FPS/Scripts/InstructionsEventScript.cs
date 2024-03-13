using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsEventScript : MonoBehaviour
{
    private GameObject child;
    private void Start()
    {
        child = gameObject.transform.GetChild(0).gameObject;
        child.SetActive(false);
    }
    private void OnEnable()
    {
        PlayerInteraction.OnPlayerNearBossRoom += PlayerNearbyBossRoom;
        PlayerInteraction.OnPlayerFarFromBossRoom += PlayerFarFromBossRoom;
    }
    private void OnDisable()
    {
        PlayerInteraction.OnPlayerNearBossRoom -= PlayerNearbyBossRoom;
        PlayerInteraction.OnPlayerFarFromBossRoom -= PlayerFarFromBossRoom;
    }
    private void PlayerFarFromBossRoom(object sender, EventArgs e)
    {
        child.SetActive(false);
    }

    private void PlayerNearbyBossRoom(object sender, EventArgs e)
    {
        child.SetActive(true);
    }
}
