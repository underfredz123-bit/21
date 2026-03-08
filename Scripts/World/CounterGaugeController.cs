using UnityEngine;
using UnityEngine.UI;

public class CounterGaugeController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image needleImage;

    [Header("Needle")]
    [SerializeField] private RectTransform needlePivot;
    [SerializeField] private float safeAngle = -25f;
    [SerializeField] private float dangerAngle = 210f;
    [SerializeField] private float needleSmooth = 8f;

    [Header("Direction")]
    [SerializeField] private bool reverseDirection = true;

    private float _targetNormalized;
    private float _smoothedNormalized;

    public void SetValue(float normalized)
    {
        _targetNormalized = Mathf.Clamp01(normalized);
    }

    public void SetVisuals(Sprite bodySprite, Sprite needleSprite)
    {
        if (bodyImage != null)
        {
            bodyImage.sprite = bodySprite;
        }

        if (needleImage != null)
        {
            needleImage.sprite = needleSprite;
        }
    }

    private void Update()
    {
        _smoothedNormalized = Mathf.Lerp(_smoothedNormalized, _targetNormalized, Time.deltaTime * needleSmooth);

        if (needlePivot == null)
        {
            return;
        }

        float t = reverseDirection ? 1f - _smoothedNormalized : _smoothedNormalized;
        float angle = Mathf.Lerp(safeAngle, dangerAngle, t);
        needlePivot.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
