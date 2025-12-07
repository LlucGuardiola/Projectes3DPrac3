using UnityEngine;

public class CoinItem : Item
{
    public AudioSource m_AudioSource;

    public override void Pick()
    {
        AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);

        GameManager.GetGameManager().m_Player.AddCoin();

        Destroy(gameObject);
    }

    public override bool CanPick()
    {
        return true;
    }
}
