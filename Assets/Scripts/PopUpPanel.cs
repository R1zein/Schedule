using UnityEngine;

public class PopUpPanel : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float animationDurration;
    private float distance;
    private float progress;
    private float speed = 1;
    private float lerp;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        distance = rect.rect.width;
    }

    private void Update()
    {
        progress += 1 / animationDurration * Time.deltaTime * speed;
        progress = Mathf.Clamp01(progress);
        lerp = Mathf.Lerp(0, -distance, curve.Evaluate(progress));
        rect.anchoredPosition = new Vector2(lerp, rect.anchoredPosition.y);
    }

    public void Toggle()
    {
        speed = -speed;
    }
}
