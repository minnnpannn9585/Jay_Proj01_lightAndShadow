using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkEnemy : MonoBehaviour
{
    public GameObject hitSfx;
    public GameObject hitVfx;
    public GameObject enemyBullet;
    public float shootInterval = 3f;
    public float bulletSpeed = 4f;

    Transform player;
    float shootTimer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        shootTimer = shootInterval;
    }

    void Update()
    {
        if (player == null || enemyBullet == null)
        {
            return;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            ShootAtPlayer();
            shootTimer = shootInterval;
        }
    }

    void ShootAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(enemyBullet, transform.position, Quaternion.Euler(0f, 0f, angle));
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = dir * bulletSpeed;
        }
    }

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
