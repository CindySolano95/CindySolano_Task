using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class InventoryPanelAnimator : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool startHidden = true;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.20f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.15f;

    [Header("Interaction While Visible")]
    [SerializeField] private bool blockRaycastsWhenVisible = true;
    [SerializeField] private bool interactableWhenVisible = true;

    private CanvasGroup _group;
    private Coroutine _routine;
    private bool _isVisible;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();

        if (startHidden)
        {
            SetInstantHidden();
        }
        else
        {
            SetInstantVisible();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    // ---- Public API (hook these in Buttons) ----

    public void Show()
    {
        if (_isVisible) return;
        StartFade(targetVisible: true);
    }

    public void Hide()
    {
        if (!_isVisible) return;
        StartFade(targetVisible: false);
    }

    public void Toggle()
    {
        if (_isVisible) Hide();
        else Show();
    }

    // ---- Internals ----

    private void StartFade(bool targetVisible)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeRoutine(targetVisible));
    }

    private IEnumerator FadeRoutine(bool makeVisible)
    {
        if (makeVisible)
        {
            // Enable object first so it can be seen while fading in
            inventoryPanel.SetActive(true);
            ApplyInteractionState(visible: true);

            yield return FadeAlpha(from: 0f, to: 1f, duration: fadeInDuration);

            _isVisible = true;
            _routine = null;
            yield break;
        }

        // Fade out first, then disable the whole object
        ApplyInteractionState(visible: false); // optional: stop clicks while fading out
        yield return FadeAlpha(from: _group.alpha, to: 0f, duration: fadeOutDuration);

        _isVisible = false;
        _routine = null;

        inventoryPanel.SetActive(false);
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        _group.alpha = from;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // UI should ignore timescale
            float k = Mathf.Clamp01(t / duration);
            _group.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        _group.alpha = to;
    }

    private void ApplyInteractionState(bool visible)
    {
        // Only block raycasts + interactable when visible, if desired
        _group.blocksRaycasts = visible && blockRaycastsWhenVisible;
        _group.interactable = visible && interactableWhenVisible;
    }

    private void SetInstantHidden()
    {
        _group.alpha = 0f;
        _isVisible = false;
        ApplyInteractionState(visible: false);
        inventoryPanel.SetActive(false);
    }

    private void SetInstantVisible()
    {
        inventoryPanel.SetActive(true);
        _group.alpha = 1f;
        _isVisible = true;
        ApplyInteractionState(visible: true);
    }
}
