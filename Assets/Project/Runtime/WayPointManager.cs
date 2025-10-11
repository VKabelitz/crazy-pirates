using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    [SerializeField] private bool isMoving;
    private int currentWayPointIndex = 0;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
            return;

        if (currentWayPointIndex < wayPoints.Count)
        {
            MoveToNextWayPoint();
        }
        else
        {
            // Damage
            isMoving = false;
            currentWayPointIndex = 0;
        }
    }
    
    private void MoveToNextWayPoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, wayPoints[currentWayPointIndex].position, Time.deltaTime * speed);

        // Rotation
        var direction = transform.position - wayPoints[currentWayPointIndex].position;
        var targetRotation = Quaternion.LookRotation(-direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        var distance = Vector3.Distance(transform.position, wayPoints[currentWayPointIndex].position);
        if (distance < 0.05f)
        {
            currentWayPointIndex++;
        }
    }
}
