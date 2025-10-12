using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelEndMenuLose : MonoBehaviour
{
    public static LevelEndMenuLose instance;
    public GameObject levelEndMenuLoseUI;
    public TextMeshProUGUI sprocketAmountText;
    
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        levelEndMenuLoseUI.SetActive(false);
    }

    public void activateMenu()
    {
        levelEndMenuLoseUI.SetActive(true);
        Time.timeScale = 0f;
        sprocketAmountText.text = "Sprockets:" + SprocketManager.instance.currentSprocketAmount.ToString();
    }

    public void tryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
