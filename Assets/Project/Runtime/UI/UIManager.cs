using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    public TextMeshProUGUI sprocketText;
    public Slider healthbar;
    //public TextMeshProUGUI healthText;
    float healthProgress = 10;
    int sprockets = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        sprocketText.text = sprockets.ToString() + " Sprockets";
        //healthText.text = healthProgress.ToString();

    }

    void Update()
    {
        
    }

    public void AddSprocket(int amount)
    {
        sprockets += amount;
        //TODO: not always +1, different points for different enemies?
        sprocketText.text = sprockets.ToString() + " Sprockets";

    }
    public void UpdateSprockets(int currentSprocketAmount)
    {
        sprocketText.text = currentSprocketAmount.ToString() + " Sprockets";
    }

    public void UpdateHealth(int damage)
    {
        healthProgress -= damage;
        healthbar.value = healthProgress;
    }

}
