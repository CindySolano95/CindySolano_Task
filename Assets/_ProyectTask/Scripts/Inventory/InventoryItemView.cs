using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image iconImage;          // Assign to: ItemImg
    [SerializeField] private Image glowImage;          // Optional
    [SerializeField] private TextMeshProUGUI amountText; // Assign to: Number

    [Header("Tooltip Refs")]
    [SerializeField] private Vector3 tooltipOffset = new(240f, 30f, 0f);
    public int ItemId { get; private set; }
    public int Amount { get; private set; }
    public bool IsStackable { get; private set; }

    private InventoryDatabase.ItemDefinition _def;
    private InventoryTooltip _tooltip; // referencia compartida

    // Se llama cuando instancias el prefab
    public void Initialize(InventoryTooltip tooltip)
    {
        _tooltip = tooltip;
    }

    public void Bind(InventoryDatabase.ItemDefinition definition, int amount)
    {
        _def = definition;
        ItemId = definition.id;
        IsStackable = definition.stackable;
        SetAmount(amount);

        if (iconImage != null) iconImage.sprite = definition.icon;
        if (iconImage != null) iconImage.enabled = definition.icon != null;
    }

    public void SetAmount(int amount)
    {
        Amount = Mathf.Max(0, amount);
        if (amountText != null)
            amountText.text = Amount.ToString();

        // If not stackable, you can hide amount text if you want.
        amountText.gameObject.SetActive(IsStackable);
    }

    public void SetGlow(bool enabled)
    {
        if (glowImage != null) glowImage.enabled = enabled;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_def == null || _tooltip == null) return;

        var pos = transform.position + tooltipOffset;
        _tooltip.Show(_def.displayName, _def.description, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip == null) return;
        _tooltip.Hide();
    }
}
