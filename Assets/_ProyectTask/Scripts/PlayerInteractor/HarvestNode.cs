using UnityEngine;


/// <summary>
/// A reusable gather node (cow, tree, rock, etc.).
/// Gives one reward, then enters cooldown and becomes unavailable.
/// </summary>
public class HarvestNode : MonoBehaviour
{
    [Header("Reward (InventoryDatabase IDs)")]
    [SerializeField] private int itemId = 1002;     // Raw Meat (example)
    [SerializeField] private int amount = 1;

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 60f;

    [Header("Indicator / Highlight")]
    [Tooltip("Object to enable/disable (glow, icon, outline, etc.)")]
    [SerializeField] private GameObject indicator;

    [Header("Interaction Prompt")]
    [SerializeField] private string promptText = "Press E to collect";

    private bool _ready = true;
    private float _nextReadyTime = 0f;

    public string PromptText => promptText;

    private void Start()
    {
        RefreshIndicator();
    }

    private void Update()
    {
        if (!_ready && Time.time >= _nextReadyTime)
        {
            _ready = true;
            RefreshIndicator();
        }
    }

    /// <summary>
    /// Returns true if the node can currently be harvested.
    /// </summary>
    public bool IsReady() => _ready;

    /// <summary>
    /// Called by the player to collect.
    /// If successful, returns reward data and starts cooldown.
    /// </summary>
    public bool TryHarvest(out int outItemId, out int outAmount)
    {
        outItemId = 0;
        outAmount = 0;

        if (!_ready) return false;

        _ready = false;
        _nextReadyTime = Time.time + cooldownSeconds;
        RefreshIndicator();

        outItemId = itemId;
        outAmount = amount;
        return true;
    }

    private void RefreshIndicator()
    {
        if (indicator != null)
            indicator.SetActive(_ready);
    }
}
