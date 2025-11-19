using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager m_GameManager;
    List<IRestartElement> m_RestartElements = new List<IRestartElement>();

    private void Awake()
    {
        if (m_GameManager != null)
        {
            Destroy(gameObject);
            return;
        }

        m_GameManager = this;
        DontDestroyOnLoad(gameObject);
    }
    public static GameManager GetGameManager()
    {
        return m_GameManager;
    }
    public void AddRestartGameElements(IRestartElement RestartElement)
    {
        m_RestartElements.Add(RestartElement);
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            RestartGame();
        }
    }
    public void RestartGame()
    {
        foreach (IRestartElement l_RestartGameElement in m_RestartElements)
        {
            l_RestartGameElement.RestartGame();
        }
    }
}
