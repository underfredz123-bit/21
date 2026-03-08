using UnityEngine;

public class WorldArtBinder : MonoBehaviour
{
    [SerializeField] private WorldArtLibrary artLibrary;
    [SerializeField] private SawRigController sawRig;
    [SerializeField] private CounterGaugeController counterGauge;

    private void Start()
    {
        Apply();
    }

    [ContextMenu("Apply World Art")]
    public void Apply()
    {
        if (artLibrary == null)
        {
            return;
        }

        if (sawRig != null)
        {
            sawRig.SetVisuals(artLibrary.GetSawBladeSprite(), artLibrary.GetSawHandSprite());
        }

        if (counterGauge != null)
        {
            counterGauge.SetVisuals(artLibrary.GetCounterBodySprite(), artLibrary.GetCounterNeedleSprite());
        }
    }
}
