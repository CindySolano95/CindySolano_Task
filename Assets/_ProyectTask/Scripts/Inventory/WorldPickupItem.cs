using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldPickupItem : MonoBehaviour
{
    [SerializeField] private int itemId = 1;
    [SerializeField] private int amount = 1;
    [SerializeField] private bool destroyOnPickup = true;

    private bool consumed;

    public int ItemId => itemId;
    public int Amount => amount;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Consume()
    {
        if (consumed) return;
        consumed = true;

        // Prevent double trigger
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
