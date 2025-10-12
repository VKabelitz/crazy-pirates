using UnityEngine;

public class BuildableSpot : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tower"))
        {
            TowerPlaceManager.Instance.canBePlaced = true;
            Debug.Log("Tower in buildable spot");
            
            // You can add additional logic here, such as allowing placement
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tower"))
        {
            TowerPlaceManager.Instance.canBePlaced = false;
            Debug.Log("Tower in not buildable spot");
            // You can add additional logic here, such as allowing placement
        }
    }
}
