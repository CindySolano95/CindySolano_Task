using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDatabase", menuName = "Inventory/Inventory Database", order = 1)]
public sealed class InventoryDatabase : ScriptableObject
{
    [Serializable]
    public sealed class ItemDefinition
    {
        public int id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemCategory category;
        public bool stackable;

        // Optional: method name to call via SendMessage (legacy-friendly).
        public string onUseMessage;
    }

    public enum ItemCategory
    {
        Consumable,
        Equipable,
        Crafting,
        QuestItem
    }

    [SerializeField] private List<ItemDefinition> items = new();

    private Dictionary<int, ItemDefinition> _byId;

    private void OnEnable() => BuildCache();

    private void BuildCache()
    {
        _byId = new Dictionary<int, ItemDefinition>(items.Count);
        foreach (var def in items)
        {
            if (def == null) continue;

            if (_byId.ContainsKey(def.id))
            {
                Debug.LogError($"[InventoryDatabase] Duplicate ID found: {def.id}", this);
                continue;
            }

            _byId.Add(def.id, def);
        }
    }

    public bool TryGet(int id, out ItemDefinition definition)
    {
        if (_byId == null) BuildCache();
        return _byId.TryGetValue(id, out definition);
    }

    public ItemDefinition GetRequired(int id)
    {
        if (!TryGet(id, out var def))
            throw new KeyNotFoundException($"Item ID '{id}' was not found in InventoryDatabase.");
        return def;
    }
}
