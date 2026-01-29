using UnityEngine;
using TMPro;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float showSeconds = 2f;

    private float _hideAt;

    private void Awake()
    {
        if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(true);
        //if (text != null) text.gameObject.SetActive(false);
    }

    private void Update()
    {
        //if (text == null) return;
        //if (text.gameObject.activeSelf && Time.time >= _hideAt)
        //{
        //    if (text.transform.parent != null)
        //    {
        //        text.transform.parent.gameObject.SetActive(false);
        //    }
        //}
    }

    public void Show(string message)
    {
        if (text == null)
        {
            Debug.LogWarning("[FloatingTextSpawner] TMP_Text is missing.");
            return;
        }

        text.text = message;
        text.gameObject.SetActive(true); 
        if (text.transform.parent != null)
        {
            text.transform.parent.gameObject.SetActive(true);
        }
        _hideAt = Time.time + showSeconds;
    }
}
