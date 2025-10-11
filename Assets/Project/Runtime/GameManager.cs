public class GameManager : MonoBehaviour
{

    #region Variables
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    #region Unity Event Functions
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        instance.LoadSounds();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    private void StartGame()
    {

    }
}