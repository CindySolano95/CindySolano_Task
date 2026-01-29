using UnityEngine;

/// <summary>
/// e filter by checking if the Player is currently inside this trigger using OverlapCollider
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HarvestNodeTriggerBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HarvestNode node;
    [SerializeField] private PlayerHarvester playerHarvester; // assign in Inspector OR auto-find


    private Collider2D _trigger;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
        if (_trigger != null) _trigger.isTrigger = true;

        if (node == null) node = GetComponent<HarvestNode>();

        // Auto-find harvester if not assigned (works fine for a small test scene).
        if (playerHarvester == null)
            playerHarvester = FindFirstObjectByType<PlayerHarvester>();
    }

    /// <summary>
    /// Call this from TriggerEvent.OnEnter (UnityEvent).
    /// Since TriggerEvent provides no collider info, we detect the player via overlap.
    /// </summary>
    public void HandleEnter()
    {
        if (playerHarvester == null || node == null)
        {
            Debug.LogWarning("[HarvestNodeTriggerBridge] Missing refs. playerHarvester or node is null.");
            return;
        }
        playerHarvester.SetCurrentNode(node);
    }

    /// <summary>
    /// Call this from TriggerEvent.OnExit (UnityEvent).
    /// We re-check overlap: if player is no longer inside, clear.
    /// </summary>
    public void HandleExit()
    {
        if (playerHarvester == null || node == null) return;
        playerHarvester.ClearCurrentNode(node);
    }

    
}
