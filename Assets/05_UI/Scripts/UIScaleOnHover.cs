using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger), typeof(RectTransform))]
public class UIScaleOnHover : MonoBehaviour
{
    [Header("Scale hover Settings")]
    [SerializeField] private float scaleDuration;
    [SerializeField] private float scaleMultiplier;

    private EventTrigger eventTrigger;
    private RectTransform rectTransform;

    private Vector2 baseScale;
    private Vector2 hoverScale;

    private void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
        rectTransform = (RectTransform)transform;

        EventTrigger.Entry pointerEnterTrigger = new() { eventID = EventTriggerType.PointerEnter };
        pointerEnterTrigger.callback.AddListener((eventData) => ScaleUp());
        eventTrigger.triggers.Add(pointerEnterTrigger);

        EventTrigger.Entry pointerExitTrigger = new() { eventID = EventTriggerType.PointerExit };
        pointerExitTrigger.callback.AddListener((eventData) => ScaleDown());
        eventTrigger.triggers.Add(pointerExitTrigger);

        baseScale = rectTransform.sizeDelta;
        hoverScale = rectTransform.sizeDelta * scaleMultiplier;
    }

    public void ScaleUp()
    {
        StartCoroutine(ScalingUI(hoverScale));
    }

    public void ScaleDown()
    {
        StartCoroutine(ScalingUI(baseScale));
    }

    private IEnumerator ScalingUI(Vector2 targetScale)
    {
        float elapsedTime = 0;
        Vector2 currentScale = rectTransform.sizeDelta;

        while (elapsedTime <= scaleDuration)
        {
            Vector2 newScale = Vector2.Lerp(currentScale, targetScale, Mathf.Pow(elapsedTime / scaleDuration, 2));

            rectTransform.sizeDelta = newScale;

            yield return null;

            elapsedTime += Time.deltaTime;
        }
    }
}
