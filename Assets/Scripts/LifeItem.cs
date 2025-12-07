using UnityEngine;

public class LifeItem : Item
{
    public AudioSource m_AudioSource;

    public override void Pick()
    {
        AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);
        base.Pick();
        GameManager.GetGameManager().m_Player.Heal();
    }
    public override bool CanPick()
    {
        return !(GameManager.GetGameManager().m_Player.GetComponent<PlayerController>().m_LifeController.m_Life >= 8);
    }
}
