using UnityEngine;

[CreateAssetMenu(menuName = "Card Game/Environment Art Library")]
public class EnvironmentArtLibrary : ScriptableObject
{
    [SerializeField] private string environmentFolder = "Environment";

    [Header("Fallback")]
    [SerializeField] private Sprite fallbackBackground;
    [SerializeField] private Sprite fallbackTable;

    public Sprite GetBackgroundSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{environmentFolder}/background");
        return sprite != null ? sprite : fallbackBackground;
    }

    public Sprite GetTableSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"{environmentFolder}/table");
        return sprite != null ? sprite : fallbackTable;
    }
}
