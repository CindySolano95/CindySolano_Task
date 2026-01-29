using UnityEngine;

/// <summary>
/// Bridges TriggerEvent2D events to PlayerHarvester with this node reference.
/// </summary>
public class HarvestNodeTriggerBridge : MonoBehaviour
{
    [SerializeField] private HarvestNode node;

    private void Awake()
    {
        if (node == null) node = GetComponent<HarvestNode>();
    }

    public void OnPlayerEnter(GameObject player)
    {
        var harvester = player.GetComponent<PlayerHarvester>();
        if (harvester != null) harvester.SetCurrentNode(node);
    }

    public void OnPlayerExit(GameObject player)
    {
        var harvester = player.GetComponent<PlayerHarvester>();
        if (harvester != null) harvester.ClearCurrentNode(node);
    }
}
