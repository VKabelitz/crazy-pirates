using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void PlayLevel1()
    {
        //TODO: change to Level 1 
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }

    public void PlayLevel2()
    {   
        //TODO: change to Level 2
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 2);
        Time.timeScale = 1f;
    }

    public void PlayLevel3()
    {
        //TODO: change to level 3
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 3);
        Time.timeScale = 1f;
    }

    public void PlayLevel4()
    {
        //TODO: change to level 3
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 4);
        Time.timeScale = 1f;
    }
}
