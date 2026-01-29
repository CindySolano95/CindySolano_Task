using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogSystemView : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject root; // Assign the DialogSystem root panel
    [SerializeField] private Image npcImage;  // DialogSystem/NPCImage
    [SerializeField] private TMP_Text text;   // DialogSystem/TextBG/Text
    [SerializeField] private Button okBtn;    // DialogSystem/ButtonSelector/OkBtn
    [SerializeField] private Button laterBtn; // DialogSystem/ButtonSelector/LaterBtn

    private Action _onOk;
    private Action _onLater;

    private void Awake()
    {
        okBtn.onClick.AddListener(() => _onOk?.Invoke());
        laterBtn.onClick.AddListener(() => _onLater?.Invoke());
        Hide();
    }

    public void Show(Sprite npcSprite, string line, Action onOk, Action onLater)
    {
        _onOk = onOk;
        _onLater = onLater;

        if (npcImage) npcImage.sprite = npcSprite;
        if (text) text.text = line;

        if (root) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        _onOk = null;
        _onLater = null;

        if (root) root.SetActive(false);
        else gameObject.SetActive(false);
    }
}
