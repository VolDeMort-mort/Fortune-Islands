using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;     // The "Lobby" (Start Button, Title)
    public GameObject gameHUDPanel;  // The "In-Game" UI (Resources, Build Menu)

    [Header("Buttons")]
    public Button singlePlayerBtn;
    public Button multiPlayerBtn;    // Placeholder for later

    void Start()
    {
        // 1. Setup UI State
        menuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);

        // 2. Bind Buttons
        singlePlayerBtn.onClick.AddListener(OnSinglePlayerClicked);
        // multiPlayerBtn.onClick.AddListener(OnMultiPlayerClicked);
    }

    private void OnSinglePlayerClicked()
    {
        // 1. Hide Menu, Show HUD
        menuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        // 2. Tell the Game Manager to wake up
        GameManager.Instance.StartSinglePlayerGame();
    }
}