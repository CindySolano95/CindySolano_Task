using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NPCRequestManager : MonoBehaviour
{
    public static NPCRequestManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private DialogSystemView dialogView;

    // Inventory provider must implement IInventoryService
    [SerializeField] private MonoBehaviour inventoryServiceBehaviour;

    [Header("Item Database (your InventoryDatabase asset)")]
    [SerializeField] private InventoryDatabase inventoryDatabase;

    private IInventoryService _inventory;

    // Runtime progress per NPC
    private readonly Dictionary<string, NPCProgress> _progressByNpcId = new();

    private void Awake()
    {
        // Simple singleton for the test scene
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _inventory = inventoryServiceBehaviour as IInventoryService;
        if (_inventory == null)
            Debug.LogError("[NPCRequestManager] Inventory service does not implement IInventoryService.");

        if (inventoryDatabase == null)
            Debug.LogError("[NPCRequestManager] InventoryDatabase is not assigned.");
    }

    /// <summary>
    /// Called by NPCInteractor when the player talks to an NPC.
    /// </summary>
    public void TalkToNPC(string npcId, NPCRequestChainSO chain, Sprite npcPortrait)
    {
        if (chain == null || chain.firstRequest == null)
        {
            ShowOneShot(npcPortrait, "…(No requests configured for this NPC.)");
            return;
        }

        var progress = GetOrCreateProgress(npcId);

        if (progress.completedAll)
        {
            ShowDialog(npcPortrait,
                "Thanks again. No more requests for now.",
                onOk: () => dialogView.Hide(),
                onLater: () => dialogView.Hide());
            return;
        }

        // First time: assign the first request
        if (progress.currentRequest == null)
        {
            progress.currentRequest = chain.firstRequest;

            ShowDialog(npcPortrait,
                progress.currentRequest.startLine + "\n\n" + BuildRequirementsText(progress.currentRequest),
                onOk: () => dialogView.Hide(),
                onLater: () => dialogView.Hide());
            return;
        }

        // Already has an active request
        var req = progress.currentRequest;

        if (_inventory != null && _inventory.HasAll(req.requirements))
        {
            // Ready to deliver: OK will deliver
            ShowDialog(npcPortrait,
                req.inProgressLine + "\n\n" + "You have everything. Press OK to deliver.",
                onOk: () => DeliverAndAdvance(progress, npcPortrait),
                onLater: () => dialogView.Hide());
        }
        else
        {
            // Missing items: OK just closes
            ShowDialog(npcPortrait,
                req.missingItemsLine + "\n\n" + BuildMissingText(req),
                onOk: () => dialogView.Hide(),
                onLater: () => dialogView.Hide());
        }
    }

    private void DeliverAndAdvance(NPCProgress progress, Sprite npcPortrait)
    {
        var req = progress.currentRequest;
        if (req == null)
        {
            ShowOneShot(npcPortrait, "No active request.");
            return;
        }

        // Safety re-check
        if (_inventory == null || !_inventory.HasAll(req.requirements))
        {
            ShowOneShot(npcPortrait, req.missingItemsLine + "\n\n" + BuildMissingText(req));
            return;
        }

        // Remove items from inventory
        _inventory.Remove(req.requirements);

        // Show completion line
        var completeLine = req.completeLine;

        // Advance to next request
        progress.currentRequest = req.nextRequest;
        if (progress.currentRequest == null)
            progress.completedAll = true;

        ShowDialog(npcPortrait,
            completeLine,
            onOk: () => dialogView.Hide(),
            onLater: () => dialogView.Hide());
    }

    private NPCProgress GetOrCreateProgress(string npcId)
    {
        if (!_progressByNpcId.TryGetValue(npcId, out var p))
        {
            p = new NPCProgress { npcId = npcId, currentRequest = null, completedAll = false };
            _progressByNpcId.Add(npcId, p);
        }
        return p;
    }

    // ----------------- Text builders (uses InventoryDatabase for names) -----------------

    private string BuildRequirementsText(NPCRequestSO req)
    {
        if (req.requirements == null || req.requirements.Count == 0)
            return "Required:\n- (Nothing)";

        var sb = new StringBuilder();
        sb.AppendLine("Required:");

        foreach (var r in req.requirements)
        {
            var name = GetItemNameSafe(r.itemId);
            sb.AppendLine($"- {name} x{r.amount}");
        }

        return sb.ToString();
    }

    private string BuildMissingText(NPCRequestSO req)
    {
        if (_inventory == null || req.requirements == null || req.requirements.Count == 0)
            return "";

        var sb = new StringBuilder();
        sb.AppendLine("Missing:");

        foreach (var r in req.requirements)
        {
            int have = _inventory.GetCount(r.itemId);
            int missing = Mathf.Max(0, r.amount - have);
            if (missing > 0)
            {
                var name = GetItemNameSafe(r.itemId);
                sb.AppendLine($"- {name}: {missing}");
            }
        }

        return sb.ToString();
    }

    private string GetItemNameSafe(int itemId)
    {
        if (inventoryDatabase != null && inventoryDatabase.TryGet(itemId, out var def) && def != null)
            return def.displayName;

        return $"Item#{itemId}";
    }

    private void ShowOneShot(Sprite npcPortrait, string line)
    {
        ShowDialog(npcPortrait, line, () => dialogView.Hide(), () => dialogView.Hide());
    }

    private void ShowDialog(Sprite npcPortrait, string line, System.Action onOk, System.Action onLater)
    {
        dialogView.Show(npcPortrait, line, onOk, onLater);
    }

    private class NPCProgress
    {
        public string npcId;
        public NPCRequestSO currentRequest;
        public bool completedAll;
    }
}
