using System;

[Serializable]
public class Card
{
    public int Value { get; private set; }
    public bool IsFaceUp { get; private set; }

    public Card(int value, bool isFaceUp)
    {
        Value = value;
        IsFaceUp = isFaceUp;
    }

    public void Reveal()
    {
        IsFaceUp = true;
    }
}
