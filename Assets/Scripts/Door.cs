using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject zoneToClose;
    public GameObject zoneToOpen;
    public Transform player;
    public Vector3 transportPosition;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player.position = transportPosition;
            zoneToClose.SetActive(false);
            zoneToOpen.SetActive(true);
        }
    }
}
