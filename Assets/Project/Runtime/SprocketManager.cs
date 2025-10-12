using UnityEngine;

public class SprocketManager : MonoBehaviour
{
    public static SprocketManager instance;
    public int currentSprocketAmount = 20; // aktueller Betrag an Sprockets

    private void Awake()
    {
        instance = this;

        /*Towerbutton[] buttons = FindObjectsOfType<Towerbutton>();
        foreach (Towerbutton button in buttons)
        {
            button.CheckIfAffordable();
        }*/
    }

    void Start()
    {
        UIManager.instance.UpdateSprockets(currentSprocketAmount);
        Towerbutton[] buttons = FindObjectsOfType<Towerbutton>();
        foreach (Towerbutton button in buttons)
        {
            button.CheckIfAffordable();
        }
    }

    public void AddSprockets(int sprocketAmount)
    {
        //UIManager.instance.AddSprocket(sprocketAmount);

        currentSprocketAmount += sprocketAmount;
        AudioManager.instance.PlaySound("coins");
        UIManager.instance.UpdateSprockets(currentSprocketAmount);
        Towerbutton[] buttons = FindObjectsOfType<Towerbutton>();
        foreach (Towerbutton button in buttons)
        {
            button.CheckIfAffordable();
        }
    }

    public void SubstractSprocket(int sprocketAmount)
    {
        if (currentSprocketAmount - sprocketAmount >= 0)
        {
            currentSprocketAmount -= sprocketAmount;
            UIManager.instance.UpdateSprockets(currentSprocketAmount);
            Towerbutton[] buttons = FindObjectsOfType<Towerbutton>();
            foreach (Towerbutton button in buttons)
            {
                button.CheckIfAffordable();
            }
        }
    }
}
