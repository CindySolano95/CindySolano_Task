using UnityEngine;
using TMPro;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float showSeconds = 2f;

    private float _hideAt;

    private void Awake()
    {
        if (text != null) text.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (text == null) return;
        if (text.gameObject.activeSelf && Time.time >= _hideAt)
            text.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (text == null) return;
        text.text = message;
        text.gameObject.SetActive(true);
        _hideAt = Time.time + showSeconds;
    }
}
