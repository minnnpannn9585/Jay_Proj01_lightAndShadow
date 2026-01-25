using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemy : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("LightBullet"))
        {
            if (GameObject.Find("LevelManager") != null)
            {
                GameObject.Find("LevelManager").GetComponent<LevelManager>().score++;
            }
            
            Destroy(this.gameObject);
        }
    }
}
