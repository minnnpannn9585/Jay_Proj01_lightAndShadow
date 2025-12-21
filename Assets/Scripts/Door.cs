using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject zoneToClose;
    public GameObject zoneToOpen;
    public Transform player;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player.position = new Vector3(-7f, 0, 0);
            zoneToClose.SetActive(false);
            zoneToOpen.SetActive(true);
        }
    }
}
