using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartelEliminacion : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private TextMeshProUGUI amountLabel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button cancelButton;

    private Action<int> _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();

        if (continueButton != null) continueButton.onClick.AddListener(Confirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(Cancel);

        if (amountSlider != null)
            amountSlider.onValueChanged.AddListener(_ => RefreshLabel());
    }

    public void Show(int maxAmount, Action<int> onConfirm, Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        root.SetActive(true);

        if (amountSlider != null)
        {
            amountSlider.wholeNumbers = true;
            amountSlider.minValue = 1;
            amountSlider.maxValue = Mathf.Max(1, maxAmount);
            amountSlider.value = Mathf.Clamp(amountSlider.value, 1, maxAmount);
        }

        RefreshLabel();
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        _onConfirm = null;
        _onCancel = null;
    }

    private void RefreshLabel()
    {
        if (amountLabel == null || amountSlider == null) return;
        amountLabel.text = $"{(int)amountSlider.value}";
    }

    private void Confirm()
    {
        int amount = 1;
        if (amountSlider != null) amount = Mathf.Max(1, (int)amountSlider.value);

        _onConfirm?.Invoke(amount);
        Hide();
    }

    private void Cancel()
    {
        _onCancel?.Invoke();
        Hide();
    }
}
