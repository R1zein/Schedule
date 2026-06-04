using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float animationDurration;
    [SerializeField] private Image image;
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
        float angle = 180 * progress;
        image.transform.localRotation = Quaternion.Euler(0, 0, angle);
        image.transform.localPosition = Vector3.right*(-30+60*progress);
        Vector2 elementScreenPos = RectTransformUtility.WorldToScreenPoint(
            null,
            image.transform.position
        );
        var dis = Vector2.Distance(Input.mousePosition, elementScreenPos);
        if (dis > 300)
        {
            image.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            image.color = new Color(1, 1, 1, 1);
        }

    }

    public void Toggle()
    {
        speed = -speed;
    }
}
