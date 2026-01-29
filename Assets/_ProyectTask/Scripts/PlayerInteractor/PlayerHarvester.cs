using System.Collections;
using UnityEngine;

/// <summary>
/// Handles interaction with HarvestNodes:
/// - Detect node in range (via TriggerEvent2D or direct trigger callbacks)
/// - Press a Key to collect
/// - Show held item icon immediately
/// - After a short delay, add to inventory and show a message
/// </summary>
public class PlayerHarvester : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Refs")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private InventoryDatabase inventoryDatabase;
    [SerializeField] private PlayerController playerController; // to reuse held visual functions

    [Header("UI Prompt")]
    [SerializeField] private TalkPromptView promptView;

    [Header("Pickup Flow")]
    [Tooltip("Seconds before saving the harvested item into inventory (simulates 'carry then store').")]
    [SerializeField] private float storeDelaySeconds = 1.0f;

    [Header("Floating Message (optional)")]
    [SerializeField] private FloatingTextSpawner floatingText; // optional helper
    [SerializeField] private string afterStoreMessage = "Check your inventory!";

    private HarvestNode _currentNode;

    private void Awake()
    {
        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        // promptView can live in Canvas; assign in Inspector
    }

    private void Update()
    {
        if (_currentNode == null) return;

        // If node is not ready anymore, hide prompt.
        if (!_currentNode.IsReady())
        {
            promptView?.Hide();
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            TryCollectFromCurrentNode();
        }
    }

    public void SetCurrentNode(HarvestNode node)
    {
        _currentNode = node;

        Debug.Log($"[PlayerHarvester] Node in range: {node.name}. Ready={node.IsReady()} PromptViewNull={promptView == null}");

        if (_currentNode != null && _currentNode.IsReady())
            promptView?.Show(_currentNode.PromptText);
        else
            promptView?.Hide();
    }

    public void ClearCurrentNode(HarvestNode node)
    {
        // Only clear if the node leaving is the one we're tracking
        if (_currentNode == node)
            _currentNode = null;

        promptView?.Hide();
    }

    private void TryCollectFromCurrentNode()
    {
        if (_currentNode == null) return;
        if (!_currentNode.IsReady()) return;

        if (!_currentNode.TryHarvest(out int itemId, out int amount))
            return;

        // Show held icon immediately (your PlayerController already supports this)
        if (playerController != null)
            playerController.SetHeldItemVisualFromDatabase(itemId);

        // Hide prompt to avoid double presses while storing
        promptView?.Hide();

        Debug.Log($"[PlayerHarvester] StoreAfterDelay done. floatingTextNull={floatingText == null} message='{afterStoreMessage}'");

        // Store into inventory after delay
        StartCoroutine(StoreAfterDelay(itemId, amount));
    }

    private IEnumerator StoreAfterDelay(int itemId, int amount)
    {
        yield return new WaitForSeconds(storeDelaySeconds);

        if (inventoryController != null)
            inventoryController.AddItem(itemId, amount);

        // Clear held icon after storing (optional)
        if (playerController != null)
            playerController.ClearHeldItemVisual();

        // Floating feedback
        floatingText?.Show(afterStoreMessage);

        // When cooldown ends, node turns indicator back on; player must re-enter/approach or you can refresh via trigger.
    }
}
