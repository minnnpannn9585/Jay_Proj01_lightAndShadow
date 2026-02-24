using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkEnemy : MonoBehaviour
{
    public GameObject hitSfx;
    public GameObject hitVfx;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DarkBullet"))
        {
            if (GameObject.Find("LevelManager") != null)
            {
                GameObject.Find("LevelManager").GetComponent<LevelManager>().score++;
            }

            Instantiate(hitVfx, transform.position, Quaternion.identity);
            Instantiate(hitSfx, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
