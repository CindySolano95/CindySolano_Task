using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{

    // We store inventory as a fixed list of slots.
    // Each slot can be empty or contain (itemId, amount).
    // This guarantees "slot persistence": item position is stable across sessions.

    [Serializable]
    public struct SlotStack
    {
        public int id;
        public int amount;

        public bool IsEmpty => id <= 0 || amount <= 0;

        public SlotStack(int id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }

        public static SlotStack Empty => new SlotStack(0, 0);
    }

    // This is the JSON structure written to disk.
    // "slots" length should match the UI slot count, but we also handle mismatches
    [Serializable]
    private class InventorySaveData
    {
        public int version = 1;
        public List<SlotStack> slots = new List<SlotStack>();
    }

    [Header("Data")]
    [SerializeField] private InventoryDatabase database;

    // used ONLY on first run when there is no save file.
    // This allows you to start with some items for testing.
    [SerializeField] private List<SlotStack> seedItems = new();

    [Header("UI Refs")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private RectTransform dragCanvasRoot;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private InventoryItemView itemPrefab;
    [SerializeField] private InventoryTooltip tooltip;

    [Header("Save/Load")]
    [SerializeField] private string saveFileName = "inventory_save.json";

    private readonly List<RaycastResult> _raycastResults = new();
    private PointerEventData _pointerEventData;

    // Cached slot components in the UI (fixed order = persistence key)
    private readonly List<InventorySlot> _slots = new();

    // View pool: 1 view per slot, reused to avoid instantiate/destroy each refresh
    private readonly List<InventoryItemView> _viewsBySlot = new();

    // The actual runtime inventory model: 1 element per UI slot index.
    // _slotItems[i] represents what is stored in slot i.
    private List<SlotStack> _slotItems = new();

    // Drag state (UI interaction)
    private InventoryItemView _draggingItem;
    private InventorySlot _dragOriginSlot;
    private CanvasGroup _dragCanvasGroup;

    [Header("Delete UI")]
    [SerializeField] private CartelEliminacion deleteConfirmPanel; // tu panel con slider/botones

    // Delete workflow runtime
    private bool _deletePromptOpen;
    private InventoryItemView _pendingDeleteView;
    private InventorySlot _pendingDeleteOriginSlot;
    private int _pendingDeleteSlotIndex;
    private int _pendingDeleteMaxAmount;


    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);


    private void Awake()
    {
        // PointerEventData uses the active EventSystem
        _pointerEventData = new PointerEventData(EventSystem.current);

        // 1) Read and index UI slots (slot index is the persistence key)
        CacheSlotsAndAssignIndexes();

        // 2) Build the 1-view-per-slot pool
        EnsureViewsPool();

        // 3) Load from disk if exists; otherwise seed (first run)
        LoadOrCreate();

        // 4) Render loaded runtime model into the UI
        RefreshUI();
    }

    private void Update()
    {
        HandleDragAndDrop();
    }

    private void OnApplicationQuit()
    {
        SaveToDisk();
    }

    private void OnDisable()
    {
        SaveToDisk();
    }

    // ---------------------------
    // Public API (used by NPC + others)
    // ---------------------------

    /// <summary>
    /// Returns how many of a given item ID the player currently has.
    /// </summary>
    public int GetCount(int id)
    {
        int total = 0;
        for (int i = 0; i < _slotItems.Count; i++)
        {
            if (_slotItems[i].id == id && _slotItems[i].amount > 0)
                total += _slotItems[i].amount;
        }
        return total;
    }

    /// <summary>
    /// True if the player has at least 'amount' of the given item.
    /// </summary>
    public bool HasItem(int id, int amount)
    {
        return GetCount(id) >= amount;
    }

    public void AddItem(int id, int amount)
    {
        if (amount <= 0) return;

        if (!database.TryGet(id, out var def))
        {
            Debug.LogError($"[Inventory] Tried to add unknown item id: {id}", this);
            return;
        }

        if (_slotItems == null || _slotItems.Count != _slots.Count)
            ResizeModelToSlots();

        if (def.stackable)
        {
            // 1) Try find existing stack
            for (int i = 0; i < _slotItems.Count; i++)
            {
                if (_slotItems[i].id == id && _slotItems[i].amount > 0)
                {
                    _slotItems[i] = new SlotStack(id, _slotItems[i].amount + amount);
                    SaveToDisk();
                    RefreshUI();
                    return;
                }
            }

            // 2) Else place in first empty slot
            int empty = FindFirstEmptySlot();
            if (empty < 0)
            {
                Debug.LogWarning("[Inventory] No empty slots available.", this);
                return;
            }

            _slotItems[empty] = new SlotStack(id, amount);
            SaveToDisk();
            RefreshUI();
            return;
        }

        // Non-stackable: add "amount" times into empty slots
        int remaining = amount;
        while (remaining > 0)
        {
            int empty = FindFirstEmptySlot();
            if (empty < 0)
            {
                Debug.LogWarning("[Inventory] Not enough empty slots for non-stackable items.", this);
                break;
            }

            _slotItems[empty] = new SlotStack(id, 1);
            remaining--;
        }

        SaveToDisk();
        RefreshUI();
    }

    public void RemoveItem(int id, int amount)
    {
        if (amount <= 0) return;
        if (_slotItems == null || _slotItems.Count == 0) return;

        int remaining = amount;

        // Remove across slots (important for non-stackables)
        for (int i = 0; i < _slotItems.Count && remaining > 0; i++)
        {
            var s = _slotItems[i];
            if (s.id != id || s.amount <= 0) continue;

            if (!database.TryGet(id, out var def))
            {
                // If definition missing, still remove safely
                int take = Mathf.Min(remaining, s.amount);
                s.amount -= take;
                remaining -= take;
                _slotItems[i] = s.amount <= 0 ? SlotStack.Empty : s;
                continue;
            }

            if (def.stackable)
            {
                int take = Mathf.Min(remaining, s.amount);
                s.amount -= take;
                remaining -= take;
                _slotItems[i] = s.amount <= 0 ? SlotStack.Empty : s;
            }
            else
            {
                // Each slot is one instance
                _slotItems[i] = SlotStack.Empty;
                remaining--;
            }
        }

        SaveToDisk();
        RefreshUI();
    }


    // ---------------------------
    // Slots / UI
    // ---------------------------

    /// <summary>
    /// Reads all InventorySlot components under the contentRoot and assigns an index.
    /// The index is the persistence key: slot 0 in UI = slot 0 in save file.
    /// </summary>
    private void CacheSlotsAndAssignIndexes()
    {
        _slots.Clear();

        for (int i = 0; i < contentRoot.childCount; i++)
        {
            var slot = contentRoot.GetChild(i).GetComponent<InventorySlot>();
            if (slot == null) continue;

            slot.SetIndex(_slots.Count);
            _slots.Add(slot);
        }
    }

    private void EnsureViewsPool()
    {
        _viewsBySlot.Clear();

        for (int i = 0; i < _slots.Count; i++)
        {
            var view = Instantiate(itemPrefab);
            view.Initialize(tooltip);
            view.gameObject.SetActive(false);
            _viewsBySlot.Add(view);
        }
    }

    private void RefreshUI()
    {
        if (_slotItems == null || _slotItems.Count != _slots.Count)
            ResizeModelToSlots();

        // Clear all slots
        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsEmpty)
            {
                var old = _slots[i].Detach();
                if (old != null) old.gameObject.SetActive(false);
            }
        }

        // Place per-slot
        for (int i = 0; i < _slots.Count; i++)
        {
            var data = _slotItems[i];
            if (data.IsEmpty) continue;

            if (!database.TryGet(data.id, out var def))
            {
                Debug.LogError($"[Inventory] Missing definition for id: {data.id}", this);
                continue;
            }

            var view = _viewsBySlot[i];
            view.Bind(def, data.amount);
            view.gameObject.SetActive(true);

            _slots[i].Attach(view);

            // Optional legacy click action via SendMessage
            var button = view.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (!string.IsNullOrWhiteSpace(def.onUseMessage))
                    button.onClick.AddListener(() => gameObject.SendMessage(def.onUseMessage, SendMessageOptions.DontRequireReceiver));
            }
        }

        tooltip?.Hide();
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < _slotItems.Count; i++)
        {
            if (_slotItems[i].IsEmpty) return i;
        }
        return -1;
    }

    private void ResizeModelToSlots()
    {
        if (_slotItems == null) _slotItems = new List<SlotStack>();

        // Grow
        while (_slotItems.Count < _slots.Count)
            _slotItems.Add(SlotStack.Empty);

        // Shrink (rare)
        if (_slotItems.Count > _slots.Count)
            _slotItems.RemoveRange(_slots.Count, _slotItems.Count - _slots.Count);
    }


    // ---------------------------
    // Drag & Drop
    // ---------------------------

    private void HandleDragAndDrop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tooltip?.Hide();

            var hitItem = RaycastForComponentInParents<InventoryItemView>();
            if (hitItem == null) return;

            var originSlot = hitItem.GetComponentInParent<InventorySlot>();
            if (originSlot == null) return;

            _draggingItem = hitItem;
            _dragOriginSlot = originSlot;

            // While dragging, the item should not block raycasts,
            // otherwise it can prevent detecting the slot under the cursor.
            _dragCanvasGroup = _draggingItem.GetComponent<CanvasGroup>();
            if (_dragCanvasGroup == null)
                _dragCanvasGroup = _draggingItem.gameObject.AddComponent<CanvasGroup>();

            _dragCanvasGroup.blocksRaycasts = false;

            originSlot.Detach();
            _draggingItem.transform.SetParent(dragCanvasRoot, worldPositionStays: false);
            _draggingItem.transform.SetAsLastSibling();
        }

        if (_draggingItem == null) return;

        _draggingItem.GetComponent<RectTransform>().position = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {

            if (IsPointerOverDeleteZone())
            {
                BeginDeleteFlow();
                return;
            }

            var targetSlot = RaycastForComponent<InventorySlot>();

            // No valid target -> return to origin (no model change)
            if (targetSlot == null)
            {
                _dragOriginSlot.Attach(_draggingItem);
                EndDrag();
                return;
            }

            int a = _dragOriginSlot.SlotIndex;
            int b = targetSlot.SlotIndex;

            if (a < 0 || b < 0)
            {
                _dragOriginSlot.Attach(_draggingItem);
                EndDrag();
                return;
            }

            // Drop on empty slot: move model a -> b
            if (targetSlot.IsEmpty)
            {
                MoveSlot(a, b);
                SaveToDisk();

                // IMPORTANT: stop dragging BEFORE RefreshUI rebuilds views.
                // Otherwise the dragged object is still parented to dragCanvasRoot
                // while RefreshUI detaches/attaches pooled views, causing visual glitches.
                _draggingItem.gameObject.SetActive(false);
                EndDrag();

                RefreshUI();
                return;
            }

            // Target occupied:
            var targetItem = targetSlot.CurrentItem;

            // Merge if same id + stackable
            if (targetItem != null &&
                targetItem.ItemId == _draggingItem.ItemId &&
                _draggingItem.IsStackable)
            {
                var sb = _slotItems[b];
                sb.amount += _draggingItem.Amount;
                _slotItems[b] = sb;

                _slotItems[a] = SlotStack.Empty;

                SaveToDisk();

                _draggingItem.gameObject.SetActive(false);
                EndDrag();

                RefreshUI();
                return;
            }

            // Otherwise swap model a <-> b
            SwapSlots(a, b);
            SaveToDisk();

            _draggingItem.gameObject.SetActive(false);
            EndDrag();

            RefreshUI();
        }
    }

    private void MoveSlot(int from, int to)
    {
        if (from == to) return;
        var temp = _slotItems[from];
        _slotItems[from] = SlotStack.Empty;
        _slotItems[to] = temp;
    }

    private void SwapSlots(int a, int b)
    {
        if (a == b) return;
        var tmp = _slotItems[a];
        _slotItems[a] = _slotItems[b];
        _slotItems[b] = tmp;
    }

    private void EndDrag()
    {
        if (_dragCanvasGroup != null)
            _dragCanvasGroup.blocksRaycasts = true;

        _draggingItem = null;
        _dragOriginSlot = null;
        _dragCanvasGroup = null;
    }

    private bool IsPointerOverDeleteZone()
    {
        _raycastResults.Clear();
        _pointerEventData.position = Input.mousePosition;
        graphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        foreach (var r in _raycastResults)
        {
            // revisa el objeto y sus padres por si el tag está en el root del icono
            if (r.gameObject != null && (r.gameObject.CompareTag("Delete") || r.gameObject.GetComponentInParent<Transform>().CompareTag("Delete")))
                return true;

            var t = r.gameObject != null ? r.gameObject.transform : null;
            while (t != null)
            {
                if (t.CompareTag("Delete")) return true;
                t = t.parent;
            }
        }
        return false;
    }

    private void BeginDeleteFlow()
    {
        if (_draggingItem == null || _dragOriginSlot == null) return;

        int slotIndex = _dragOriginSlot.SlotIndex;
        if (slotIndex < 0 || slotIndex >= _slotItems.Count)
        {
            // fallback: vuelve al slot
            _dragOriginSlot.Attach(_draggingItem);
            EndDrag();
            return;
        }

        var stack = _slotItems[slotIndex];
        if (stack.IsEmpty)
        {
            // nada que borrar realmente
            _dragOriginSlot.Attach(_draggingItem);
            EndDrag();
            return;
        }

        // Guardamos contexto para confirm/cancel
        _deletePromptOpen = true;
        _pendingDeleteView = _draggingItem;
        _pendingDeleteOriginSlot = _dragOriginSlot;
        _pendingDeleteSlotIndex = slotIndex;
        _pendingDeleteMaxAmount = Mathf.Max(1, stack.amount);

        // Congelamos el ítem visualmente (opcional: lo ocultas mientras decide)
        _pendingDeleteView.gameObject.SetActive(false);

        // Cortamos drag state para evitar glitches
        EndDrag();

        // Si solo hay 1, borrado inmediato
        if (_pendingDeleteMaxAmount <= 1)
        {
            ConfirmDeleteAmount(1);
            return;
        }

        // Si > 1: abrimos panel con slider
        if (deleteConfirmPanel != null)
        {
            deleteConfirmPanel.Show(
                maxAmount: _pendingDeleteMaxAmount,
                onConfirm: ConfirmDeleteAmount,
                onCancel: CancelDeleteFlow
            );
        }
        else
        {
            // Sin panel asignado: por seguridad, no borramos y devolvemos
            CancelDeleteFlow();
        }
    }

    private void CancelDeleteFlow()
    {
        if (!_deletePromptOpen) return;

        // Devuelve el view al slot original (modelo no se tocó)
        if (_pendingDeleteOriginSlot != null && _pendingDeleteView != null)
        {
            _pendingDeleteView.gameObject.SetActive(true);
            _pendingDeleteOriginSlot.Attach(_pendingDeleteView);
        }

        ClearDeleteContext();
    }

    private void ConfirmDeleteAmount(int amountToDelete)
    {
        if (!_deletePromptOpen) return;

        amountToDelete = Mathf.Clamp(amountToDelete, 1, _pendingDeleteMaxAmount);

        RemoveFromSlot(_pendingDeleteSlotIndex, amountToDelete);

        SaveToDisk();
        RefreshUI(); // reconstruye el view pool correctamente

        ClearDeleteContext();
    }

    private void RemoveFromSlot(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex >= _slotItems.Count) return;
        if (amount <= 0) return;

        var s = _slotItems[slotIndex];
        if (s.IsEmpty) return;

        s.amount -= amount;
        if (s.amount <= 0) _slotItems[slotIndex] = SlotStack.Empty;
        else _slotItems[slotIndex] = s;
    }

    private void ClearDeleteContext()
    {
        _deletePromptOpen = false;
        _pendingDeleteView = null;
        _pendingDeleteOriginSlot = null;
        _pendingDeleteSlotIndex = -1;
        _pendingDeleteMaxAmount = 0;
    }


    // ---------------------------
    // Save / Load
    // ---------------------------

    /// <summary>
    /// Loads inventory automatically on startup.
    /// If no save exists, seeds from inspector for first-run and saves immediately.
    /// </summary>
    private void LoadOrCreate()
    {
        ResizeModelToSlots();

        if (File.Exists(SavePath))
        {
            LoadFromDisk();
            // aseguramos tamaño correcto por si cambiaron #slots
            ResizeModelToSlots();
            return;
        }

        // If no save exists, seed from inspector (first run)
        SeedFromInspector();
        SaveToDisk();
    }

    // This does NOT run if a save file already exists.
    private void SeedFromInspector()
    {
        // Seed sequentially into slots, preserving order, but only first time.
        for (int i = 0; i < _slotItems.Count; i++)
            _slotItems[i] = SlotStack.Empty;

        int slot = 0;
        for (int i = 0; i < seedItems.Count && slot < _slotItems.Count; i++)
        {
            var s = seedItems[i];
            if (s.IsEmpty) continue;

            _slotItems[slot] = s;
            slot++;
        }
    }

    // Writes the slot-based inventory snapshot to disk as JSON.
    public void SaveToDisk()
    {
        try
        {
            var data = new InventorySaveData
            {
                version = 1,
                slots = new List<SlotStack>(_slotItems)
            };

            var json = JsonUtility.ToJson(data, prettyPrint: true);

            var dir = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Inventory] Save failed: {e.Message}", this);
        }
    }

    public void LoadFromDisk()
    {
        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<InventorySaveData>(json);

            if (data == null || data.slots == null)
            {
                Debug.LogWarning("[Inventory] Save file invalid. Recreating.", this);
                SeedFromInspector();
                return;
            }

            _slotItems = data.slots;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Inventory] Load failed: {e.Message}. Recreating.", this);
            SeedFromInspector();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        SeedFromInspector();
        SaveToDisk();
        RefreshUI();
    }

    // ---------------------------
    // Raycast Helpers
    // ---------------------------

    private T RaycastForComponent<T>() where T : Component
    {
        _raycastResults.Clear();
        _pointerEventData.position = Input.mousePosition;
        graphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        foreach (var r in _raycastResults)
        {
            var c = r.gameObject.GetComponent<T>();
            if (c != null) return c;
        }
        return null;
    }

    private T RaycastForComponentInParents<T>() where T : Component
    {
        _raycastResults.Clear();
        _pointerEventData.position = Input.mousePosition;
        graphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        foreach (var r in _raycastResults)
        {
            var c = r.gameObject.GetComponentInParent<T>();
            if (c != null) return c;
        }
        return null;
    }

}
