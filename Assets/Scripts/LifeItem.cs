using UnityEngine;

public class LifeItem : Item
{
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().m_Player.Heal();
    }
    public override bool CanPick()
    {
        return !(GameManager.GetGameManager().m_Player.GetComponent<PlayerController>().m_LifeController.m_Life >= 8);
    }
}
