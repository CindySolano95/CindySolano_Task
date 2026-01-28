using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image background;

    private void Awake()
    {
        Hide();

        var cg = root.GetComponent<CanvasGroup>();
        if (cg == null) cg = root.gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.ignoreParentGroups = true;

        // Si tu tooltip a veces viene con cosas deshabilitadas:
        if (background != null) background.enabled = true;
        if (titleText != null) titleText.enabled = true;
        if (bodyText != null) bodyText.enabled = true;
    }

    public void Show(string title, string body, Vector3 worldPosition)
    {
        if (root == null) return;

        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;

        root.position = worldPosition;
        root.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }
}
