using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Text Elements")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI coinsText;

    private string Settings = "SettingsMenu";
    private string mainMenu = "MainMenu";

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject Hud;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameWinMenu;
    [SerializeField] private GameObject LevelPicker;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryScoreText;


    private bool isPaused = false;


    //[SerializeField] private InputHandle input;

    void Update()
    {
        bool PausePressed = Input.GetKeyDown(KeyCode.Escape);

        if (PausePressed)
        {
            PauseMenuControle();
        }
    }


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateLives(int lives)
    {
        livesText.text = "Lives: " + lives;
    }

    public void UpdateCoins(int coins)
    {
        coinsText.text = "Coins: " + coins;
    }

  //  private void OnEnable()
   // {
    //    pauseAction.action.performed += OnPausePerformed;
    //    pauseAction.action.Enable();


   // }

   // private void OnDisable()
   // {
     //   pauseAction.action.performed -= OnPausePerformed;
      //  pauseAction.action.Disable();


   // }

   // private void OnPausePerformed(InputAction.CallbackContext context)
   // {
   //     PauseMenuControle();
   // }


    public void FreezeGame()
    {
        Time.timeScale = 0f;
    }

    public void UnFreezeGame()
    {
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;

        Debug.Log("Restart");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenu);
    }


    public void SettingsLoad()
    {
        SceneManager.LoadScene(Settings, LoadSceneMode.Additive);
    }

    public void CloseSettings()
    {
        SceneManager.UnloadSceneAsync(Settings);
    }

    public void PauseMenuControle()
    {
        if (isPaused == false)
        {
            pauseMenu.SetActive(true);
            Hud.SetActive(false);
            isPaused = true;
            FreezeGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else if (isPaused == true)
        {
            pauseMenu.SetActive(false);
            Hud.SetActive(true);
            isPaused = false;
            UnFreezeGame();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void GameOverMenu()
    {

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOverMenu.SetActive(true);

    }

    public void GameWin()
    {

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameWinMenu.SetActive(true);

    }

    public void LevelSelect()
    {

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        LevelPicker.SetActive(true);

    }

    public void VictoryScreen(int score)
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOverMenu.SetActive(false);

        victoryPanel.SetActive(true);
        victoryScoreText.text = "Coins: " + score;
    }
}
