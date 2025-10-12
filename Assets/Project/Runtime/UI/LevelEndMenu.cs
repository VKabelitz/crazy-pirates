using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelEndMenu : MonoBehaviour
{

    public static LevelEndMenu instance;
    public GameObject levelEndMenuUI;
    public TextMeshProUGUI sprocketAmountText;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        levelEndMenuUI.SetActive(false);
    }

    public void activateMenu()
    {
        levelEndMenuUI.SetActive(true);
        Time.timeScale = 0f;
        sprocketAmountText.text = "Sprockets:" + SprocketManager.instance.currentSprocketAmount.ToString();
    }

    public void loadMenu()
    {
        SceneManager.LoadSceneAsync("Menü");
    }
}
