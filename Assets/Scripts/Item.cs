using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public virtual void Pick()
    {
        Destroy(gameObject);
    }

    public abstract bool CanPick();
}
