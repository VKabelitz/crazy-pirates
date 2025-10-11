using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    private List<Transform> wayPoints = new List<Transform>();

    void Awake()
    {
        foreach (Transform child in transform)
        {
            wayPoints.Add(child);
            child.gameObject.SetActive(false);
        }
    }

    public List<Transform> GetWayPoints()
    {
        return wayPoints;
    }
}
