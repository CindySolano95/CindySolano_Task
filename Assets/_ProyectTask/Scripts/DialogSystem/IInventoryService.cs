using UnityEngine;
using System.Collections.Generic;

public interface IInventoryService
{
    // Returns how many of this item (by ID) the player has.
    int GetCount(int itemId);

    // Returns true if the player has all requirements.
    bool HasAll(List<ItemRequirement> requirements);

    // Removes required items from inventory (assumes HasAll = true).
    void Remove(List<ItemRequirement> requirements);
}
