using UnityEngine;
using UnityEngine.UI;

public class TrumpCardView : MonoBehaviour
{
    [SerializeField] private Image trumpImage;

    public void Initialize(TrumpCard trump, CardArtLibrary art)
    {
        if (trumpImage == null || art == null || trump == null)
        {
            return;
        }

        trumpImage.sprite = art.GetTrumpSprite(trump);
    }
}
