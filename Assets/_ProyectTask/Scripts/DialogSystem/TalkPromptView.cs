using TMPro;
using UnityEngine;

public class TalkPromptView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string message = "Press E to talk")
    {
        if (label != null) label.text = message;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
