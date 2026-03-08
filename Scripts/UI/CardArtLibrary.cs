using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Game/Card Art Library")]
public class CardArtLibrary : ScriptableObject
{
    [Header("Resources folders")]
    [SerializeField] private string cardFolder = "Cards";
    [SerializeField] private string trumpFolder = "Trumps";

    [Header("Fallback sprites")]
    [SerializeField] private Sprite fallbackCardSprite;
    [SerializeField] private Sprite fallbackBackSprite;
    [SerializeField] private Sprite fallbackTrumpSprite;

    public Sprite GetCardSprite(int value)
    {
        Sprite sprite = Resources.Load<Sprite>($"{cardFolder}/card_{value}");
        return sprite != null ? sprite : fallbackCardSprite;
    }

    public Sprite GetCardBackSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{cardFolder}/card_back");
        return sprite != null ? sprite : fallbackBackSprite;
    }

    public Sprite GetTrumpSprite(TrumpCard trump)
    {
        if (trump == null)
        {
            return fallbackTrumpSprite;
        }

        string effectName = ToSnakeCase(trump.EffectType.ToString());
        Sprite sprite = Resources.Load<Sprite>($"{trumpFolder}/trump_{effectName}");
        return sprite != null ? sprite : fallbackTrumpSprite;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (char.IsUpper(current) && i > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }
}
