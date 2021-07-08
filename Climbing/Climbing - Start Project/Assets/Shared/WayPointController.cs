using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointController : MonoBehaviour
{
    WayPoint[] waypoints;
    int currentWayPointIndex = -1;

    public event System.Action<WayPoint> OnWayPointChanged;
    private void Awake()
    {
        waypoints = GetWayPoints();
    }

    public void SetNextWayPoint()
    {
       
        currentWayPointIndex++;

        if (currentWayPointIndex == waypoints.Length)
            currentWayPointIndex = 0;

        if (OnWayPointChanged != null)
            OnWayPointChanged(waypoints[currentWayPointIndex]);

    }
   WayPoint[] GetWayPoints()
    {
       return GetComponentsInChildren<WayPoint>(); 
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 previousWayPoint = Vector3.zero;

        foreach(var waypoint in GetWayPoints())
        {
            Vector3 wayPointPosition = waypoint.transform.position;
            Gizmos.DrawWireSphere(wayPointPosition, 0.2f);
            if (previousWayPoint != Vector3.zero)
            {
                Gizmos.DrawLine(previousWayPoint, wayPointPosition);

            }
            previousWayPoint = wayPointPosition;
        }
    }
}
