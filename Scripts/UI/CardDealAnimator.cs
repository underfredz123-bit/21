using System.Collections;
using UnityEngine;

public class CardDealAnimator : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RectTransform animationRoot;
    [SerializeField] private float flyDuration = 0.28f;
    [SerializeField] private float arcHeight = 30f;

    public void AnimateFromDeck(Card card, CardArtLibrary artLibrary, RectTransform deckOrigin, RectTransform target, bool forceFaceDown)
    {
        if (cardPrefab == null || animationRoot == null || deckOrigin == null || target == null || card == null || artLibrary == null)
        {
            return;
        }

        CardView temp = Instantiate(cardPrefab, animationRoot);
        temp.Bind(card, artLibrary, forceFaceDown);
        temp.SetFaceUp(!forceFaceDown);
        temp.SetSelected(false);

        RectTransform rect = temp.transform as RectTransform;
        if (rect == null)
        {
            Destroy(temp.gameObject);
            return;
        }

        Vector2 start = animationRoot.InverseTransformPoint(deckOrigin.position);
        Vector2 end = animationRoot.InverseTransformPoint(target.position);
        rect.anchoredPosition = start;

        StartCoroutine(FlyRoutine(rect, start, end));
    }

    private IEnumerator FlyRoutine(RectTransform rect, Vector2 start, Vector2 end)
    {
        float timer = 0f;
        while (timer < flyDuration)
        {
            float t = timer / flyDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            Vector2 pos = Vector2.Lerp(start, end, eased);
            pos.y += Mathf.Sin(eased * Mathf.PI) * arcHeight;
            rect.anchoredPosition = pos;

            timer += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = end;
        Destroy(rect.gameObject);
    }
}
