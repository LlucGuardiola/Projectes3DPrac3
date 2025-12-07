using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject m_Panel;
    public Button m_RetryButton;

    private void Start()
    {
        m_Panel.SetActive(false);
        m_RetryButton.onClick.AddListener(OnRetry);
    }

    public void Show()
    {
        m_Panel.SetActive(true);
        Time.timeScale = 0f;    
    }

    public void Hide()
    {
        m_Panel.SetActive(false);
        Time.timeScale = 1f;  
    }

    void OnRetry()
    {
        Hide();
        GameManager.GetGameManager().RestartGame();
    }
}
