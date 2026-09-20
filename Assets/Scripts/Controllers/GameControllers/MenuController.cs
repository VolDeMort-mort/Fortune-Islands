using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;    
    public GameObject gameHUDPanel; 

    [Header("Buttons")]
    public Button singlePlayerBtn;
    public Button multiPlayerBtn;    

    void Start()
    {
        menuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);

        singlePlayerBtn.onClick.AddListener(OnSinglePlayerClicked);
    }

    private void OnSinglePlayerClicked()
    {
        menuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        GameManager.Instance.StartSinglePlayerGame();
    }
}