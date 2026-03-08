using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePlayAreaView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RectTransform playAreaRoot;
    [SerializeField] private RectTransform playerOrigin;
    [SerializeField] private RectTransform opponentOrigin;
    [SerializeField] private float flyDuration = 0.28f;
    [SerializeField] private float spreadRadius = 54f;

    private readonly List<CardView> _tableCards = new List<CardView>();

    public void AnimateCardToTable(Card card, CardArtLibrary artLibrary, bool fromPlayer, bool forceFaceDown)
    {
        if (cardPrefab == null || playAreaRoot == null)
        {
            return;
        }

        CardView view = Instantiate(cardPrefab, playAreaRoot);
        view.Bind(card, artLibrary, forceFaceDown);
        view.SetFaceUp(!forceFaceDown);
        view.SetSelected(false);

        RectTransform rect = view.transform as RectTransform;
        if (rect == null)
        {
            return;
        }

        Vector2 start = GetStartPoint(fromPlayer);
        Vector2 end = GetRandomTablePoint();
        float randomRot = Random.Range(-8f, 8f);

        rect.anchoredPosition = start;
        rect.localRotation = Quaternion.identity;

        _tableCards.Add(view);
        StartCoroutine(FlyRoutine(rect, start, end, randomRot));
    }

    public void ClearTable()
    {
        for (int i = 0; i < _tableCards.Count; i++)
        {
            if (_tableCards[i] != null)
            {
                Destroy(_tableCards[i].gameObject);
            }
        }

        _tableCards.Clear();
    }

    private Vector2 GetStartPoint(bool fromPlayer)
    {
        RectTransform origin = fromPlayer ? playerOrigin : opponentOrigin;
        if (origin == null)
        {
            return fromPlayer ? new Vector2(0f, -260f) : new Vector2(0f, 260f);
        }

        return playAreaRoot.InverseTransformPoint(origin.position);
    }

    private Vector2 GetRandomTablePoint()
    {
        float x = Random.Range(-spreadRadius, spreadRadius);
        float y = Random.Range(-spreadRadius * 0.7f, spreadRadius * 0.7f);
        return new Vector2(x, y);
    }

    private IEnumerator FlyRoutine(RectTransform target, Vector2 start, Vector2 end, float endRotation)
    {
        float timer = 0f;
        while (timer < flyDuration)
        {
            float t = timer / flyDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            target.anchoredPosition = Vector2.Lerp(start, end, eased);
            target.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, endRotation, eased));
            timer += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = end;
        target.localRotation = Quaternion.Euler(0f, 0f, endRotation);
    }
}
