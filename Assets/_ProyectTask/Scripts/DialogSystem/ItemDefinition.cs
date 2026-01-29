using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory
{
    Consumable,
    Equipable,
    Crafting,
    QuestItem
}

[CreateAssetMenu(menuName = "Items/Item Definition", fileName = "Item_")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public int id;
    public string displayName;

    [Header("Visual")]
    public Sprite icon;

    [Header("Rules")]
    public ItemCategory category = ItemCategory.QuestItem;
    public bool stackable = true;

    [Header("Description")]
    [TextArea] public string description;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Si no llenas displayName, usa el nombre del asset para evitar nulls.
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;
    }
#endif
}
