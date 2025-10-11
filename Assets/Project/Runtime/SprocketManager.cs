using UnityEngine;

public class SprocketManager : MonoBehaviour
{
    public static SprocketManager instance = null;
    public int sprocketAmount;
    public int currentSprocketAmount; // Anzahl der Sprockets die Spieler im Moment hat

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    public void AddSprocket(int sprocketAmount)
    {
        UIManager.instance.AddSprocket(sprocketAmount);
        currentSprocketAmount += sprocketAmount;
    }
    public void SubtractSprocket(int sprocketAmount)
    {
        UIManager.instance.SubstractSprocket(sprocketAmount);
        currentSprocketAmount -= sprocketAmount;
    }
}
