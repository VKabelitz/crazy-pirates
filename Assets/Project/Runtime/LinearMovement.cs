using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LinearMovement : BaseMovement
{
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    private int currentWayPointIndex = 0;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    private void Start()
    {
        GameObject pathObject = GameObject.FindWithTag("WayPoints");
        if (pathObject != null)
        {
            WayPointManager wayPointManager = pathObject.GetComponent<WayPointManager>();
            if (wayPointManager != null)
            {
                wayPoints = wayPointManager.GetWayPoints();
                Debug.Log("Waypoints loaded: " + wayPoints.Count);
            }
        }
    }

    public void FixedUpdate()
    {
        if (currentWayPointIndex < wayPoints.Count)
        {
            MoveToNextWayPoint();
        }
        else
        {
            // Damage
            currentWayPointIndex = 0;
        }
    }

    private void MoveToNextWayPoint()
    {
        Vector3 current = transform.position;
        Vector3 targetPos = wayPoints[currentWayPointIndex].position;

        targetPos.y = current.y; // Keep the same height
        
        transform.position = Vector3.MoveTowards(current, targetPos, Time.deltaTime * movementSpeed);

        // Rotation
        Vector3 direction = targetPos - current;

        var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        var distance = Vector3.Distance(current, targetPos);

        if (distance < 0.05f)
        {
            currentWayPointIndex++;
        }
    }
}
