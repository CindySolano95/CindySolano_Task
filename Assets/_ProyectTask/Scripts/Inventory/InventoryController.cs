using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Serializable]
    public struct ItemStack
    {
        public int id;
        public int amount;

        public ItemStack(int id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }
    }

    [Header("Data")]
    [SerializeField] private InventoryDatabase database;
    [SerializeField] private List<ItemStack> items = new();

    [Header("UI Refs")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private RectTransform dragCanvasRoot;         // e.g. InventoryPanel root
    [SerializeField] private RectTransform contentRoot;            // parent of slots (grid)
    [SerializeField] private InventoryItemView itemPrefab;         // prefab with ItemImg + Number, etc
    [SerializeField] private RectTransform tooltipRoot;            // DescriptionPanel root
    [SerializeField] private TextMeshProUGUI tooltipTitle;         // child 0
    [SerializeField] private TextMeshProUGUI tooltipBody;          // child 1

    [Header("Optional")]
    [SerializeField] private GameObject deletePanel;         
    [SerializeField] private InventoryTooltip tooltip;

    private readonly List<RaycastResult> _raycastResults = new();
    private PointerEventData _pointerEventData;

    private readonly List<InventorySlot> _slots = new();
    private readonly List<InventoryItemView> _pool = new();

    private InventoryItemView _draggingItem;
    private InventorySlot _dragOriginSlot;

    private void Awake()
    {
        _pointerEventData = new PointerEventData(EventSystem.current);

        CacheSlots();

        if (deletePanel != null)
            deletePanel.SetActive(false);
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        HandleDragAndDrop();
    }

    // ---------------------------
    // Public API (Model)
    // ---------------------------

    /// <summary>
    /// Returns how many of a given item ID the player currently has.
    /// </summary>
    public int GetCount(int id)
    {
        int total = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == id)
                total += items[i].amount;
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

        if (def.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].id == id)
                {
                    items[i] = new ItemStack(id, items[i].amount + amount);
                    RefreshUI();
                    return;
                }
            }

            items.Add(new ItemStack(id, amount));
        }
        else
        {
            // Non-stackable: add as many entries as amount
            for (int i = 0; i < amount; i++)
                items.Add(new ItemStack(id, 1));
        }

        RefreshUI();
    }

    public void RemoveItem(int id, int amount)
    {
        if (amount <= 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id != id) continue;

            var newAmount = items[i].amount - amount;
            if (newAmount <= 0)
                items.RemoveAt(i);
            else
                items[i] = new ItemStack(id, newAmount);

            RefreshUI();
            return;
        }
    }

    // ---------------------------
    // UI Build / Refresh
    // ---------------------------

    private void CacheSlots()
    {
        _slots.Clear();
        for (int i = 0; i < contentRoot.childCount; i++)
        {
            var slot = contentRoot.GetChild(i).GetComponent<InventorySlot>();
            if (slot != null) _slots.Add(slot);
        }
    }

    private void RefreshUI()
    {
        // Ensure pool size
        while (_pool.Count < items.Count)
        {
            var view = Instantiate(itemPrefab);
            SetupTooltipRefs(view);
            view.Initialize(tooltip);
            view.gameObject.SetActive(false);
            _pool.Add(view);
        }

        // Clear all slots
        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty)
            {
                var oldItem = slot.Detach();
                if (oldItem != null) oldItem.gameObject.SetActive(false);
            }
        }

        // Place items in slots
        for (int i = 0; i < items.Count; i++)
        {
            if (i >= _slots.Count)
            {
                Debug.LogWarning("[Inventory] Not enough slots for current items.", this);
                break;
            }

            var stack = items[i];

            if (!database.TryGet(stack.id, out var def))
            {
                Debug.LogError($"[Inventory] Missing definition for id: {stack.id}", this);
                continue;
            }

            var view = _pool[i];
            view.Bind(def, stack.amount);
            view.gameObject.SetActive(true);

            _slots[i].Attach(view);

            // Optional legacy: click action via SendMessage
            // (Better long-term: replace with ICommand/IItemAction interfaces)
            var button = view.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (!string.IsNullOrWhiteSpace(def.onUseMessage))
                    button.onClick.AddListener(() => gameObject.SendMessage(def.onUseMessage, SendMessageOptions.DontRequireReceiver));
            }
        }

        // Hide remaining pooled views not used
        for (int i = items.Count; i < _pool.Count; i++)
        {
            if (_pool[i] != null) _pool[i].gameObject.SetActive(false);
        }

        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    private void SetupTooltipRefs(InventoryItemView view)
    {
        // Keeps the view reusable without static globals.
        // Only needed once at instantiation time.
        var so = view.GetType(); // no-op, avoids “unused” warnings if you later expand.

        // Assign via serialized fields on the prefab if you prefer.
        // If your prefab doesn't have them wired, you can wire them here by exposing setters.
        // For simplicity, ensure the prefab has tooltip refs already assigned,
        // OR make tooltip fields public setters.
    }

    // ---------------------------
    // Drag & Drop
    // ---------------------------

    private void HandleDragAndDrop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tooltip.Hide();

            var hitItem = RaycastForComponentInParents<InventoryItemView>();
            if (hitItem == null) return;

            var originSlot = hitItem.GetComponentInParent<InventorySlot>();
            if (originSlot == null) return;

            _draggingItem = hitItem;
            _dragOriginSlot = originSlot;

            originSlot.Detach();
            _draggingItem.transform.SetParent(dragCanvasRoot, worldPositionStays: false);
        }

        if (_draggingItem != null)
        {
            _draggingItem.GetComponent<RectTransform>().position = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                var targetSlot = RaycastForComponent<InventorySlot>();

                if (targetSlot == null)
                {
                    // No valid target: return to origin
                    _dragOriginSlot.Attach(_draggingItem);
                }
                else if (targetSlot.IsEmpty)
                {
                    // Free slot: attach
                    targetSlot.Attach(_draggingItem);
                }
                else
                {
                    // Occupied: stack if same id and stackable, else swap
                    var other = targetSlot.CurrentItem;

                    if (other.ItemId == _draggingItem.ItemId && _draggingItem.IsStackable)
                    {
                        other.SetAmount(other.Amount + _draggingItem.Amount);
                        _dragOriginSlot.Attach(null);
                        Destroy(_draggingItem.gameObject);

                        // Also update model to stay consistent
                        MergeInModel(_draggingItem.ItemId, _draggingItem.Amount);
                    }
                    else
                    {
                        // Swap views
                        targetSlot.Detach();
                        targetSlot.Attach(_draggingItem);
                        _dragOriginSlot.Attach(other);
                    }
                }

                _draggingItem.transform.localPosition = Vector3.zero;
                _draggingItem = null;
                _dragOriginSlot = null;
            }
        }
    }

    private void MergeInModel(int id, int amountAdded)
    {
        // Keeps the “data list” in sync after UI stacking.
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == id)
            {
                items[i] = new ItemStack(id, items[i].amount + amountAdded);
                return;
            }
        }
        items.Add(new ItemStack(id, amountAdded));
    }

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
