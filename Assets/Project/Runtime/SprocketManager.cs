using UnityEngine;

public class SprocketManager : MonoBehaviour
{
    public static SprocketManager instance;
    public int currentSprocketAmount = 0; // aktueller Betrag an Sprockets
      private void Awake()
    {
        instance = this;
    }
    public void AddSprockets(int sprocketAmount)
    {
        //UIManager.instance.AddSprocket(sprocketAmount);

        currentSprocketAmount += sprocketAmount;
        UIManager.instance.UpdateSprockets(currentSprocketAmount);

    }
    public void SubstractSprocket(int sprocketAmount)
    {
        if (currentSprocketAmount - sprocketAmount >= 0)
        {
            currentSprocketAmount -= sprocketAmount;
            UIManager.instance.UpdateSprockets(currentSprocketAmount);
        }
        Debug.Log("Not enough sprockets!");
        
    }
}
