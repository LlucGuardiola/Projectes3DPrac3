using UnityEngine;

public class LifeController
{
    int m_Life = 8;
    public delegate void OnLifeChangedFn(LifeController _LifeController);
    public event OnLifeChangedFn m_OnLifeChanged;

    public LifeController()
    {
        DependencyInjector.AddDependency<LifeController>(this);
    }

    public void AddLife(int life)
    {
        m_Life += life;
        m_OnLifeChanged.Invoke(this);
        if (m_Life <= 0)
        {
            Die();
        }
    }
    public int GetValue()
    {
        return m_Life;
    }
    void Die()
    {
        GameManager.GetGameManager().m_Player.Die();
    }

    public void ResetLife()
    {
        m_Life = 8;
        m_OnLifeChanged?.Invoke(this);
    }

}
