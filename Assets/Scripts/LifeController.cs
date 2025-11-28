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
    }
    public int GetValue()
    {
        return m_Life;
    }
}
