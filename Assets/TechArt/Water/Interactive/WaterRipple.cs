using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRipple : MonoBehaviour
{
    public GameObject Player;
    public GameObject Water;
    public GameObject Ripple;
    public float WaitTime = 1f;
    private float timelock = 0;
    void Awake()
    {
        FrameEvent.Instance.On(FrameEvent.CreateWorldPlayer, OnCreateWorldPlayer, this);
    }

    private void Update()
    {
        if (Player == null || Water == null || Ripple == null)
        {
            return;
        }
        Ripple.transform.position = new Vector3(Player.transform.position.x, Water.transform.position.y, Player.transform.position.z);

        if (Player.transform.position.y < Water.transform.position.y)
        {
            Ripple.SetActive(true);
            timelock = Time.time + WaitTime;
        }
        else
        {
            if (Time.time > timelock)
            {
                Ripple.SetActive(false);
            }
        }

    }
    private void OnCreateWorldPlayer(object[] args)
    {
        Player = args[0] as GameObject;
        if (Player == null)
        {
            Debug.LogError("WorldEnemy: Player object is null.");
            return;
        }
    }
}
