using System.Collections.Generic;
using UnityEngine;

public class DebugInventoryService : MonoBehaviour, IInventoryService
{
    [System.Serializable]
    public class DebugStack
    {
        public int itemId;
        public int amount;
    }

    [Header("Debug Inventory (IDs from InventoryDatabase)")]
    public List<DebugStack> debugStacks = new();

    private Dictionary<int, int> _map;

    private void Awake()
    {
        _map = new Dictionary<int, int>();
        foreach (var s in debugStacks)
        {
            if (s.amount <= 0) continue;
            _map[s.itemId] = _map.TryGetValue(s.itemId, out var v) ? v + s.amount : s.amount;
        }
    }

    public int GetCount(int itemId) => _map.TryGetValue(itemId, out var v) ? v : 0;

    public bool HasAll(List<ItemRequirement> requirements)
    {
        foreach (var r in requirements)
        {
            if (GetCount(r.itemId) < r.amount) return false;
        }
        return true;
    }

    public void Remove(List<ItemRequirement> requirements)
    {
        foreach (var r in requirements)
        {
            var current = GetCount(r.itemId);
            _map[r.itemId] = Mathf.Max(0, current - r.amount);
        }
    }
}
