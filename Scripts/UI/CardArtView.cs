using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Single image setup")]
    [SerializeField] private Image cardImage;

    [Header("Selection")]
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.85f, 1f);

    private Card _card;
    private CardArtLibrary _artLibrary;
    private bool _forceFaceDown;
    private bool _selected;

    public event Action<CardView> Clicked;
    public event Action<CardView, bool> HoverChanged;

    public Card BoundCard => _card;

    public void Bind(Card card, CardArtLibrary artLibrary, bool forceFaceDown)
    {
        _card = card;
        _artLibrary = artLibrary;
        _forceFaceDown = forceFaceDown;
        Refresh();
    }

    public void Bind(Card card)
    {
        _card = card;
        Refresh();
    }

    public void Initialize(Card card, CardArtLibrary artLibrary, bool forceFaceDown)
    {
        Bind(card, artLibrary, forceFaceDown);
    }

    public void Refresh()
    {
        if (cardImage == null || _card == null || _artLibrary == null)
        {
            return;
        }

        bool showFaceUp = !_forceFaceDown && _card.IsFaceUp;
        cardImage.sprite = showFaceUp
            ? _artLibrary.GetCardSprite(_card.Value)
            : _artLibrary.GetCardBackSprite();

        cardImage.color = _selected ? selectedColor : normalColor;
        transform.localScale = _selected ? Vector3.one * selectedScale : Vector3.one;
    }

    public void RefreshImmediate()
    {
        Refresh();
    }

    // Left for compatibility with existing callers (no extra animation in simplified view).
    public void RefreshAnimated()
    {
        Refresh();
    }

    public void SetFaceUp(bool faceUp)
    {
        _forceFaceDown = !faceUp;
        Refresh();
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        Refresh();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverChanged?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverChanged?.Invoke(this, false);
    }
}
