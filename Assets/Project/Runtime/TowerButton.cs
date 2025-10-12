using UnityEngine;
using UnityEngine.UI;

public class Towerbutton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] private int towerCost;
    public void CheckIfAffordable()
    {
        if (SprocketManager.instance.currentSprocketAmount >= towerCost)
        {
            Debug.Log("Affordable, Current Sprockets: " + SprocketManager.instance.currentSprocketAmount + " Tower Cost: " + towerCost);
            button.interactable = true;
        }
        else
        {
            Debug.Log("Not Affordable, Current Sprockets: " + SprocketManager.instance.currentSprocketAmount + " Tower Cost: " + towerCost);
            button.interactable = false;
        }
    }

    

}
