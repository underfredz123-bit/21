using UnityEngine;
using UnityEngine.UI;

public class HorrorFlicker : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Color baseColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color flickerColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flickerSpeed = 2.5f;
    [SerializeField] private float flickerAmount = 0.4f;

    private void Update()
    {
        if (targetImage == null)
        {
            return;
        }

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        float t = Mathf.Clamp01((noise - 0.5f) * 2f * flickerAmount + 0.5f);
        targetImage.color = Color.Lerp(baseColor, flickerColor, t);
    }
}
