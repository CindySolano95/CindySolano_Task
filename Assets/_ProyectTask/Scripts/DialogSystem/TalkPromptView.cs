using TMPro;
using UnityEngine;

public class TalkPromptView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        if (root == null) root = gameObject;
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>(true);

        Hide();
    }

    public void Show(string message = "Press E to talk")
    {
        if (label == null)
        {
            Debug.LogWarning("[TalkPromptView] No TMP_Text found under this object.");
            // At least show the bubble/root so you can see something
            root.SetActive(true);
            return;
        }

        // IMPORTANT: enable root first so TMP can be enabled safely
        root.SetActive(true);

        label.gameObject.SetActive(true);
        label.text = message;

        // Debug
        Debug.Log($"[TalkPromptView] Show: '{message}' (rootActive={root.activeSelf}, labelActive={label.gameObject.activeSelf})");
    }

    public void Hide()
    {
        if (label != null) label.gameObject.SetActive(false);
        if (root != null) root.SetActive(false);
    }
}
