using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public TextMeshProUGUI sprocketText;
    public Slider healthbar;
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    //public TextMeshProUGUI healthText;
    float healthProgress = 10;
    int sprockets = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //sprocketText.text = sprockets.ToString() + " Sprockets";
        //healthText.text = healthProgress.ToString();
        Vector2 hotspot = new Vector2(0, 0);
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);

    }

    void Update() { }

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
