using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Timing")]
    [SerializeField] private float clickDelay = 0.12f; // tiempo para ver la animación Pressed

    [Header("Scene")]
    [SerializeField] private string nextSceneName;

    private bool isLoading;

    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("[SceneTransitionManager] Fade Image is not assigned.");
            return;
        }

        fadeImage.gameObject.SetActive(true);
        StartCoroutine(Fade(1f, 0f)); // Fade-in al entrar
    }

    public void PlayGame()
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (clickDelay > 0f)
            yield return new WaitForSeconds(clickDelay);

        yield return Fade(0f, 1f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogError("[SceneTransitionManager] nextSceneName is empty.");
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
