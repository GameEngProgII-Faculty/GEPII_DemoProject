using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Replaces the old USS ".menu-buttons:hover" rule (color change + scale: 1.25 1.25, 0.1s transition)
// now that menu buttons are plain UGUI Buttons instead of UI Toolkit elements.
[RequireComponent(typeof(Button))]
public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.25f;
    [SerializeField] private float transitionDuration = 0.1f;
    [SerializeField] private Color normalTextColor = new Color(0.6902f, 0.6902f, 0.6902f); // rgb(176,176,176)
    [SerializeField] private Color hoverTextColor = new Color(0.0157f, 1f, 0f);             // rgb(4,255,0)
    [SerializeField] private TMP_Text label;

    private Vector3 baseScale;
    private Coroutine activeTransition;

    private void Awake()
    {
        baseScale = transform.localScale;
        label ??= GetComponentInChildren<TMP_Text>();
    }

    public void OnPointerEnter(PointerEventData eventData) => StartTransition(hoverScale, hoverTextColor);

    public void OnPointerExit(PointerEventData eventData) => StartTransition(1f, normalTextColor);

    private void StartTransition(float targetScaleMultiplier, Color targetColor)
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(targetScaleMultiplier, targetColor));
    }

    private IEnumerator TransitionRoutine(float targetScaleMultiplier, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = baseScale * targetScaleMultiplier;
        Color startColor = label != null ? label.color : Color.white;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            if (label != null) label.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        transform.localScale = targetScale;
        if (label != null) label.color = targetColor;
    }
}
