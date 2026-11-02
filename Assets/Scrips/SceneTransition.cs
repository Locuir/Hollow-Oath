
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    private void Start()
    {
        fadePanel.alpha = 1f;
        StartCoroutine(Fade(1f, 0f));
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;

        fadePanel.alpha = start;
        fadePanel.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadePanel.alpha = Mathf.Lerp(
                start,
                end,
                elapsed / fadeDuration
            );

            yield return null;
        }

        fadePanel.alpha = end;
        fadePanel.blocksRaycasts = end >= 1f;
    }
}
