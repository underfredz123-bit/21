using UnityEngine;

[CreateAssetMenu(menuName = "Card Game/World Art Library")]
public class WorldArtLibrary : ScriptableObject
{
    [SerializeField] private string worldFolder = "World";

    [Header("Fallback")]
    [SerializeField] private Sprite fallbackSawBlade;
    [SerializeField] private Sprite fallbackSawHand;
    [SerializeField] private Sprite fallbackCounterBody;
    [SerializeField] private Sprite fallbackCounterNeedle;

    public Sprite GetSawBladeSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{worldFolder}/saw_blade");
        return sprite != null ? sprite : fallbackSawBlade;
    }

    public Sprite GetSawHandSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{worldFolder}/saw_hand");
        return sprite != null ? sprite : fallbackSawHand;
    }

    public Sprite GetCounterBodySprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{worldFolder}/counter_body");
        return sprite != null ? sprite : fallbackCounterBody;
    }

    public Sprite GetCounterNeedleSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{worldFolder}/counter_needle");
        return sprite != null ? sprite : fallbackCounterNeedle;
    }
}
