using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Transform[] waypoints;
    int currentWaypoint = 0;
    public float speed;
    
    // Start is called before the first frame update
    void Start()
    {
        waypoints = new Transform[transform.parent.childCount - 1];
        for (int i = 0; i < transform.parent.childCount - 1; i++)
        {
            waypoints[i] = transform.parent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, waypoints[currentWaypoint].position) <= 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
        Vector3 direction = (waypoints[currentWaypoint].position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
