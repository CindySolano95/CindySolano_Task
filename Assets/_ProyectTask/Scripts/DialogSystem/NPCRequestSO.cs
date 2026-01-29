using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Request", fileName = "NPCRequest_")]
public class NPCRequestSO : ScriptableObject
{

    [Header("Identity")]
    public string requestId;

    [Header("Dialog Lines")]
    [TextArea] public string startLine = "Hello. I need something.";
    [TextArea] public string inProgressLine = "How is my request going?";
    [TextArea] public string missingItemsLine = "You're missing items.";
    [TextArea] public string completeLine = "Great. Delivery confirmed.";

    [Header("Requirements (InventoryDatabase IDs)")]
    public List<ItemRequirement> requirements = new();


    [Header("Progression")]
    public NPCRequestSO nextRequest;
}

[Serializable]
public class ItemRequirement
{
    [Tooltip("Item ID from InventoryDatabase.")]
    public int itemId;

    [Min(1)]
    public int amount = 1;
}
