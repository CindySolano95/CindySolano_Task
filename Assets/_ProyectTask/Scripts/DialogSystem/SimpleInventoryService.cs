using System.Collections.Generic;
using UnityEngine;

public class SimpleInventoryService : MonoBehaviour, IInventoryService
{
    [System.Serializable]
    public class ItemStack
    {
        [Tooltip("Item ID from InventoryDatabase (e.g., 1001 for Apple).")]
        public int itemId;

        [Min(0)]
        public int amount;
    }

    [Header("Debug Inventory (IDs from InventoryDatabase)")]
    public List<ItemStack> debugItems = new();

    // itemId -> amount
    private Dictionary<int, int> _map;

    private void Awake()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        _map = new Dictionary<int, int>();

        foreach (var stack in debugItems)
        {
            if (stack.amount <= 0) continue;

            if (_map.TryGetValue(stack.itemId, out var current))
                _map[stack.itemId] = current + stack.amount;
            else
                _map.Add(stack.itemId, stack.amount);
        }
    }

    /// <summary>
    /// Returns the amount of an item the player currently has (by item ID).
    /// </summary>
    public int GetCount(int itemId)
    {
        return _map != null && _map.TryGetValue(itemId, out var v) ? v : 0;
    }

    /// <summary>
    /// True if the player has all required items and amounts.
    /// </summary>
    public bool HasAll(List<ItemRequirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var r in requirements)
        {
            if (r.amount <= 0) continue; // ignore invalid rows safely
            if (GetCount(r.itemId) < r.amount) return false;
        }

        return true;
    }

    /// <summary>
    /// Removes required items (clamps to zero). Assumes HasAll() was true.
    /// </summary>
    public void Remove(List<ItemRequirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return;

        foreach (var r in requirements)
        {
            if (r.amount <= 0) continue;

            var current = GetCount(r.itemId);
            _map[r.itemId] = Mathf.Max(0, current - r.amount);
        }
    }
}
