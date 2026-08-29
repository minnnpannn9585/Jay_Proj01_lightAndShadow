using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed;
    public GameObject weapon01, weapon02;

    private bool isWeapon01 = false;
    private bool isweapon02 = false;
    public GameObject bullet01;
    public GameObject bullet02;
    public float shootSpeed;
    
    private Vector3 mousePos;
    public SpriteRenderer sr;
    public Animator anim;
    
    void Update()
    {
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;
        rb.velocity = dir * moveSpeed;
        if (rb.velocity.x < 0f)
        {
            sr.flipX = true;
        }
        else if (rb.velocity.x > 0f)
        {
            sr.flipX = false;
        }

        anim.SetBool("isWalking", dir.sqrMagnitude > 0f);
        
        mousePos = new Vector3(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
            0f);

        SwitchWeapon();
        Shoot();
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 dir = mousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            if (isWeapon01)
            {
                GameObject bulletOne = Instantiate(bullet01, transform.position, rotation);
                bulletOne.GetComponent<Rigidbody2D>().velocity = dir.normalized * shootSpeed;
                
            }
            else if (isweapon02)
            {
                GameObject bulletTwo = Instantiate(bullet02, transform.position, rotation);
                bulletTwo.GetComponent<Rigidbody2D>().velocity = dir.normalized * shootSpeed;
            }
        }
    }

    private void SwitchWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weapon01.SetActive(true);
            weapon02.SetActive(false);
            isWeapon01 = true;
            isweapon02 = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weapon01.SetActive(false);
            weapon02.SetActive(true);
            isWeapon01 = false;
            isweapon02 = true;
        }
    }
}
