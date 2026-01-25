using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BoxCollider2D bossDoor;
    public int score;
        
    void Update()
    {
        if (score == 5)
        {
            bossDoor.enabled = true;
        }
    }
}
