using System.Collections.Generic;
using System.Linq;

public class PlayerState
{
    public string Name { get; }
    public List<Card> Hand { get; } = new List<Card>();
    public List<TrumpCard> Trumps { get; } = new List<TrumpCard>();
    public bool HasPassed { get; set; }
    public Card LastDrawnCard { get; set; }
    public int BetModifier { get; set; }
    public bool BlessActive { get; set; }

    public PlayerState(string name)
    {
        Name = name;
    }

    public void Reset()
    {
        Hand.Clear();
        Trumps.Clear();
        HasPassed = false;
        LastDrawnCard = null;
        BetModifier = 0;
        BlessActive = false;
    }

    public void AddCard(Card card)
    {
        Hand.Add(card);
        LastDrawnCard = card;
    }

    public void RemoveCard(Card card)
    {
        Hand.Remove(card);
        if (LastDrawnCard == card)
        {
            LastDrawnCard = Hand.LastOrDefault();
        }
    }

    public int TotalVisible()
    {
        return Hand.Where(card => card.IsFaceUp).Sum(card => card.Value);
    }

    public int TotalAll()
    {
        return Hand.Sum(card => card.Value);
    }
}
