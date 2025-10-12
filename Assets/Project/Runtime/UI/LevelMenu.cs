using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void PlayLevel1()
    {
        //TODO: change to Level 1 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1 );
    }

    public void PlayLevel2()
    {   
        //TODO: change to Level 2
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void PlayLevel3()
    {
        //TODO: change to level 3
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
