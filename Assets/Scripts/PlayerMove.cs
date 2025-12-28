using System;
using System.Collections;
using System.Collections.Generic;
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
    
    private Vector3 mousePos;
    
    void Update()
    {
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;
        rb.velocity = dir * moveSpeed;
        
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
            if (isWeapon01)
            {
                GameObject bulletOne = Instantiate(bullet01, transform.position, Quaternion.identity);
                bulletOne.GetComponent<Rigidbody2D>().velocity = dir.normalized * 2f;
                
            }
            else if (isweapon02)
            {
                GameObject bulletTwo = Instantiate(bullet02, transform.position, Quaternion.identity);
                bulletTwo.GetComponent<Rigidbody2D>().velocity = dir.normalized * 2f;
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
