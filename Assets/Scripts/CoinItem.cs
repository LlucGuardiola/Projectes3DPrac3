using UnityEngine;

public class CoinItem : Item
{
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().m_Player.AddCoin();
    }
    public override bool CanPick()
    {
        return true;
    }
}
