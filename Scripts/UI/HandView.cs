using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private bool hideFaceDownCards;

    [Header("Layout")]
    [SerializeField] private float cardSpacing = 96f;
    [SerializeField] private float fanCurveHeight = 12f;
    [SerializeField] private float selectedLift = 22f;
    [SerializeField] private float hoverLift = 14f;
    [SerializeField] private float layoutLerpSpeed = 14f;
    [SerializeField] private bool autoClampSpacing = true;
    [SerializeField] private float maxHandWidth = 900f;

    private readonly List<CardView> _spawned = new List<CardView>();
    private readonly List<Card> _cards = new List<Card>();

    private int _selectedIndex = -1;
    private int _hoveredIndex = -1;
    private bool _cardInteractable = true;

    public event Action<int, Card> CardClicked;

    public int SelectedIndex => _selectedIndex;

    private void Update()
    {
        AnimateLayout(Time.deltaTime);
    }

    public void Refresh(List<Card> cards, CardArtLibrary art, bool animateFlip)
    {
        if (cardPrefab == null || container == null)
        {
            return;
        }

        EnsurePool(cards.Count);

        _cards.Clear();
        _cards.AddRange(cards);

        for (int i = 0; i < _spawned.Count; i++)
        {
            bool active = i < cards.Count;
            _spawned[i].gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            Card card = cards[i];
            bool forceFaceDown = hideFaceDownCards && !card.IsFaceUp;
            _spawned[i].Bind(card, art, forceFaceDown);
            _spawned[i].SetFaceUp(!forceFaceDown && card.IsFaceUp);
            _spawned[i].SetSelected(i == _selectedIndex);

            if (animateFlip)
            {
                _spawned[i].RefreshAnimated();
            }
            else
            {
                _spawned[i].RefreshImmediate();
            }
        }

        if (_selectedIndex >= cards.Count)
        {
            _selectedIndex = -1;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i].gameObject);
            }
        }

        _spawned.Clear();
        _cards.Clear();
        _selectedIndex = -1;
        _hoveredIndex = -1;
    }

    public void ClearSelection()
    {
        _selectedIndex = -1;
        for (int i = 0; i < _spawned.Count; i++)
        {
            _spawned[i].SetSelected(false);
        }
    }

    public Card GetSelectedCard()
    {
        return _selectedIndex >= 0 && _selectedIndex < _cards.Count ? _cards[_selectedIndex] : null;
    }

    public Vector3 GetCardWorldPosition(int index)
    {
        if (index < 0 || index >= _spawned.Count || !_spawned[index].gameObject.activeSelf)
        {
            return container.position;
        }

        return _spawned[index].transform.position;
    }

    public void SetCardInteractable(bool value)
    {
        _cardInteractable = value;
    }

    private void EnsurePool(int needed)
    {
        while (_spawned.Count < needed)
        {
            CardView view = Instantiate(cardPrefab, container);
            int index = _spawned.Count;
            view.Clicked += OnCardClicked;
            view.HoverChanged += OnCardHoverChanged;
            view.gameObject.name = $"CardView_{index}";
            _spawned.Add(view);
        }
    }

    private void OnCardClicked(CardView view)
    {
        if (!_cardInteractable)
        {
            return;
        }

        int index = _spawned.IndexOf(view);
        if (index < 0 || index >= _cards.Count)
        {
            return;
        }

        _selectedIndex = _selectedIndex == index ? -1 : index;
        for (int i = 0; i < _cards.Count; i++)
        {
            _spawned[i].SetSelected(i == _selectedIndex);
        }

        if (_selectedIndex >= 0)
        {
            CardClicked?.Invoke(_selectedIndex, _cards[_selectedIndex]);
        }
    }

    private void OnCardHoverChanged(CardView view, bool isHover)
    {
        int index = _spawned.IndexOf(view);
        if (index < 0)
        {
            return;
        }

        if (isHover)
        {
            _hoveredIndex = index;
        }
        else if (_hoveredIndex == index)
        {
            _hoveredIndex = -1;
        }
    }

    private void AnimateLayout(float dt)
    {
        int count = _cards.Count;
        if (count == 0)
        {
            return;
        }

        float effectiveSpacing = cardSpacing;
        if (autoClampSpacing && count > 1)
        {
            float maxSpacing = maxHandWidth / (count - 1);
            effectiveSpacing = Mathf.Min(cardSpacing, maxSpacing);
        }

        float totalWidth = (count - 1) * effectiveSpacing;
        for (int i = 0; i < count; i++)
        {
            RectTransform cardRect = _spawned[i].transform as RectTransform;
            if (cardRect == null)
            {
                continue;
            }

            float x = -totalWidth * 0.5f + i * effectiveSpacing;
            float normalized = count == 1 ? 0f : (i / (float)(count - 1)) * 2f - 1f;
            float y = -Mathf.Abs(normalized) * fanCurveHeight;

            if (i == _hoveredIndex)
            {
                y += hoverLift;
            }

            if (i == _selectedIndex)
            {
                y += selectedLift;
            }

            Vector2 target = new Vector2(x, y);
            cardRect.anchoredPosition = Vector2.Lerp(cardRect.anchoredPosition, target, dt * layoutLerpSpeed);
        }
    }
}
