using UnityEngine;

public class SawCounterGameBridge : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SawRigController sawRig;

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged.AddListener(OnStateChanged);
            gameManager.OnRoundEnded.AddListener(OnRoundEnded);
        }

        if (sawRig != null)
        {
            sawRig.ResetToStart();
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged.RemoveListener(OnStateChanged);
            gameManager.OnRoundEnded.RemoveListener(OnRoundEnded);
        }
    }

    private void OnStateChanged()
    {
        if (gameManager == null || sawRig == null)
        {
            return;
        }

        // Начало нового раунда: у обоих по 2 стартовые карты.
        bool freshRound = gameManager.GetPlayer().Hand.Count == 2 && gameManager.GetOpponent().Hand.Count == 2;
        if (freshRound)
        {
            sawRig.ResetToStart();
        }
    }

    private void OnRoundEnded(string result)
    {
        if (gameManager == null || sawRig == null)
        {
            return;
        }

        PlayerState player = gameManager.GetPlayer();
        PlayerState opponent = gameManager.GetOpponent();

        if (result.StartsWith("Player wins"))
        {
            float lossDistance = gameManager.GetEffectiveBetDistance(opponent);
            sawRig.MoveTowardOpponent(lossDistance);
            return;
        }

        if (result.StartsWith("Opponent wins"))
        {
            float lossDistance = gameManager.GetEffectiveBetDistance(player);
            sawRig.MoveTowardPlayer(lossDistance);
        }
    }
}
