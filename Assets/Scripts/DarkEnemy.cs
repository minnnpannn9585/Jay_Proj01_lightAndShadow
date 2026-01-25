using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkEnemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DarkBullet"))
        {
            if (GameObject.Find("LevelManager") != null)
            {
                GameObject.Find("LevelManager").GetComponent<LevelManager>().score++;
            }
            Destroy(this.gameObject);
        }
    }
}
