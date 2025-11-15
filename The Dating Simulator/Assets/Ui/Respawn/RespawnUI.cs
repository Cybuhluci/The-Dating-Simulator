using System.Collections;
using UnityEngine;

public class RespawnUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 1f;

    public float FadeDuration => fadeDuration;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Enter()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f));
    }

    public void Exit()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = (targetAlpha > 0.9f);
        canvasGroup.interactable = (targetAlpha > 0.9f);
    }
}
