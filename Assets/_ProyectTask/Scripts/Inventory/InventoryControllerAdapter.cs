using System.Collections.Generic;
using UnityEngine;

public class InventoryControllerAdapter : MonoBehaviour, IInventoryService
{
    [SerializeField] private InventoryController inventory;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryController>();
    }

    public int GetCount(int itemId)
    {
        if (inventory == null) return 0;
        return inventory.GetCount(itemId);
    }

    public bool HasAll(List<ItemRequirement> requirements)
    {
        if (inventory == null) return false;
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var r in requirements)
        {
            if (r.amount <= 0) continue;
            if (!inventory.HasItem(r.itemId, r.amount))
                return false;
        }

        return true;
    }

    public void Remove(List<ItemRequirement> requirements)
    {
        if (inventory == null) return;
        if (requirements == null || requirements.Count == 0) return;

        // Remove via the inventory's own API (this also refreshes UI internally).
        foreach (var r in requirements)
        {
            if (r.amount <= 0) continue;
            inventory.RemoveItem(r.itemId, r.amount);
        }
    }
}
