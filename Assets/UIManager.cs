using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    public TextMeshProUGUI sprocketText;
    public Slider healthbar;
    int healthProgress = 100;
    int sprockets = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        sprocketText.text = sprockets.ToString() + " Sprockets";
    }

    void Update()
    {
        
    }

    public void AddSprocket() 
    {
        sprockets += 1;
        //TODO: not always +1, different points for different enemies?
        sprocketText.text = sprockets.ToString() + " Sprockets";

    }

    //TODO: Add point call in kill enemies


    public void UpdateHealth()
    {
        healthProgress--;
        healthbar.value = healthProgress;
    }
}
